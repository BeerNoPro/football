# Cách Hệ Thống Hoạt Động

## Tổng quan kiến trúc

```
Browser / Telegram
      │
      ▼
FootballBlog.Web  (Blazor)
  SSR pages      ──HttpClient──▶  FootballBlog.API  (ASP.NET Core)
  Admin pages                           │
  Live widget ◀──SignalR──────────      │   (SignalR: Phase 4 TODO)
                                   IPostService
                                   ICategoryService
                                   ILiveScoreService
                                        │
                                   IUnitOfWork
                                   (10 repositories)
                                        │
                                   PostgreSQL  ◀──EF Core──  FootballBlog.Infrastructure
                                   Redis
```

**4 projects:**
- `Web` → gọi `API` qua typed HttpClient (4 clients: `IPostApiClient`, `ICategoryApiClient`, `ITagApiClient`, `IAdminApiClient`)
- `API` → chứa controllers, Hangfire jobs (SignalR hub: chưa tạo)
- `Core` → business logic thuần — không phụ thuộc framework
- `Infrastructure` → EF Core, repositories, external services

---

## 1. Request Flow — Blog (SSR)

```
User gõ URL /bai-viet/man-utd-vs-chelsea
      │
      ▼
Web: PostDetail.razor (Static SSR — không có JS)
      │  IPostApiClient.GetBySlugAsync("man-utd-vs-chelsea")
      ▼
API: GET /api/posts/man-utd-vs-chelsea
      │  [OutputCache 5 phút — cache theo tag "posts"]
      ▼
PostsController.GetBySlug()
      │  IPostService.GetBySlugAsync()
      ▼
PostService
      │  uow.Posts.GetBySlugAsync()  ← chỉ trả post có PublishedAt != null
      ▼
PostRepository (AsNoTracking + Include Category, Author, Tags)
      │
      ▼
PostgreSQL → PostDetailDto → JSON response
      │
      ▼
Web: render HTML đầy đủ (SSR) → trả về browser
      │
      ▼
Google crawler: thấy HTML có nội dung → index SEO ✅
```

**Lưu ý quan trọng:**
- Draft (`PublishedAt == null`) KHÔNG bao giờ trả về từ `GetBySlugAsync`
- Cache 5 phút — sau khi admin publish/edit post, cache tự invalidate qua `EvictByTagAsync("posts")`

---

## 2. Request Flow — Admin Write

```
Admin: POST /api/posts (Authorize Roles="Admin")
      │
      ▼
PostsController.Create()
      │  IPostService.CreateAsync(dto)
      ▼
PostService
      │  SlugService.GenerateUnique() — tiếng Việt → slug latin
      │  uow.Posts.AddAsync(post)
      │  uow.CommitAsync()  ← 1 transaction duy nhất
      ▼
PostgreSQL INSERT
      │
      ▼
cacheStore.EvictByTagAsync("posts")  ← xoá cache ngay
      │
      ▼
201 Created + PostDetailDto
```

---

## 2b. Admin Auth Flow (Cookie + JWT)

```
Admin mở /admin/login
      │
      ▼
Login.razor → POST /api/auth/login (email + password)
      │
      ▼
AuthController
      │  UserManager.CheckPasswordAsync()
      │  Generate JWT token (claims: email, name, roles, 7-day expiry)
      ▼
HTTP 200 + { token: "eyJ..." }
      │
      ▼
Login.razor
      │  Tạo ClaimsPrincipal với claim "jwt_token" = token
      │  HttpContext.SignInAsync(CookieAuth)  ← tạo cookie session 7 ngày
      ▼
Blazor Admin pages (InteractiveServer)
      │
      ▼
AdminPageBase.OnInitializedAsync()
      │  AuthenticationState → lấy claim "jwt_token"
      │  JwtTokenStore.SetToken(token)  ← scoped service, in-memory per circuit
      ▼
IAdminApiClient (AdminApiClient)
      │  JwtAuthHandler (DelegatingHandler)
      │  tự động thêm "Authorization: Bearer {token}" vào mọi request
      ▼
API nhận → JWT middleware validate → cho phép access
```

