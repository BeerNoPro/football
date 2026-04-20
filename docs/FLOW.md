# Cách Hệ Thống Hoạt Động — Football Blog

> Tài liệu này mô tả toàn bộ luồng xử lý nghiệp vụ từ source code thực tế.
> Dùng làm context overview khi bắt đầu session mới hoặc onboard developer mới.

---

## Mục lục

- [Cách Hệ Thống Hoạt Động — Football Blog](#cách-hệ-thống-hoạt-động--football-blog)
  - [Mục lục](#mục-lục)
  - [1. Kiến trúc tổng quan](#1-kiến-trúc-tổng-quan)
  - [2. Request Flow — Blog SSR (Public)](#2-request-flow--blog-ssr-public)
    - [2a. Trang chi tiết bài viết](#2a-trang-chi-tiết-bài-viết)
    - [2b. Các public endpoints khác](#2b-các-public-endpoints-khác)
  - [3. Auth Flow — Admin (Cookie + JWT)](#3-auth-flow--admin-cookie--jwt)
  - [4. Admin CRUD — Bài viết \& Media](#4-admin-crud--bài-viết--media)
    - [4a. Tạo / Cập nhật bài viết](#4a-tạo--cập-nhật-bài-viết)
    - [4b. Admin API endpoints (đầy đủ)](#4b-admin-api-endpoints-đầy-đủ)
  - [5. Typed HTTP Clients (Web Layer)](#5-typed-http-clients-web-layer)
  - [6. Live Score — Football API + SignalR](#6-live-score--football-api--signalr)
    - [6a. FetchUpcomingMatchesJob — Đồng bộ lịch đấu](#6a-fetchupcomingmatchesjob--đồng-bộ-lịch-đấu)
    - [6b. PreMatchDataJob — Dữ liệu trước trận](#6b-prematchdatajob--dữ-liệu-trước-trận)
    - [6c. LiveScorePollingJob — Poll real-time + SignalR broadcast](#6c-livescorepollingjob--poll-real-time--signalr-broadcast)
    - [6d. LiveScoreHub — SignalR Server](#6d-livescorehub--signalr-server)
    - [6e. LiveScoreWidget.razor — Blazor Client](#6e-livescorewidgetrazor--blazor-client)
  - [7. AI Prediction Pipeline](#7-ai-prediction-pipeline)
  - [8. Telegram Notification Flow](#8-telegram-notification-flow)
  - [9. Data Model Relationships](#9-data-model-relationships)
  - [10. Blazor Render Mode Rules](#10-blazor-render-mode-rules)
  - [11. Caching Strategy](#11-caching-strategy)
  - [12. Hangfire Background Jobs](#12-hangfire-background-jobs)
  - [13. External Services \& Config](#13-external-services--config)
  - [14. DI Container \& Service Lifetimes](#14-di-container--service-lifetimes)
  - [15. Logging Architecture](#15-logging-architecture)
  - [16. Database Migration](#16-database-migration)
    - [Nguyên tắc](#nguyên-tắc)
    - [Cách 1 — Thủ công (CLI) ← Khuyến nghị cho Production](#cách-1--thủ-công-cli--khuyến-nghị-cho-production)
    - [Cách 2 — Tự động khi startup (Auto-Migrate) ← Dev/Staging](#cách-2--tự-động-khi-startup-auto-migrate--devstaging)
    - [Migration hiện có](#migration-hiện-có)

---

## 1. Kiến trúc tổng quan

```
Browser / Telegram Bot
        │
        ▼
FootballBlog.Web  (Blazor — :7241)
  SSR pages (Blog/SEO)   ──HttpClient──▶   FootballBlog.API  (:7007)
  Admin pages (MudBlazor)                        │
  LiveScore widget ◀──SignalR──────────          │
                                           ┌─────┴──────┐
                                      Services       Hangfire Jobs
                                           │               │
                                      IUnitOfWork   Football API
                                      (14 repos)    Claude/Gemini API
                                           │        Telegram Bot API
                                      PostgreSQL
                                      Redis
```

**4 projects — dependency direction:**

| Project | Vai trò | Phụ thuộc |
|---------|---------|-----------|
| `FootballBlog.Web` | Blazor UI — SSR + InteractiveServer | `Core` (DTOs) |
| `FootballBlog.API` | ASP.NET Core Web API + Hangfire | `Core`, `Infrastructure` |
| `FootballBlog.Core` | Business logic thuần — entities, services, interfaces | Không có |
| `FootballBlog.Infrastructure` | EF Core, repositories, external service clients | `Core` |

**Pattern cốt lõi:**
- Web KHÔNG gọi DB trực tiếp — luôn qua typed HttpClient đến API
- API KHÔNG gọi DbContext trực tiếp — luôn qua `IUnitOfWork`
- Repository chỉ modify ChangeTracker — commit duy nhất qua `uow.CommitAsync()`
- DTOs trong `Core/DTOs/` — service layer không expose entity ra ngoài

---

## 2. Request Flow — Blog SSR (Public)

### 2a. Trang chi tiết bài viết

```
User gõ URL: /bai-viet/man-utd-vs-chelsea
      │
      ▼
Web: PostDetail.razor (Static SSR — không rendermode, SEO-friendly)
      │  IPostApiClient.GetBySlugAsync("man-utd-vs-chelsea")
      ▼
API: GET /api/posts/man-utd-vs-chelsea
      │  [OutputCache PolicyName="BlogPages" — 5 phút, tag="posts"]
      ▼
PostsController.GetBySlug(string slug)
      │  IPostService.GetBySlugAsync(slug)
      ▼
PostService
      │  LogDebug("Getting post by slug {Slug}")
      │  uow.Posts.GetBySlugAsync(slug)
      ▼
PostRepository
      │  AsNoTracking()
      │  .Include(Category).Include(Author).Include(PostTags.Tag)
      │  .Where(p => p.Slug == slug && p.PublishedAt != null)  ← chỉ published
      ▼
PostgreSQL → map to PostDetailDto → JSON response
      │
      ▼
Web: render HTML đầy đủ (Static SSR) → browser + Google crawler
```

**Lưu ý:**
- `PostSummaryDto` (danh sách) **không có** trường `Content` — tránh over-fetching
- `PostDetailDto` có đầy đủ: `Id, Title, Slug, Content, Thumbnail, CategoryName, CategorySlug, AuthorName, PublishedAt, Tags[]`
- Draft (`PublishedAt == null`) **không bao giờ** xuất hiện ở public endpoint
- Cache 5 phút, invalidate ngay khi admin publish/update/delete qua `EvictByTagAsync("posts")`

### 2b. Các public endpoints khác

| Endpoint | Controller Method | Cache | Mô tả |
|----------|------------------|-------|-------|
| `GET /api/posts?page=1&pageSize=10` | `GetAll()` | ✅ BlogPages | Danh sách published, paginated |
| `GET /api/posts/by-category/{slug}` | `GetByCategory()` | ✅ | Lọc theo category |
| `GET /api/posts/by-tag/{slug}` | `GetByTag()` | ✅ | Lọc theo tag |
| `GET /api/categories` | `CategoriesController.GetAll()` | ✅ | Tất cả categories |
| `GET /api/tags` | `TagsController.GetAll()` | ✅ | Tất cả tags |
| `GET /api/livescore` | `LiveScoreController.GetAll()` | ❌ | Live matches hiện tại |
| `GET /api/livescore/{id}` | `LiveScoreController.GetById()` | ❌ | Match cụ thể |

---

## 3. Auth Flow — Admin (Cookie + JWT)

Dùng **2 lớp auth** kết hợp:
- **Cookie Auth** → bảo vệ Blazor admin routes (server-side session)
- **JWT Bearer** → bảo vệ API endpoints (stateless, per-request)

```
Admin mở /admin/login
      │
      ▼
Login.razor (InteractiveServer)
      │  POST /api/auth/login  { email, password }
      │  [EnableRateLimiting("login")] — 5 req/phút/IP
      ▼
AuthController.Login()
      │  userManager.FindByEmailAsync(email)
      │  signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)
      │  userManager.GetRolesAsync(user)
      │  GenerateJwtToken(user, roles)
      │     → claims: NameIdentifier(userId), Email, Name, Role[]
      │     → expires: UtcNow + 7 days
      │     → signed: HmacSha256(Jwt:Key)
      ▼
HTTP 200 + LoginResponseDto { Token, Email, DisplayName, Roles }
      │
      ▼
Login.razor
      │  Tạo ClaimsPrincipal với claim "jwt_token" = token
      │  HttpContext.SignInAsync(CookieAuth)  ← cookie 7 ngày, sliding expiration
      │  NavigationManager.NavigateTo("/admin")
      ▼
Admin page (InteractiveServer) kế thừa AdminPageBase
      │
      ▼
AdminPageBase.OnInitializedAsync()
      │  AuthProvider.GetAuthenticationStateAsync()
      │  TokenStore.Token = user.FindFirst("jwt_token")?.Value  ← scoped per Blazor circuit
      │  CurrentUserId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value)
      │  → OnAdminInitializedAsync()  ← override ở mỗi trang con
      ▼
IAdminApiClient (AdminApiClient)
      │  JwtAuthHandler (DelegatingHandler)
      │  → request.Headers.Authorization = "Bearer {token}"  ← tự động mọi request
      ▼
API: JWT middleware validate → [Authorize(Roles="Admin")] → xử lý
```

**Quy tắc:**
- Cookie chỉ dùng để carry JWT token trong Blazor circuit
- `JwtTokenStore` là Scoped — mỗi Blazor circuit (browser tab) có token riêng
- API validate JWT stateless — không cần session server-side

---

## 4. Admin CRUD — Bài viết & Media

### 4a. Tạo / Cập nhật bài viết

```
Admin mở /admin/posts/create hoặc /admin/posts/edit/{id}
      │  Trang kế thừa AdminPageBase → auto-inject JWT
      ▼
OnAdminInitializedAsync()
      │  ICategoryApiClient.GetAllAsync()  ← dropdown categories
      │  (Edit only) IAdminApiClient.GetPostByIdAsync(id)  ← GET /api/posts/{id:int}
      │                                     [Authorize Admin] — kể cả draft
      ▼
Form UI (MudBlazor)
      │  QuillEditor.razor — rich text editor via JS interop
      │     quill-interop.js: QuillInterop.create(elementId, html, dotnetRef)
      │     dotnetRef.OnContentChanged(html) → Value binding
      │  InputFile — thumbnail upload
      │     UploadImageAsync(stream, fileName, contentType) → POST /api/media/upload
      │     MediaController → lưu wwwroot/uploads/{guid}.ext → trả "/uploads/abc.jpg"
      ▼
Submit → CreatePostDto(Title, Slug, Content, Thumbnail, CategoryId, AuthorId, PublishNow)
      │  AuthorId = CurrentUserId (từ JWT claim NameIdentifier)
      │  PublishNow = true → PublishedAt = UtcNow
      │  PublishNow = false → draft (PublishedAt = null)
      ▼
IAdminApiClient.CreatePostAsync(dto)  → POST /api/posts
IAdminApiClient.UpdatePostAsync(id, dto) → PUT /api/posts/{id}
      │  [Authorize(Roles="Admin")]
      ▼
PostService.CreateAsync / UpdateAsync
      │  uow.Posts.AddAsync / UpdateAsync
      │  uow.CommitAsync()  ← 1 transaction
      │  cacheStore.EvictByTagAsync("posts")  ← invalidate cache ngay
      ▼
201 Created / 200 OK + PostDetailDto → redirect /admin/posts
```

### 4b. Admin API endpoints (đầy đủ)

| Method | Endpoint | Auth | Mô tả |
|--------|----------|------|-------|
| `GET` | `/api/posts/all?page&pageSize` | Admin | Tất cả posts kể cả draft |
| `GET` | `/api/posts/{id:int}` | Admin | Post by ID (kể cả draft) |
| `POST` | `/api/posts` | Admin | Tạo bài viết mới |
| `PUT` | `/api/posts/{id}` | Admin | Cập nhật bài viết |
| `DELETE` | `/api/posts/{id}` | Admin | Xóa bài viết |
| `POST` | `/api/media/upload` | Admin | Upload ảnh → trả URL |

---

## 5. Typed HTTP Clients (Web Layer)

Tất cả clients đăng ký trong `Web/Program.cs` với `IHttpClientFactory`, `BaseAddress = ApiBaseUrl`.

```
Web/Program.cs:
  AddHttpClient<IPostApiClient, PostApiClient>(BaseAddress=apiBaseUrl)
  AddHttpClient<ICategoryApiClient, CategoryApiClient>(...)
  AddHttpClient<ITagApiClient, TagApiClient>(...)
  AddHttpClient<ILiveScoreApiClient, LiveScoreApiClient>(...)
  AddHttpClient<IAdminApiClient, AdminApiClient>(...).AddHttpMessageHandler<JwtAuthHandler>()
```

| Interface | Implementation | Auth | Endpoint prefix |
|-----------|---------------|------|----------------|
| `IPostApiClient` | `PostApiClient` | Không | `api/posts` |
| `ICategoryApiClient` | `CategoryApiClient` | Không | `api/categories` |
| `ITagApiClient` | `TagApiClient` | Không | `api/tags` |
| `ILiveScoreApiClient` | `LiveScoreApiClient` | Không | `api/livescore` |
| `IAdminApiClient` | `AdminApiClient` | Bearer JWT (tự động) | `api/posts`, `api/categories`, `api/tags`, `api/media` |

**JwtAuthHandler flow:**
```
AdminApiClient gọi HTTP request
      │
      ▼
JwtAuthHandler.SendAsync()
      │  token = JwtTokenStore.Token  ← Scoped per Blazor circuit
      │  if token != null: request.Headers.Authorization = "Bearer {token}"
      ▼
HTTP request gửi đến API → JWT middleware validate
```

**API Response wrapper** dùng thống nhất:
```csharp
record ApiResponse<T>(bool Success, T? Data, string? Error = null)
```
Client tự unwrap → `response?.Data` — null nếu `Success = false` hoặc exception.

---

## 6. Live Score — Football API + SignalR

### 6a. FetchUpcomingMatchesJob — Đồng bộ lịch đấu

**Cron:** `0 */6 * * *` (mỗi 6 tiếng)

```
[Hangfire] FetchUpcomingMatchesJob.ExecuteAsync()
      │
      │  Config: LeagueIds[] (từ appsettings), FixturesPerLeague=20
      │
      For each leagueId:
        Football API: GET /fixtures?league={id}&next={n}
        │  Polly retry: 3 lần, exponential backoff (2^attempt giây)
        │  Header: x-apisports-key
        ▼
      For each fixture:
        ┌─ Upsert theo thứ tự FK dependency ─────────────────────┐
        │  1. Country.UpsertByCode()                             │
        │  2. League.UpsertByExternalId()                        │
        │  3. HomeTeam.UpsertByExternalId()                      │
        │  4. AwayTeam.UpsertByExternalId()                      │
        └────────────────────────────────────────────────────────┘
        │
        Check: uow.Matches.GetByExternalIdAsync(fixture.ExternalId)
        │
        ├── NEW → uow.Matches.AddAsync(newMatch)
        │         Status mapped: "NS"→Scheduled, "1H/2H/HT/ET"→Live, "FT/AET/PEN"→Finished
        │         Schedule 2 jobs:
        │           PreMatchDataJob.FetchH2HAsync()      @ kickoffUtc - 5h
        │           PreMatchDataJob.FetchLineupsAsync()  @ kickoffUtc - 15min
        │
        └── EXISTS → Update Status, HomeScore, AwayScore, FetchedAt
      │
      uow.CommitAsync()
```

**Upsert helpers:** Check DB → if not found: Add + CommitAsync (để lấy real ID) → cache in-memory dict per job run để tránh N+1 queries.

---

### 6b. PreMatchDataJob — Dữ liệu trước trận

```
PreMatchDataJob.FetchH2HAsync(fixtureExternalId, homeExternalId, awayExternalId)
  Chạy: H-5h trước kickoff (scheduled by FetchUpcomingMatchesJob)
      │
      Football API: GET /fixtures/headtohead?h2h={homeId}-{awayId}
      │  → Log số trận H2H nhận được
      │  (Data hiện chỉ log — Phase 5 sẽ persist vào MatchContextData.ContextJson)
      ▼
PreMatchDataJob.FetchLineupsAsync(fixtureExternalId)
  Chạy: H-15min trước kickoff
      │
      Football API: GET /fixtures/lineups?fixture={id}
      │  → Log JSON length của lineup response
      │  (Data hiện chỉ log — Phase 5 sẽ persist)
```

---

### 6c. LiveScorePollingJob — Poll real-time + SignalR broadcast

**Cron:** `* * * * *` (mỗi phút)

```
[Hangfire] LiveScorePollingJob.ExecuteAsync()
      │
      ├─ ADAPTIVE GATE: uow.LiveMatches.GetLiveMatchesAsync()
      │  Nếu count == 0 → skip ngay (tiết kiệm Football API quota)
      │
      Football API: GET /fixtures?live=all  ← 1 request duy nhất, không phải 1/league
      │  Nếu null response → log warning, return
      │
      For each fixture in API response:
        Check: uow.LiveMatches.GetByExternalIdAsync(externalId)
        ├── NEW → find parent Match → set MatchId FK → AddAsync(fixture)
        └── EXISTS → update HomeScore, AwayScore, Status, Minute → UpdateAsync
      │
      For each dbLive NOT in API response:  ← trận vừa kết thúc
        dbLive.Status = Finished → UpdateAsync
        parentMatch.Status = Finished → UpdateAsync
        BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendResultAsync(matchId))
      │
      uow.CommitAsync()
      │
      SignalR broadcast: for each live fixture with MatchId:
        hubContext.Clients
          .Group($"match-{fixture.MatchId}")
          .MatchUpdated(LiveMatchDto)  ← strongly-typed ILiveScoreClient
```

**Tại sao không poll từ browser?**
500 users × 1 req/30s = tiêu hết 100 req/ngày Football API trong vài phút.
Pattern đúng: 1 job server-side poll → 1 SignalR broadcast → N browser clients cập nhật realtime.

---

### 6d. LiveScoreHub — SignalR Server

```csharp
public interface ILiveScoreClient         // Strongly-typed client contract
{
    Task MatchUpdated(LiveMatchDto dto);   // Broadcast từ server → client
}

public class LiveScoreHub : Hub<ILiveScoreClient>
{
    JoinMatch(matchId)  → Groups.AddToGroupAsync($"match-{matchId}")
    LeaveMatch(matchId) → Groups.RemoveFromGroupAsync($"match-{matchId}")
}
```

**Redis backplane:** `AddStackExchangeRedis(ConnectionStrings:Redis)` — cho phép scale-out nhiều API server instances.

---

### 6e. LiveScoreWidget.razor — Blazor Client

```
LiveScore/Index.razor (@page "/livescore", InteractiveServer)
  OnInitializedAsync → LiveScoreClient.GetLiveMatchesAsync() → list of LiveMatchDto
  Foreach match → <LiveScoreWidget MatchId="@match.Id" />

LiveScoreWidget.razor (InteractiveServer, IAsyncDisposable)
  OnInitializedAsync  → GET /api/livescore/{id}  ← initial data
  OnAfterRenderAsync  → HubConnectionBuilder
                           .WithUrl(ApiBaseUrl + "/hubs/livescore")
                           .WithAutomaticReconnect()
                        → _hubConnection.On<LiveMatchDto>("MatchUpdated", dto => {
                             if (dto.Id != MatchId) return;
                             _match = dto;
                             InvokeAsync(StateHasChanged);
                           })
                        → StartAsync() → SendAsync("JoinMatch", MatchId)
  DisposeAsync        → if State == Connected: SendAsync("LeaveMatch", MatchId)
                        → DisposeAsync()
```

---

### 6f. SeedLeagueDataJob — Seed toàn bộ data một lần

**Trigger:** Thủ công từ Admin UI `/admin/jobs` (không schedule tự động)

**Mục đích:** Populate DB với data đầy đủ phục vụ test UI mà không tiêu tốn quota liên tục.

**DB-first pattern** — mỗi bước check DB trước, chỉ gọi API khi thiếu data:

```
SeedLeagueDataJob.ExecuteAsync()
      │  Config: LeagueIds[] (từ appsettings), Season = năm hiện tại
      │
      For each leagueId:
        ┌─ Step 1: Teams + Venues ────────────────────────────────────┐
        │  Football API: GET /teams?league={id}&season={season}       │
        │  → Upsert Team (by ExternalId) + Venue (by ExternalId)      │
        │  → Team.VenueId = venue.Id (nếu chưa có)                   │
        │  uow.CommitAsync()                                          │
        └─────────────────────────────────────────────────────────────┘
        │
        ┌─ Step 2: Standings ─────────────────────────────────────────┐
        │  Check: uow.Standings.HasDataForSeasonAsync(leagueId, year) │
        │  ├─ Có → skip API call                                      │
        │  └─ Không → Football API: GET /standings?league={id}&season │
        │             → Upsert Standing (by LeagueId+TeamId+Season)   │
        │             uow.CommitAsync()                               │
        └─────────────────────────────────────────────────────────────┘
        │
        ┌─ Step 3: Fixtures (past 30 + next 30 ngày) ─────────────────┐
        │  Check: Matches table có data cho league+season này chưa    │
        │  ├─ Có → skip API call                                      │
        │  └─ Không → Football API:                                   │
        │             GET /fixtures?league={id}&season={s}&from=&to=  │
        │             → Upsert Country, League, Team, Match           │
        │             uow.CommitAsync()                               │
        └─────────────────────────────────────────────────────────────┘

Budget tiêu thụ (~15 API calls cho 5 giải):
  GET /teams    × 5 leagues = 5 calls
  GET /standings × 5 leagues = 5 calls (skip nếu đã có)
  GET /fixtures  × 5 leagues = 5 calls (skip nếu đã có)
```

**FootballApiClient methods mới** (dùng bởi SeedLeagueDataJob):

| Method | Endpoint | Returns |
|--------|---------|---------|
| `GetTeamsByLeagueAsync(leagueId, season)` | `GET /teams?league=X&season=Y` | `IEnumerable<TeamRawDto>` (kèm venue) |
| `GetStandingsAsync(leagueId, season)` | `GET /standings?league=X&season=Y` | `IEnumerable<StandingRawDto>` |
| `GetFixturesByRangeAsync(leagueId, season, from, to)` | `GET /fixtures?league=X&season=Y&from=&to=` | `IEnumerable<FixtureRawDto>` |

---

## 7. AI Prediction Pipeline

Luồng hoàn chỉnh từ job schedule đến blog post tự động:

```
[Hangfire Hourly] GeneratePredictionJob.ExecuteAsync()
      │
      │  Filter candidates:
      │    uow.Matches.GetWithoutPredictionAsync()
      │    WHERE ContextData IS NOT NULL AND KickoffUtc > NOW()
      │
      For each match:
        ┌─ Deserialize ContextJson → MatchContext POCO ───────────┐
        │  MatchContext {                                          │
        │    H2HContext { RecentMatches[5], HomeWins, Draws, ... }│
        │    TeamFormContext HomeForm { FormString="WWDLW", ... } │
        │    TeamFormContext AwayForm { ... }                      │
        │    LineupContext? { HomeProbableXI, Injuries, ... }     │
        │    FatigueContext? { DaysSinceLastMatch, ... }          │
        │  }                                                       │
        └──────────────────────────────────────────────────────────┘
        │
        Try: ClaudeAIPredictionProvider.PredictAsync(match, context)
          │  API: POST https://api.anthropic.com/v1/messages
          │  Model: claude-opus-4-6, max_tokens: 1024
          │  Prompt: Vietnamese analysis request → JSON output format
          │  Parse: { predictedOutcome, predictedHomeScore, predictedAwayScore,
          │           confidenceScore, analysisSummary }
        Catch → Fallback: GeminiAIPredictionProvider.PredictAsync(match, context)
          │  API: POST https://generativelanguage.googleapis.com/v1beta/...
          │  Model: gemini-2.0-flash
        Catch → Log error, skip match
        │
        uow.MatchPredictions.AddAsync(new MatchPrediction {
          MatchId, AIProvider, AIModel,
          PredictedOutcome, PredictedHomeScore, PredictedAwayScore,
          ConfidenceScore, AnalysisSummary, PromptTokens, CompletionTokens,
          GeneratedAt=UtcNow, IsPublished=false
        })
        uow.CommitAsync()  ← per-match (cần Id để enqueue job tiếp theo)
        │
        BackgroundJob.Enqueue<PublishPredictionJob>(j => j.ExecuteAsync(prediction.Id))
```

```
[Hangfire enqueue] PublishPredictionJob.ExecuteAsync(predictionId)
      │
      │  Guard: IsPublished? → skip
      │  uow.Matches.GetWithPredictionAsync(pred.MatchId)  ← Include HomeTeam, AwayTeam, League
      │
      Build blog post:
        Title = "Nhận định {homeTeam} vs {awayTeam} — {kickoff}"
        Slug  = "nhan-dinh-{SlugService.Generate(homeTeam)}-vs-{...}-{yyyyMMdd}"
        Content = Markdown với phân tích AI, tỷ số dự đoán, độ tự tin
      │
      postService.CreateAsync(CreatePostDto {
        CategoryId = Prediction:BlogCategoryId (config)
        AuthorId   = Prediction:SystemAuthorId (config)
        PublishNow = true
      })
      │
      pred.BlogPostId = post.Id
      pred.IsPublished = true
      uow.CommitAsync()
      │
      BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendPredictionAsync(predictionId))
```

**AI Provider contract:**
```csharp
interface IAIPredictionProvider {
    string ProviderName { get; }   // "Claude" hoặc "Gemini"
    string ModelName { get; }      // "claude-opus-4-6" hoặc "gemini-2.0-flash"
    Task<AIPredictionResult> PredictAsync(Match match, MatchContext context, CancellationToken ct)
}
```

---

## 8. Telegram Notification Flow

```
[Hangfire enqueue] TelegramNotificationJob.SendPredictionAsync(predictionId)
      │
      │  Guard: TelegramMessageId đã có → skip (idempotent)
      │  uow.Matches.GetWithPredictionAsync(matchId)
      │
      TelegramService.SendPredictionAsync(prediction, match)
        │  Check: _channelId == 0 → skip (chưa config)
        │  BuildPredictionMessage:
        │    "⚽ *{homeTeam} vs {awayTeam}*"
        │    "🏆 {league} \| 🕐 {kickoff}"
        │    "🤖 *Nhận định AI ({AIProvider})*"
        │    "Kết quả: {outcomeVi} | Tỷ số: *{H} \- {A}*"
        │    "{AnalysisSummary[:500]}"
        │  _bot.SendMessage(channelId, text, ParseMode.MarkdownV2)
        │  return msg.MessageId (long)
      │
      prediction.TelegramMessageId = messageId
      uow.CommitAsync()

--- sau khi trận kết thúc ---

LiveScorePollingJob phát hiện trận Finished:
  BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendResultAsync(matchId))
      │
[Hangfire enqueue] TelegramNotificationJob.SendResultAsync(matchId)
      │
      │  Guard: Prediction null → skip
      │  Guard: TelegramMessageId null → skip (chưa gửi lần đầu)
      │  Guard: HomeScore hoặc AwayScore null → skip
      │
      TelegramService.EditResultAsync(messageId, match, prediction)
        │  BuildResultMessage:
        │    "⚽ *{homeTeam} vs {awayTeam}* — KẾT THÚC"
        │    "📊 Kết quả thực tế: *{HomeScore} \- {AwayScore}*"
        │    "🤖 AI dự đoán: {predicted} \({outcome}\)"
        │    "Dự đoán: ✅ Đúng\! hoặc ❌ Sai"
        │  _bot.EditMessageText(channelId, messageId, text, ParseMode.MarkdownV2)
```

**MarkdownV2 safety:** `EscapeMd()` helper escape tất cả ký tự đặc biệt trong dynamic values: `_ * [ ] ( ) ~ \` > # + - = | { } . !`

---

## 9. Data Model Relationships

```
Post ──────────── Category           (N:1, FK: CategoryId)
Post ──────────── ApplicationUser    (N:1, FK: AuthorId)
Post ──────────── PostTag ─────────── Tag  (N:M, composite key PostId+TagId)
Post ◀──────────── MatchPrediction.BlogPost  (1:0..1, sau khi publish)

Match ──────────── League            (N:1, FK: LeagueId)
Match ──────────── Team              (N:1, FK: HomeTeamId)
Match ──────────── Team              (N:1, FK: AwayTeamId)
Match ──────────── MatchPrediction   (1:0..1, unique constraint MatchId)
Match ──────────── MatchContextData  (1:0..1, JSONB blob)
Match ◀──────────── LiveMatch        (1:0..1, FK nullable, SetNull on delete)

League ──────────── Country          (N:1)
Team ──────────── Country            (N:1, nullable)
Team ──────────── Venue              (N:1, FK: VenueId, nullable — sân chủ)
Team ──────────── SquadMember ─────── Player  (N:M qua SquadMember, unique TeamId+PlayerId)

Standing ──────── League             (N:1, FK: LeagueId)
Standing ──────── Team               (N:1, FK: TeamId)
                  unique: (LeagueId, TeamId, Season)

LiveMatch ──────── MatchEvent        (1:N, goal/card/sub/penalty)
```

**Entity fields quan trọng:**

`Match`:
- `ExternalId` — Fixture ID từ Football API (unique index)
- `Status: MatchStatus` — `Scheduled | Live | Finished | Postponed | Cancelled`
- `HomeScore?, AwayScore?` — nullable (null khi chưa bắt đầu)
- `KickoffUtc` — UTC (convert `.ToLocalTime()` khi hiển thị)
- `FetchedAt` — timestamp lần fetch gần nhất từ Football API

`MatchPrediction`:
- `AIProvider` — "Claude" hoặc "Gemini"
- `PredictedOutcome` — "HomeWin" | "Draw" | "AwayWin"
- `ConfidenceScore: decimal` — 0-100
- `TelegramMessageId: long?` — để edit message sau trận
- `BlogPostId: int?` — link đến blog post đã publish
- `IsPublished: bool` — idempotency guard cho PublishPredictionJob

`MatchContextData` — EF entity với JSONB column:
- `ContextJson: string` — serialized `MatchContext` POCO

`MatchContext` (POCO, không phải EF entity):
```
H2HContext      { RecentMatches[5], HomeWins, Draws, AwayWins }
TeamFormContext { TeamName, FormString "WWDLW", RecentMatches[5], GoalsScored, GoalsConceded }
LineupContext?  { HomeProbableXI[], AwayProbableXI[], HomeInjuries[], AwayInjuries[] }
RefereeContext? { Name, Notes }
FatigueContext? { HomeDaysSinceLastMatch, AwayDaysSinceLastMatch, HomePlayingEurope, Notes }
```

`Venue`:
- `ExternalId` — Venue ID từ Football API (unique index)
- `Name`, `City`, `Capacity?`, `ImageUrl?`
- Populate từ `GET /teams?league=X&season=Y` (team response kèm venue)

`Standing`:
- `LeagueId`, `TeamId`, `Season` — unique constraint (3 fields)
- `Rank`, `Points`, `Played`, `Won`, `Drawn`, `Lost`, `GoalsFor`, `GoalsAgainst`, `GoalsDiff`
- `Form` — "WWDLW" (5 trận gần nhất)
- `Description` — "Promotion - Champions League"
- `UpdatedAt`

`Player`:
- `ExternalId` — Player ID từ Football API (unique index)
- `Name`, `Photo?`, `Nationality?`, `Position?`, `Age?`

`SquadMember` (join table Team ↔ Player):
- `TeamId`, `PlayerId` — unique constraint
- `Number?`, `Position?`

**IUnitOfWork — 14 repositories:**
```
uow.Posts           // IPostRepository
uow.Categories      // ICategoryRepository
uow.Tags            // ITagRepository
uow.LiveMatches     // ILiveMatchRepository
uow.Matches         // IMatchRepository
uow.MatchPredictions // IMatchPredictionRepository
uow.Countries       // ICountryRepository   — upsert by Code
uow.Leagues         // ILeagueRepository    — upsert by ExternalId
uow.Teams           // ITeamRepository      — upsert by ExternalId
uow.MatchContexts   // IMatchContextRepository — 1-to-1 với Match (JSONB)
uow.Venues          // IVenueRepository     — upsert by ExternalId
uow.Standings       // IStandingRepository  — unique (LeagueId, TeamId, Season)
uow.Players         // IPlayerRepository    — upsert by ExternalId
uow.SquadMembers    // ISquadMemberRepository — unique (TeamId, PlayerId)
```

**DbContext config quan trọng:**
- `Post.Slug` — unique index
- `Category.Slug` — unique index
- `Tag.Slug` — unique index
- `LiveMatch.ExternalId` — unique index
- `PostTag` — composite PK (PostId, TagId)
- `LiveMatch → Match` — optional FK, `OnDelete: SetNull`
- `Venue.ExternalId` — unique index
- `Standing` — unique index (LeagueId, TeamId, Season)
- `Player.ExternalId` — unique index
- `SquadMember` — unique index (TeamId, PlayerId)

---

## 10. Blazor Render Mode Rules

| Trang/Component | Render Mode | Lý do |
|----------------|-------------|-------|
| Home, PostDetail, CategoryDetail, TagDetail | **Static SSR** (không có `@rendermode`) | HTML đầy đủ → Google index → SEO |
| Sitemap.xml, robots.txt | Static SSR | Phục vụ crawler |
| LiveScoreWidget, LiveScore/Index | **`@rendermode InteractiveServer`** | Cần WebSocket cho realtime SignalR |
| Admin toàn bộ (Dashboard, Posts, Categories...) | **`@rendermode InteractiveServer`** | CRUD form, MudBlazor dialog — không cần SEO |

**Quy tắc bất biến:**
- KHÔNG set `@rendermode` global ở `App.razor` — mỗi component tự declare
- SSR page **CÓ THỂ** chứa InteractiveServer child component (e.g., LiveScoreWidget trên Home)
- KHÔNG inject `IHttpContextAccessor` trong component dùng chung cả 2 mode
- `AdminPageBase` — base class cho mọi admin page: auto-inject JWT token vào `JwtTokenStore`

---

## 11. Caching Strategy

| Layer | Cơ chế | TTL | Invalidation |
|-------|--------|-----|-------------|
| Blog API GET | `OutputCache` + tag `"posts"` | 5 phút | `EvictByTagAsync("posts")` sau mỗi create/update/delete |
| Football API quota | Redis counter `football_api:requests:{date}` | Reset 00:00 UTC | N/A — guard: if > 90/day → skip |
| SignalR backplane | Redis pub/sub | N/A | Auto-managed |

**Endpoints có OutputCache `"BlogPages"`:**
- `GET /api/posts`, `/api/posts/{slug}`, `/api/posts/by-category/{slug}`, `/api/posts/by-tag/{slug}`
- `GET /api/tags`, `/api/tags/{slug}`, `/api/tags/{slug}/posts`

**Rate limit guard trước mỗi Football API call:**
```
Redis INCR "football_api:requests:{date}" → nếu > 90 → skip, log warning
```
(Hard limit 100 req/day, guard tại 90 để buffer)

---

## 12. Hangfire Background Jobs

| Job | Cron | Trigger | Mô tả |
|-----|------|---------|-------|
| `SeedLeagueDataJob` | Không schedule | **Thủ công** từ Admin UI | Seed toàn bộ data giải đấu 1 lần — Teams, Venues, Standings, Fixtures (~15 API calls) |
| `FetchUpcomingMatchesJob` | `0 */6 * * *` (mỗi 6h) | Recurring | Đồng bộ lịch đấu từ Football API |
| `LiveScorePollingJob` | `* * * * *` (mỗi phút) | Recurring | Poll live score + SignalR broadcast (adaptive gate: skip nếu không có live match trong DB) |
| `GeneratePredictionJob` | `0 * * * *` (mỗi giờ) | Recurring | Tạo AI prediction cho các trận sắp diễn ra |
| `PreMatchDataJob.FetchH2HAsync` | Scheduled (H-5h) | Enqueued by FetchUpcoming | Lấy H2H data |
| `PreMatchDataJob.FetchLineupsAsync` | Scheduled (H-15min) | Enqueued by FetchUpcoming | Lấy lineup data |
| `PublishPredictionJob` | Ngay sau Generate | Enqueued by GeneratePrediction | Tạo blog post từ prediction |
| `TelegramNotificationJob.SendPrediction` | Ngay sau Publish | Enqueued by PublishPrediction | Gửi Telegram message |
| `TelegramNotificationJob.SendResult` | Khi trận kết thúc | Enqueued by LiveScorePolling | Edit Telegram message với kết quả thực |

**Hangfire config:**
- Storage: PostgreSQL, schema `"hangfire"`
- Worker threads: 2
- Dashboard: `/hangfire` (dev only)
- Job activation: ASP.NET Core DI — mỗi job chạy trong Scoped DI scope riêng

**Job chain hoàn chỉnh:**
```
FetchUpcomingMatchesJob
    └─ schedule → PreMatchDataJob.FetchH2HAsync (H-5h)
    └─ schedule → PreMatchDataJob.FetchLineupsAsync (H-15min)

GeneratePredictionJob (hourly)
    └─ enqueue → PublishPredictionJob
                   └─ enqueue → TelegramNotificationJob.SendPrediction

LiveScorePollingJob (minutely)
    └─ on finish → enqueue → TelegramNotificationJob.SendResult
```

---

## 13. External Services & Config

| Service | Keys | Dùng cho | Layer |
|---------|------|----------|-------|
| **Football API** (api-sports.io) | `FootballApi:ApiKey` | Match data, live scores, H2H, lineups | Infrastructure |
| **PostgreSQL** | `ConnectionStrings:DefaultConnection` | Primary DB | Infrastructure |
| **Redis** | `ConnectionStrings:Redis` | Rate limiter + SignalR backplane | API |
| **Hangfire** | (dùng PostgreSQL) | Background jobs | API |
| **Claude API** | `Claude:ApiKey` | AI prediction primary (claude-opus-4-6) | Infrastructure |
| **Gemini API** | `Gemini:ApiKey` | AI prediction fallback (gemini-2.0-flash) | Infrastructure |
| **Telegram Bot** | `Telegram:BotToken` + `Telegram:ChannelId` | Prediction notifications | Infrastructure |

**App config:**

| Key | Dùng ở | Mô tả |
|-----|--------|-------|
| `Jwt:Key` | API | JWT signing key (min 32 chars) |
| `WebBaseUrl` | API CORS | Origin whitelist |
| `ApiBaseUrl` | Web | Base URL cho tất cả typed HttpClients |
| `FootballApi:LeagueIds[]` | FetchUpcomingMatchesJob | Danh sách giải theo dõi |
| `FootballApi:FixturesPerLeague` | FetchUpcomingMatchesJob | Số trận lấy mỗi giải |
| `Prediction:BlogCategoryId` | PublishPredictionJob | Category ID cho bài nhận định |
| `Prediction:SystemAuthorId` | PublishPredictionJob | User ID dùng cho auto-publish |

---

## 14. DI Container & Service Lifetimes

**API (`FootballBlog.API/Program.cs`):**

| Service | Lifetime | Ghi chú |
|---------|----------|---------|
| `IUnitOfWork` → `UnitOfWork` | Scoped | Per HTTP request / Hangfire job scope |
| `IPostService` → `PostService` | Scoped | |
| `ICategoryService` → `CategoryService` | Scoped | |
| `ILiveScoreService` → `LiveScoreService` | Scoped | |
| `IAIPredictionProvider` → `ClaudeAIPredictionProvider` | Scoped | Cả 2 registered — inject là `IEnumerable<IAIPredictionProvider>` |
| `IAIPredictionProvider` → `GeminiAIPredictionProvider` | Scoped | Fallback |
| `ITelegramService` → `TelegramService` | Scoped | |
| `IConnectionMultiplexer` (Redis) | Singleton | Thread-safe, shared |
| `IFootballApiRateLimiter` → `RedisFootballApiRateLimiter` | Singleton | Dùng Redis singleton |
| `IHttpClientFactory` (Football API) | Built-in | Polly retry 3 lần, exponential backoff |

**Web (`FootballBlog.Web/Program.cs`):**

| Service | Lifetime | Ghi chú |
|---------|----------|---------|
| `JwtTokenStore` | Scoped | Per Blazor circuit — lưu JWT token |
| `JwtAuthHandler` | Transient | DelegatingHandler cho AdminApiClient |
| `IPostApiClient` | HttpClient factory | Per request |
| `IAdminApiClient` | HttpClient factory + JwtAuthHandler | Bearer auto-inject |

**Middleware pipeline (API):**
```
SerilogRequestLogging → DevExceptionPage → Swagger (dev) →
Hangfire Dashboard (dev) → HTTPS Redirect → Static Files →
CORS("BlazorWeb") → RateLimiter → Authentication → Authorization →
OutputCache → MapControllers → MapHub<LiveScoreHub>("/hubs/livescore") → HealthChecks
```

---

## 15. Logging Architecture

**Sinks (Serilog):** cả API và Web đều có cấu trúc tương tự

| Sink | File pattern | Level |
|------|-------------|-------|
| Console | — | Information+ |
| `logs/app/app-.log` | Rolling daily | Information+ |
| `logs/error/error-.log` | Rolling daily | Error + Fatal |
| `logs/api/api-.log` | Rolling daily | HTTP request logs |
| `logs/jobs/jobs-.log` | Rolling daily | Hangfire jobs |

**Logging convention (service layer):**
```csharp
LogDebug(...)       // Bắt đầu operation, flow detail
LogInformation(..)  // Thành công với data (Id, Count, Slug)
LogWarning(..)      // Not found / unexpected state (không crash)
LogError(ex, ..)    // Exception — luôn truyền exception object
```

**Structured logging — luôn dùng named properties:**
```csharp
_logger.LogInformation("Post created {PostId} slug={Slug}", post.Id, post.Slug);  // ✅
_logger.LogInformation("Post created " + post.Id);  // ❌ string concat
```

---

## 16. Database Migration

### Nguyên tắc
- EF Core dùng bảng `__EFMigrationsHistory` để theo dõi migration nào đã apply
- Migration chỉ thay đổi **schema** (DDL) — KHÔNG can thiệp data
- Data seeding (`ApiKeySeeder`) là bước riêng, chạy sau migration trong `Program.cs`
- Lệnh `dotnet ef database update` **idempotent** — bỏ qua migration đã apply, chỉ chạy pending

### Cách 1 — Thủ công (CLI) ← Khuyến nghị cho Production

Chạy từ root folder của solution:

```bash
# Apply tất cả pending migrations
dotnet ef database update --project FootballBlog.Infrastructure --startup-project FootballBlog.API

# Kiểm tra migration nào đã/chưa apply
dotnet ef migrations list --project FootballBlog.Infrastructure --startup-project FootballBlog.API

# Tạo migration mới (sau khi thay đổi model)
dotnet ef migrations add <TênMigration> --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

**Ưu điểm:** Kiểm soát hoàn toàn — không bao giờ tự apply migration bất ngờ lên Production.

**Khi dùng:** CI/CD pipeline, deploy lên Production, hoặc sau khi tạo migration mới trong dev.

---

### Cách 2 — Tự động khi startup (Auto-Migrate) ← Dev/Staging

Thêm vào `Program.cs` **trước** khi gọi `ApiKeySeeder`:

```csharp
// Áp dụng pending migrations khi khởi động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Seed API keys (chỉ chạy nếu bảng còn trống)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ApiKeySeeder>();
    await seeder.SeedAsync();
}
```

**Ưu điểm:** Dev không cần nhớ chạy lệnh sau mỗi lần pull code có migration mới.

**Khi dùng:** Dev local và Staging — schema thay đổi thường xuyên, không sợ mất data.

> ⚠️ **KHÔNG dùng Cách 2 trên Production** — nếu migration lỗi, app sẽ crash khi deploy thay vì bắt lỗi từ CI/CD pipeline trước.

---

### Migration hiện có

| # | Migration | Ngày | Nội dung |
|---|-----------|------|---------|
| 1 | `InitialCreate` | 2026-04-01 | Schema ban đầu: Posts, Categories, Tags, Users |
| 2 | `AddMatchAndPrediction` | 2026-04-03 | Match, League, Team, Country, MatchPrediction, LiveMatch, MatchContextData |
| 3 | `IdentityMigration` | 2026-04-03 | ASP.NET Core Identity tables |
| 4 | `FixLiveMatchSchema` | 2026-04-05 | Sửa schema LiveMatch |
| 5 | `RefactorMatchSchema` | 2026-04-15 | Refactor bảng Match dùng FK thay vì string |
| 6 | `AddEventTypeEnum` | 2026-04-15 | MatchEvent.Type → integer enum |
| 7 | `AddApiKeyConfig` | 2026-04-17 | Bảng `ApiKeyConfigs` — quản lý API key đa nhà cung cấp |
| 8 | `AddPromptTemplate` | 2026-04-18 | Bảng `PromptTemplates` — lưu AI prompt để A/B test |
| 9 | `AddVenueStandingPlayerSquad` | 2026-04-20 | Bảng `Venues`, `Standings`, `Players`, `SquadMembers` + Team.VenueId FK |

---

*Cập nhật lần cuối: 2026-04-20. Phase 1–6.6 hoàn thành. Phase 6.7 (Wire Admin UI + Seed data) đang thực hiện. Phase 7 (Deploy & DevOps) chưa bắt đầu.*
