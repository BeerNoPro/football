# Cách Hệ Thống Hoạt Động

## Tổng quan kiến trúc

```
Browser / Telegram
      │
      ▼
FootballBlog.Web  (Blazor)
  SSR pages      ──HttpClient──▶  FootballBlog.API  (ASP.NET Core)
  Admin pages                           │
  Live widget ◀──SignalR──────────      │
                                   IPostService
                                   ICategoryService
                                        │
                                   IUnitOfWork
                                   (6 repositories)
                                        │
                                   PostgreSQL  ◀──EF Core──  FootballBlog.Infrastructure
                                   Redis
```

**4 projects:**
- `Web` → gọi `API` qua typed HttpClient (`IPostApiClient`, `ICategoryApiClient`)
- `API` → chứa controllers, SignalR hub, Hangfire jobs
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

## 3. Live Score Flow (Phase 4 — chưa implement)

```
[Hangfire Cron 30s] LiveScorePollingJob
      │  Chỉ chạy khi có Match có Status = Live
      │  Kiểm tra Redis counter trước (rate limit 100 req/ngày)
      ▼
Football API: GET /fixtures?live=all
      │
      ▼
Update LiveMatch.Status, Score, Minute
Update MatchEvents (goal/card/sub)
uow.CommitAsync()
      │
      ▼
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

---

## 4. AI Prediction Pipeline (Phase 5 — chưa implement)

```
[Cron 6h] FetchUpcomingMatchesJob
      │  Football API: fixtures?next=20
      │  Upsert Match records (by ExternalId)
      ▼
[Cron 1h] FetchMatchContextJob
      │  Query: Match WHERE Status=Scheduled AND KickoffUtc <= NOW()+24h AND ContextData IS NULL
      │  Với mỗi match: 3-4 API calls (H2H, HomeForm, AwayForm, Lineup)
      │  Serialize → MatchContextData.ContextJson (JSONB)
      ▼
[Cron 1h] GeneratePredictionJob
      │  Query: Match WHERE Prediction IS NULL AND ContextData IS NOT NULL
      │  Deserialize ContextJson → MatchContext POCO
      │  Build AI prompt từ MatchContext
      ▼
IAIPredictionProvider (abstraction)
      │
      ├── ClaudeAIPredictionProvider  (primary)
      │     claude-opus-4-6 API call
      └── GeminiAIPredictionProvider  (fallback)
            gemini-2.0-flash API call
      │
      ▼
MatchPrediction { PredictedScore, Confidence, Analysis }
uow.CommitAsync()
      │
      ▼
[Trigger] PublishPredictionJob
      │  Tạo Post từ prediction
      │  Gửi Telegram message → lưu TelegramMessageId
      ▼
Sau trận kết thúc: Edit Telegram message với kết quả thực tế
```

**Rate limit guard:**  
Trước mỗi Football API call: `INCR redis "football_api:requests:{date}"` — nếu > 90 thì skip, log warning.

---

## 5. Data Model Relationships

```
Post ──────── Category  (N:1)
Post ──────── ApplicationUser  (N:1, Author)
Post ──────── PostTag ──── Tag  (N:M)
Post ◀──────── MatchPrediction.BlogPost  (1:0..1, sau khi publish)

Match ──────── MatchPrediction  (1:0..1)
Match ◀──────── LiveMatch.Match  (1:0..1, khi trận live)

LiveMatch ──────── MatchEvent  (1:N, goal/card/sub)
```

**IUnitOfWork quick ref:**
```csharp
uow.Posts           // IPostRepository
uow.Categories      // ICategoryRepository
uow.Tags            // ITagRepository
uow.LiveMatches     // ILiveMatchRepository
uow.Matches         // IMatchRepository
uow.MatchPredictions // IMatchPredictionRepository
```

---

## 6. Blazor Render Mode — Quy tắc

| Trang | Mode | Lý do |
|-------|------|-------|
| Home, Blog list, Bài viết, Category | **Static SSR** | HTML đầy đủ → Google index → SEO |
| Sitemap.xml, robots.txt | **Static SSR** | Phục vụ crawler |
| Live Score widget | **InteractiveServer** | Cần WebSocket cho realtime |
| Admin (toàn bộ) | **InteractiveServer** | CRUD form, dialog — không cần SEO |

**Quy tắc bất biến:**
- KHÔNG set `@rendermode` global ở `App.razor`
- SSR page CÓ THỂ chứa InteractiveServer child component (LiveScoreWidget trên Home)
- KHÔNG inject `IHttpContextAccessor` trong component dùng chung cả 2 mode

---

## 7. Caching Strategy

| Layer | Cơ chế | TTL |
|-------|--------|-----|
| API blog GET endpoints | `OutputCache` + tag `"posts"` | 5 phút |
| Football API rate limit | Redis counter `football_api:requests:{date}` | Reset 00:00 UTC |
| SignalR backplane (Phase 4) | Redis pub/sub | N/A |

Khi admin create/update/delete post → `cacheStore.EvictByTagAsync("posts")` → cache cleared ngay lập tức.