**Quy tắc:**
- Public pages (Blog/SSR) → dùng `IPostApiClient`, `ICategoryApiClient`, `ITagApiClient` (không cần auth)
- Admin pages → dùng `IAdminApiClient` (auto-inject Bearer token qua `JwtAuthHandler`)
- Cookie auth chỉ dùng cho Blazor session — không phải cho API calls trực tiếp

---

## 3. Typed HTTP Clients (Web Layer)

Tất cả clients đăng ký trong `Web/Program.cs` với `HttpClientFactory`, base address = `ApiBaseUrl` config.

| Interface | Dùng cho | Auth |
|-----------|----------|------|
| `IPostApiClient` | SSR public pages — lấy/tìm bài viết | Không |
| `ICategoryApiClient` | SSR public pages — nav category | Không |
| `ITagApiClient` | SSR public pages — nav tag | Không |
| `IAdminApiClient` | Admin CRUD — posts/categories/tags/media | Bearer JWT via JwtAuthHandler |

**IAdminApiClient methods (Posts):**
```
GetAllPostsAsync(page, pageSize)   → GET /api/posts/all      (kể cả draft)
GetPostByIdAsync(id)               → GET /api/posts/{id:int} (admin, kể cả draft)
CreatePostAsync(dto)               → POST /api/posts
UpdatePostAsync(id, dto)           → PUT /api/posts/{id}
DeletePostAsync(id)                → DELETE /api/posts/{id}
UploadImageAsync(stream, name, ct) → POST /api/media/upload  → trả URL /uploads/xxx.jpg
```

**JwtAuthHandler flow:**
```
AdminApiClient gọi HTTP request
      │
      ▼
JwtAuthHandler.SendAsync()
      │  JwtTokenStore.GetToken()  ← đọc token scoped theo Blazor circuit
      │  request.Headers.Authorization = Bearer {token}
      ▼
HTTP request gửi đến API
```

---

## 3b. Admin Posts CRUD + Media Upload (Phase 3 ✅)

```
Admin mở /admin/posts/create hoặc /admin/posts/edit/{id}
      │  InteractiveServer — dùng IAdminApiClient (Bearer JWT)
      ▼
Trang load categories → ICategoryApiClient.GetAllAsync()
Edit page thêm: AdminClient.GetPostByIdAsync(id) → pre-fill form
      │
      ▼
Quill.js Editor (rich text)
      │  JS interop: QuillInterop.create(elementId, html, dotnetRef)
      │  Khi text thay đổi → JS gọi dotnetRef.OnContentChanged(html)
      │  Component QuillEditor.razor bind Value ↔ _content
      ▼
Upload thumbnail (tùy chọn)
      │  InputFile → UploadImageAsync(stream, name, contentType)
      │  → POST /api/media/upload (Authorize Admin)
      │  → MediaController lưu vào wwwroot/uploads/{guid}.ext (dev)
      │  → trả về URL: "/uploads/abc123.jpg"
      │  → _thumbnail = url → hiển thị preview
      ▼
Bấm "Xuất bản" / "Lưu nháp"
      │  CreatePostDto(title, slug, content, thumbnail, categoryId, authorId, publishNow)
      │  authorId = CurrentUserId (từ JWT claim NameIdentifier trong AdminPageBase)
      ▼
API: POST /api/posts hoặc PUT /api/posts/{id}
      │  [Authorize Roles="Admin"]
      │  IPostService.CreateAsync / UpdateAsync
      │  uow.CommitAsync()
      │  cacheStore.EvictByTagAsync("posts")
      ▼
201/200 → redirect về /admin/posts
```

**Slug generation:** Client-side (JavaScript regex trong Create.razor) — tự động từ tiêu đề.
**Draft vs Published:** `PublishNow = false` → `PublishedAt = null` (không hiển thị public).
**PostSummaryDto.PublishedAt** là `DateTime?` — nullable để hỗ trợ draft.

---

## 4. Live Score Flow (Phase 4 — Partially Implemented)

### 4a. FetchUpcomingMatchesJob (✅ Implemented)

```
[Hangfire Cron: 0 */6 * * *] FetchUpcomingMatchesJob
      │  Check Redis rate limit counter trước
      │  Football API: GET /fixtures?next=20 (cho các leagues được config)
      ▼
Upsert theo thứ tự dependency:
  1. Country (upsert by Code)
  2. League  (upsert by ExternalId)
  3. Team    (upsert by ExternalId)
  4. Match   (upsert by ExternalId)
      │
      ▼
Với mỗi match MỚI → Schedule Hangfire one-time jobs:
  - PreMatchDataJob.FetchH2HAsync()      → H-5h trước kickoff
  - PreMatchDataJob.FetchLineupsAsync()  → H-15min trước kickoff
      │
      ▼
uow.CommitAsync()
```

### 4b. PreMatchDataJob (✅ Implemented)

```
[Hangfire one-time, scheduled by FetchUpcomingMatchesJob]

FetchH2HAsync(fixtureExternalId, homeTeamExternalId, awayTeamExternalId)
  → Chạy 5h trước kickoff
  → Football API: GET /fixtures/headtohead
  → Log kết quả (Phase 5: persist vào MatchContextData.ContextJson)

FetchLineupsAsync(fixtureExternalId)
  → Chạy 15min trước kickoff
  → Football API: GET /fixtures/lineups
  → Log kết quả (Phase 5: persist vào MatchContextData.ContextJson)
```

### 4c. LiveScorePollingJob (✅ Implemented, SignalR TODO)

```
[Hangfire Cron: mỗi phút] LiveScorePollingJob
      │  Adaptive gate: query DB xem có Match status=Live không
      │  Nếu không có live match → skip (tiết kiệm API quota)
      │  Check Redis rate limit counter
      ▼
Football API: GET /fixtures?live=all  (1 request duy nhất)
      │
      ▼
Với mỗi fixture trả về:
  - Upsert LiveMatch (score, minute, status)
  - Upsert MatchEvents (goal/card/sub/penalty)
  - Update parent Match.Status
  - Nếu fixture BIẾN MẤT khỏi response → đánh dấu Finished
      │
      ▼
uow.CommitAsync()
      │
      ▼
[TODO — Phase 4 còn lại]
SignalR Hub: LiveScoreHub.SendUpdateAsync(matchId, dto)
      │  Broadcast đến group "match-{matchId}"
      ▼
Browser: LiveScoreWidget.razor (InteractiveServer)
      │  Subscribe group khi mount, unsubscribe khi unmount
      ▼
UI update realtime — không reload trang
```

**Tại sao không poll từ browser?**
Với 500 user xem cùng lúc = 500 HTTP requests/30s đến Football API → vượt quota ngay.
Pattern đúng: 1 job poll → 1 SignalR broadcast → N clients nhận.

**Rate limit guard:**
Trước mỗi Football API call: `INCR redis "football_api:requests:{date}"` — nếu > 90 thì skip, log warning.

---

## 5. AI Prediction Pipeline (Phase 5 — Chưa implement)

```
[Cron 1h] FetchMatchContextJob  (TODO)
      │  Query: Match WHERE Status=Scheduled AND KickoffUtc <= NOW()+24h AND ContextData IS NULL
      │  Với mỗi match: đọc từ MatchContextData.ContextJson (đã được PreMatchDataJob populate)
      │  Deserialize → MatchContext POCO (H2H, TeamForm, Lineup, Referee, Fatigue)
      ▼
[Cron 1h] GeneratePredictionJob  (TODO)
      │  Query: Match WHERE Prediction IS NULL AND ContextData IS NOT NULL
      │  Build AI prompt từ MatchContext
      ▼
IAIPredictionProvider (abstraction — TODO)
      │
      ├── ClaudeAIPredictionProvider  (primary — claude-opus-4-6)
      └── GeminiAIPredictionProvider  (fallback — gemini-2.0-flash)
      │
      ▼
MatchPrediction { PredictedScore, PredictedOutcome, ConfidenceScore, Analysis }
uow.MatchPredictions.AddAsync()
uow.CommitAsync()
      │
      ▼
[Trigger] PublishPredictionJob  (TODO)
      │  Tạo Post từ prediction (IPostService.CreateAsync)
      │  Gửi Telegram message → lưu TelegramMessageId
      ▼
Sau trận kết thúc: Edit Telegram message với kết quả thực tế
```

**Các entity đã sẵn sàng cho Phase 5:**
- `MatchContextData` — JSONB blob, 1-to-1 với Match (đã migrate)
- `MatchPrediction` — stores AIProvider, AIModel, PredictedOutcome, ConfidenceScore (đã migrate)
- `FixtureRawDto` — raw API response mapping (đã có)

---

## 6. Data Model Relationships

```
Post ──────── Category  (N:1)
Post ──────── ApplicationUser  (N:1, Author)
Post ──────── PostTag ──── Tag  (N:M)
Post ◀──────── MatchPrediction.BlogPost  (1:0..1, sau khi publish)

Match ──────── League  (N:1, FK)
Match ──────── Team  (N:1, HomeTeam)
Match ──────── Team  (N:1, AwayTeam)
Match ──────── MatchPrediction  (1:0..1)
Match ──────── MatchContextData  (1:0..1, JSONB blob)
Match ◀──────── LiveMatch.Match  (1:0..1, khi trận live)

League ──────── Country  (N:1)
Team ──────── Country  (N:1, nullable)

LiveMatch ──────── MatchEvent  (1:N, goal/card/sub/penalty)
```

**IUnitOfWork — 10 repositories:**
```csharp
uow.Posts           // IPostRepository
uow.Categories      // ICategoryRepository
uow.Tags            // ITagRepository
uow.LiveMatches     // ILiveMatchRepository
uow.Matches         // IMatchRepository
uow.MatchPredictions // IMatchPredictionRepository
uow.Countries       // ICountryRepository  — upsert by Code
uow.Leagues         // ILeagueRepository   — upsert by ExternalId
uow.Teams           // ITeamRepository     — upsert by ExternalId
uow.MatchContexts   // IMatchContextRepository — 1-to-1 với Match (JSONB)
```

---

## 7. Blazor Render Mode — Quy tắc

| Trang | Mode | Lý do |
|-------|------|-------|
| Home, Blog list, Bài viết, Category, Tag | **Static SSR** | HTML đầy đủ → Google index → SEO |
| Sitemap.xml, robots.txt | **Static SSR** | Phục vụ crawler |
| Live Score widget | **InteractiveServer** | Cần WebSocket cho realtime (Phase 4) |
| Admin (toàn bộ) | **InteractiveServer** | CRUD form, dialog — không cần SEO |

**Quy tắc bất biến:**
- KHÔNG set `@rendermode` global ở `App.razor`
- SSR page CÓ THỂ chứa InteractiveServer child component (LiveScoreWidget trên Home)
- KHÔNG inject `IHttpContextAccessor` trong component dùng chung cả 2 mode

---

## 8. Caching Strategy

| Layer | Cơ chế | TTL |
|-------|--------|-----|
| API blog GET endpoints | `OutputCache` + tag `"posts"` | 5 phút |
| Football API rate limit | Redis counter `football_api:requests:{date}` | Reset 00:00 UTC |
| SignalR backplane (Phase 4 TODO) | Redis pub/sub | N/A |

Khi admin create/update/delete post → `cacheStore.EvictByTagAsync("posts")` → cache cleared ngay lập tức.

**Các endpoints có OutputCache:**
- `GET /api/posts` — paginated published posts
- `GET /api/posts/{slug}` — post by slug
- `GET /api/posts/by-category/{slug}` — posts by category
- `GET /api/posts/by-tag/{slug}` — posts by tag
- `GET /api/tags` — all tags
- `GET /api/tags/{slug}` — tag by slug
- `GET /api/tags/{slug}/posts` — posts by tag

---

## 9. External Services

| Service | Dùng cho | Config |
|---------|----------|--------|
| **Football API** (api-sports.io) | Match data, live scores, H2H, lineups | `FootballApi:ApiKey` (user-secrets) |
| **PostgreSQL** | Primary database | `ConnectionStrings:DefaultConnection` |
| **Redis** | Rate limiter, future: SignalR backplane | `ConnectionStrings:Redis` |
| **Hangfire** | Background job scheduling (uses PostgreSQL) | Built-in dashboard `/hangfire` |
| **Claude API** | AI prediction — Phase 5 | `Claude:ApiKey` (user-secrets) |
| **Gemini API** | AI prediction fallback — Phase 5 | `Gemini:ApiKey` (user-secrets) |
| **Telegram Bot** | Notification + bot commands — Phase 6 | `Telegram:BotToken` (user-secrets) |
