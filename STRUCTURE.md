# Football Blog — Kiến Trúc & Vận Hành Hệ Thống

> Tài liệu tổng hợp về cách hệ thống hoạt động, tech stack, lifecycle từng module, và các điểm hạn chế.

---

## 1. Tổng Quan Kiến Trúc

### 4-Layer Architecture

```
FootballBlog.Web  ──────►  FootballBlog.API
        │                        │
        │                        ▼
        └──────────►  FootballBlog.Core  ◄──────  FootballBlog.Infrastructure
```

| Project | Vai trò | Port (dev) |
|---------|---------|-----------|
| `FootballBlog.Web` | Blazor frontend (SSR + InteractiveServer) | https://localhost:7241 |
| `FootballBlog.API` | REST API + SignalR + Hangfire jobs | https://localhost:7007 |
| `FootballBlog.Core` | Domain models, DTOs, interfaces (không dependency) | — |
| `FootballBlog.Infrastructure` | EF Core, repositories, external services | — |

### Project References

```
FootballBlog.Web        → Core
FootballBlog.API        → Core + Infrastructure
FootballBlog.Infrastructure → Core
FootballBlog.Core       → (không reference project nào)
```

### Local Infrastructure

```bash
docker compose up   # PostgreSQL 16 (port 5432) + Redis 7 (port 6379)
```

### Production (Fly.io Singapore)

```
GitHub push master → GitHub Actions → flyctl deploy API → flyctl deploy Web
Fly.io Region: sin (Singapore) | Memory: 1GB | CPU: 1 core | Auto-stop khi idle
```

---

## 2. Tech Stack Đầy Đủ

| Layer | Technology | Version | Ghi chú |
|-------|-----------|---------|---------|
| **Backend** | ASP.NET Core | 8.0 | REST API + middleware pipeline |
| **Frontend** | Blazor SSR + InteractiveServer | 8.0 | Public=SSR, Admin=Island per page |
| **Admin UI** | MudBlazor | 8.0.0 | Material Design component library |
| **Public UI** | Tailwind CSS | 3.4 | npm build chain (`npm run build:css`) |
| **Database** | PostgreSQL | 16-alpine | EF Core Code-First, 15 migrations |
| **ORM** | EF Core + Npgsql | 8.0.0 | JSONB support, QueryLoggingInterceptor |
| **Cache** | Redis | 7-alpine | Rate limiter + SignalR backplane |
| **Redis Client** | StackExchange.Redis | 2.12.14 | Singleton, thread-safe |
| **Realtime** | SignalR | 8.0.0 | Redis backplane cho multi-instance |
| **Background Jobs** | Hangfire | 1.8.23 | PostgreSQL storage (schema: hangfire) |
| **AI (Primary)** | Google Gemini | — | Free tier, ưu tiên trước (GeneratePrediction + HalfTime) |
| **AI (Fallback)** | Claude API | claude-sonnet-4-6 | Fallback khi Gemini rate-limit/fail |
| **Notifications** | Telegram.Bot | 22.5.0 | MarkdownV2 format, channel broadcast |
| **External Data** | api-football v3 | api-sports.io | 100 req/day free tier, 10 req/min |
| **Auth (API)** | ASP.NET Identity + JWT Bearer | 8.0.0 | 7-day token, int-based user ID |
| **Auth (Web)** | Cookie Authentication | 8.0.0 | 7-day sliding window |
| **HTTP Resilience** | Polly | 8.0.0 | 3x retry, exponential backoff |
| **Logging** | Serilog | 8.0.3 | 5 log targets: app, error, api, jobs, db |
| **Health Checks** | AspNetCore.HealthChecks | 9.0.0 | PostgreSQL + Redis → `/health` |
| **Rate Limiting** | ASP.NET Core built-in | 8.0 | Login: 5 req/min/IP (fixed window) |
| **Rich Text** | Quill.js | — | JS interop via `quill-interop.js` |
| **API Docs** | Swagger (Swashbuckle) | 6.9.0 | Dev only |
| **Deploy** | Fly.io (Docker multi-stage) | — | Dockerfile.api + Dockerfile.web |
| **CI/CD** | GitHub Actions | — | Push master → auto deploy |

---

## 3. Domain Model (21 Entities + 3 Enums + 2 POCOs)

### Blog Module

```
ApplicationUser (IdentityUser<int>)
    └── (1:M) Post
                ├── → Category  (M:1)
                └── ↔ Tag       (M:M via PostTag junction)
```

| Entity | Key Fields | Ghi chú |
|--------|-----------|---------|
| `Post` | Id, Title, Slug*, CategoryId, AuthorId, PublishedAt (null=draft) | Slug unique index |
| `Category` | Id, Name, Slug* | Slug unique |
| `Tag` | Id, Name, Slug* | Slug unique |
| `PostTag` | PostId + TagId (composite PK) | Many-to-many junction |
| `ApplicationUser` | IdentityUser<int> | Authored posts |

### Football Module

```
Country
  ├── (1:M) League
  │           └── (1:M) Match ──┬── HomeTeam (Team)
  │                             └── AwayTeam (Team)
  └── (1:M) Team
              ├── → Venue
              ├── (1:M) SquadMember → Player
              └── (1:M) Standing → (League + Season)

Match
  ├── ↔ LiveMatch        (1:1, FK SetNull on delete)
  │     └── (1:M) MatchEvent
  ├── ↔ MatchContextData (1:1 lazy load, JSONB)
  └── (1:M) MatchPrediction  (unique per Phase: PreMatch | HalfTime)
```

| Entity | Key Fields | Ghi chú |
|--------|-----------|---------|
| `Match` | ExternalId*, KickoffUtc, Status, HomeScore/AwayScore, HtScore, EtScore, PenScore, StatsJson, EventsJson | ExternalId từ api-football |
| `LiveMatch` | ExternalId*, Status, Minute, MatchId (FK nullable) | Real-time, SetNull khi Match xóa |
| `MatchEvent` | LiveMatchId, Minute, Type (enum), Description | Goal, Card, Sub, Penalty |
| `MatchPrediction` | MatchId, Phase*, AIProvider, PredictedScore, ConfidenceScore, AnalysisSummary, TelegramMessageId | Unique: (MatchId, Phase) |
| `MatchContextData` | MatchId* (unique), ContextJson (JSONB), FetchedAt | H2H + form + lineup + fatigue |
| `Team` | ExternalId*, Name, ShortName, LogoUrl, CountryId, VenueId | |
| `League` | ExternalId*, Name, LogoUrl, CountryId, IsActive | |
| `Country` | Code* (ISO), Name, FlagUrl | |
| `Venue` | ExternalId*, Name, City, Capacity | |
| `Player` | ExternalId*, Name, Photo, Nationality, Position, Age | |
| `SquadMember` | TeamId + PlayerId (unique), Number, Position | |
| `Standing` | LeagueId + TeamId + Season (unique), Rank, Points, Form | |

### Config Module

| Entity | Mục đích |
|--------|---------|
| `ApiKeyConfig` | Key rotation theo priority + daily limit |
| `PromptTemplate` | AI prompt versioning (Claude/Gemini) |
| `ApiUsageDaily` | Quota tracking per service per day |

### Context POCOs (serialized vào JSONB, không phải DB table)

- `MatchContext` — H2H, form, lineup, fatigue (lưu trong MatchContextData.ContextJson)
- `HalfTimeContext` — HT score, stats, events (tạm thời trong job, không persist)

### Enums

- `MatchStatus`: Scheduled, Live, HalfTime, Finished, Postponed, Cancelled
- `EventType`: Goal, YellowCard, RedCard, Substitution, Penalty, Offside, Other
- `PredictionPhase`: PreMatch (0), HalfTime (1)

---

## 4. Background Jobs — Lifecycle

9 Hangfire jobs. Tất cả đều **idempotent**, max **3 retries**, backoff **5 phút**.

| Job | Trigger | API Calls | Mục đích |
|-----|---------|-----------|---------|
| `FetchUpcomingMatchesJob` | Daily 05:00 VN | 3 (per day ±1) | Sync fixtures ±1 ngày; upsert Country/League/Team/Match |
| `PreMatchDataJob` | Scheduled H-5 trước kickoff | 1 (H2H) | Build MatchContextData; enqueue GeneratePrediction |
| `GeneratePredictionJob` | Immediate sau PreMatch / batch hourly | 1 (AI) | Gemini → Claude fallback → lưu MatchPrediction PreMatch |
| `HalfTimePredictionJob` | Khi detect HalfTime transition | 2 (HT stats+events) + 1 (AI) | HT context → Gemini → Claude fallback → MatchPrediction HalfTime |
| `LiveScorePollingJob` | Cron.Minutely() | 1 (all live) | Poll live scores; broadcast SignalR; detect HT/FT |
| `TelegramNotificationJob` | Enqueued từ Prediction jobs | 0 (Telegram) | Send / Edit Telegram channel message |
| `FetchPostMatchDataJob` | Sau FT / daily batch (limit 15) | 2 per match | StatsJson + EventsJson cho premium leagues |
| `SeedLeagueDataJob` | **Manual** từ Admin UI | Many | Initial seed: teams, venues, standings, all season fixtures |
| `FetchSquadJob` | **Manual** từ Admin UI | 1 per team | Player squads cho upcoming matches |

**API Budget per premium match:**

```
FetchUpcomingMatchesJob : 1 (chia sẻ với tất cả matches ngày đó)
PreMatchDataJob         : 1 (H2H)
HalfTimePredictionJob   : 2 (stats + events)
FetchPostMatchDataJob   : 2 (stats + events)
──────────────────────────────────────────
Football API calls      : ~6 per match
AI calls (Gemini/Claude): 2 (PreMatch + HalfTime)
```

---

## 5. Request Flow — End-to-End

### Public Blog (SSR)

```
Browser GET /blog/post-slug
  → Web (Blazor SSR, OnInitializedAsync)
  → PostApiClient.GetBySlugAsync()
  → HTTP GET api/posts/{slug}
  → PostsController → PostService
  → PostRepository.GetBySlugAsync() [AsNoTracking, Include tags+category]
  → PostgreSQL
  → ApiResponse<PostDetailDto> → Render HTML → Browser
```

### Live Score (WebSocket)

```
Browser → Web LiveScore page (InteractiveServer)
  → LiveScoreApiClient.GetAllAsync()  [initial HTTP]
  → HubConnection.StartAsync() → SignalR /hubs/livescore
  → JoinMatch(matchId) → Groups.AddToGroupAsync("match-{id}")

[Server-side, every minute]
LiveScorePollingJob
  → FootballApiClient.GetAllLiveFixturesAsync()  [1 req/min]
  → Upsert LiveMatch records
  → HubContext.Clients.Group("match-{id}").MatchUpdated(dto)
  → [Redis backplane propagates to all Web instances]
  → Browser nhận MatchUpdated event → UI cập nhật
```

### Admin Workflow

```
Admin → Web /admin/predictions (InteractiveServer Blazor)
  → JwtAuthHandler: đọc JWT từ cookie → set Authorization: Bearer header
  → AdminApiClient.GetPredictionsAsync()
  → ApiUnauthorizedHandler: catch 401 → redirect /admin/login
  → API AdminPredictionsController [Authorize(Roles="Admin")]
  → JWT validate → Controller → Repository → PostgreSQL
  → PagedResult<MatchPredictionDto>
```

### Match Prediction Lifecycle

```
[Daily 05:00 VN]
FetchUpcomingMatchesJob
  → GetFixturesByDateAsync(date) [1 req × 3 ngày]
  → Filter by LeagueIds config
  → Upsert: Country → League → Team → Match (FK order)
  → Premium leagues: Schedule PreMatchDataJob cho H-5 trước kickoff

[H-5 trước kickoff]
PreMatchDataJob.FetchH2H(matchId)
  → GetHeadToHeadAsync(homeExtId, awayExtId) [1 req]
  → Query DB: recent matches for form (0 API cost)
  → Build MatchContext { H2H, form, fatigue, referee }
  → Upsert MatchContextData (JSONB ContextJson)
  → Enqueue GeneratePredictionJob(matchId) [immediate]

[Immediate]
GeneratePredictionJob.ExecuteForMatchAsync(matchId)
  → Load Match + MatchContextData
  → Deserialize MatchContext
  → GeminiAIPredictionProvider.PredictAsync()  [1 AI req, free tier]
    ↳ (nếu fail) ClaudeAIPredictionProvider.PredictAsync() [fallback]
  → Save MatchPrediction { Phase=PreMatch, TelegramMessageId=null }
  → Schedule TelegramNotificationJob.SendPredictionAsync(predId) @ 06:00 VN

[06:00 VN ngày thi đấu]
TelegramNotificationJob.SendPredictionAsync(predId)
  → Format MarkdownV2 message
  → TelegramService.SendPredictionAsync() → Telegram Bot API
  → Save TelegramMessageId vào MatchPrediction

[Khi match đang diễn ra, mỗi phút]
LiveScorePollingJob
  → GetAllLiveFixturesAsync() [1 req]
  → Upsert LiveMatch records
  → Detect HalfTime: prev status != HT AND new status = HT
  → Enqueue HalfTimePredictionJob(matchId) [immediate]
  → Broadcast MatchUpdated to SignalR groups

[Immediate khi HT]
HalfTimePredictionJob.ExecuteAsync(matchId)
  → GetFixtureHalfTimeDataAsync() [2 reqs: stats + events]
  → Gemini.PredictHalfTimeAsync(match, preMatchCtx, htCtx) [1 AI req, free tier]
    ↳ (nếu fail) Claude.PredictHalfTimeAsync() [fallback]
  → Save MatchPrediction { Phase=HalfTime }
  → Enqueue TelegramNotificationJob.EditHalfTimeAsync(htPredId)

[Immediate]
TelegramNotificationJob.EditHalfTimeAsync(htPredId)
  → TelegramService.EditHalfTimeAsync(messageId, htPrediction)
  → Edit existing Telegram message (append HT analysis)

[Khi FT]
LiveScorePollingJob
  → Match biến mất khỏi /fixtures?live=all
  → Mark LiveMatch.Status = Finished
  → Enqueue FetchPostMatchDataJob [batch, limit 15/run]

FetchPostMatchDataJob
  → GetFixturePostMatchDataAsync(fixtureExtId) [2 reqs per match]
  → Save Match.StatsJson + Match.EventsJson
```

---

## 6. API Endpoints

### Public (no auth)

| Method | Route | Mục đích |
|--------|-------|---------|
| GET | `/api/posts` | Published posts (paginated, output cached 5 min) |
| GET | `/api/posts/{slug}` | Single post by slug |
| GET | `/api/posts/by-category/{slug}` | Posts by category |
| GET | `/api/posts/by-tag/{slug}` | Posts by tag |
| GET | `/api/categories` | All categories |
| GET | `/api/categories/{slug}` | Single category |
| GET | `/api/tags` | All tags |
| GET | `/api/fixtures` | Fixtures (filter: leagueId, date, season, status, search) — `status=Live` bao gồm cả HalfTime |
| GET | `/api/livescore` | All live matches |
| GET | `/api/livescore/{id}` | Single live match |
| POST | `/api/auth/login` | JWT login (rate-limited 5/min/IP) |
| WS | `/hubs/livescore` | SignalR live score hub |
| GET | `/health` | PostgreSQL + Redis health check |

### Admin (`[Authorize(Roles="Admin")]`)

| Method | Route | Mục đích |
|--------|-------|---------|
| GET/POST/PUT/DELETE | `/api/posts/all`, `/api/posts/{id:int}` | Posts CRUD incl. drafts |
| POST | `/api/media/upload` | Image upload (5MB, JPG/PNG/WebP/GIF) |
| GET | `/api/admin/predictions` | Predictions (filter by phase) |
| GET | `/api/admin/predictions/stats` | Accuracy stats |
| GET | `/api/admin/matches/stats` | Match stats (total, live, predictions, pending) + breakdown theo mùa |
| GET | `/api/admin/matches` | Match list with filters |
| POST | `/api/admin/matches/fetch` | Trigger FetchUpcomingMatchesJob |
| POST | `/api/admin/matches/predict-all` | Trigger GeneratePredictionJob batch |
| CRUD | `/api/admin/api-keys` | API key management (masked view) |
| CRUD | `/api/admin/prompts` | AI prompt templates |

---

## 7. Cấu Trúc Thư Mục

```
football/
├── FootballBlog.API/
│   ├── Controllers/           # 13 controllers (6 public + 7 admin)
│   ├── Hubs/
│   │   └── LiveScoreHub.cs    # SignalR strongly-typed hub
│   ├── ApiClients/FootballApi/
│   │   ├── FootballApiClient.cs      # api-sports.io wrapper
│   │   └── FootballApiResponses.cs   # Response model classes
│   ├── Jobs/                  # 9 Hangfire background jobs
│   │   ├── FetchUpcomingMatchesJob.cs
│   │   ├── PreMatchDataJob.cs
│   │   ├── GeneratePredictionJob.cs
│   │   ├── HalfTimePredictionJob.cs
│   │   ├── LiveScorePollingJob.cs
│   │   ├── TelegramNotificationJob.cs
│   │   ├── FetchPostMatchDataJob.cs
│   │   ├── SeedLeagueDataJob.cs
│   │   └── FetchSquadJob.cs
│   ├── Program.cs             # DI + middleware (382 lines)
│   └── appsettings.json
│
├── FootballBlog.Web/
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Blog/          # Public SSR pages (Home, PostDetail, LeaguePage...)
│   │   │   ├── LiveScore/     # InteractiveServer (real-time)
│   │   │   └── Admin/         # InteractiveServer islands (MudBlazor)
│   │   ├── Layout/            # MainLayout, AdminLayout, 2Col, 3Col, Empty
│   │   └── Shared/            # PostCard, Pagination, LiveScoreWidget, QuillEditor...
│   ├── ApiClients/            # 6 typed HttpClients
│   │   ├── IAdminApiClient / AdminApiClient     # JWT + 401 handler
│   │   ├── IPostApiClient / PostApiClient
│   │   ├── IFixtureApiClient / FixtureApiClient
│   │   └── ILiveScoreApiClient / LiveScoreApiClient
│   ├── wwwroot/
│   │   ├── js/quill-interop.js   # Quill.js rich text editor interop
│   │   ├── css/                  # Tailwind output
│   │   └── prototype/            # HTML reference prototypes
│   ├── Program.cs             # DI + Cookie auth (117 lines)
│   └── appsettings.json
│
├── FootballBlog.Core/
│   ├── Models/                # 21 entities + 3 enums + 2 context POCOs
│   ├── DTOs/                  # 21 DTOs (PostDetailDto, FixtureDto, MatchPredictionDto, PagedResult<T>...)
│   ├── Interfaces/            # 16 IRepository<T>, IUnitOfWork, 5 IService, 4 infra interfaces
│   └── Options/               # FootballApiOptions (bound từ appsettings "FootballApi" section)
│
├── FootballBlog.Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs   # EF Core DbContext (extends IdentityDbContext)
│   │   ├── UnitOfWork.cs             # Aggregates 16 repositories
│   │   ├── QueryableExtensions.cs    # TagWithCaller() extension
│   │   └── Migrations/               # 15 migrations (InitialCreate → AddMatchEtPenScores)
│   ├── Repositories/          # 16 repos: BaseRepository<T> + 15 domain repos
│   └── Services/
│       ├── GeminiAIPredictionProvider.cs  # Primary AI (free tier)
│       ├── ClaudeAIPredictionProvider.cs  # Fallback AI (claude-opus-4-6)
│       ├── TelegramService.cs             # Bot notifications (MarkdownV2)
│       ├── ApiKeyRotator.cs               # Key rotation by priority
│       ├── ApiUsageTracker.cs             # Daily quota tracking
│       ├── RedisFootballApiRateLimiter.cs # 10 req/min, 100 req/day (Redis)
│       └── ApiKeySeeder.cs                # Seed keys từ appsettings on startup
│
├── docker-compose.yml         # PostgreSQL 16 + Redis 7 (local dev)
├── Dockerfile.api             # Multi-stage Docker build (API)
├── Dockerfile.web             # Multi-stage Docker build (Web)
├── fly.api.toml               # Fly.io config API (Singapore, 1GB)
├── fly.web.toml               # Fly.io config Web (Singapore, 1GB)
├── .github/workflows/
│   └── deploy.yml             # CI/CD: push master → deploy API → Web
└── .claude/rules/             # Code standards (api, blazor, database, security...)
```

---

## 8. Patterns Chính

### Unit of Work

```csharp
// Chỉ commit 1 lần duy nhất, không SaveChanges trong repository
await uow.Matches.AddAsync(match);          // chỉ add vào ChangeTracker
await uow.Teams.UpdateAsync(team);          // chỉ update ChangeTracker
await uow.CommitAsync();                    // SaveChangesAsync() duy nhất → transaction
```

### Repository Query Pattern

```csharp
// Read-only queries (list/detail cho API/controller): AsNoTracking + TagWithCaller
return await _context.Matches
    .AsNoTracking()
    .Include(m => m.HomeTeam).Include(m => m.AwayTeam)
    .Where(m => m.Status == MatchStatus.Scheduled)
    .TagWithCaller()   // log SQL với caller context vào db.log
    .ToListAsync();

// Upsert lookup queries (dùng trong job để check existing trước khi Add/Update):
// KHÔNG dùng AsNoTracking — entity phải được track để EF Core không cascade Add
// lên related entities khi gọi AddAsync(newEntity { Navigation = existingEntity })
await _dbSet.TagWithCaller().FirstOrDefaultAsync(x => x.Code == code);
```

### API Response Wrapper

```csharp
record ApiResponse<T>(bool Success, T? Data, string? Error = null)

// Usage trong controller:
return Ok(new ApiResponse<PostDetailDto>(true, dto));
return NotFound(new ApiResponse<PostDetailDto>(false, null, "Post not found"));
```

### Blazor Render Modes

```
Public pages:   không có @rendermode → SSR (SEO-friendly)
Admin pages:    @rendermode InteractiveServer → MudBlazor, real-time
LiveScore:      @rendermode InteractiveServer → SignalR WebSocket
Login page:     SSR only → HttpContext form submission
```

---

## 9. Logging Structure

Serilog với 5 sink targets khác nhau cho API:

```
/logs/
├── app/app-{date}.log        # General logs (Information+)
├── error/error-{date}.log    # Error+ only
├── api/api-{date}.log        # HTTP request/response logs
├── jobs/jobs-{date}.log      # Hangfire job execution
└── database/db-{date}.log    # EF Core SQL queries (với TagWithCaller)
```

---

## 10. Hạn Chế & Rủi Ro

### Critical (cần fix trước khi production)

| # | Vấn đề | Ảnh hưởng | Giải pháp |
|---|--------|-----------|----------|
| 1 | **Media upload vào `wwwroot/uploads/`** | File mất khi container restart trên Fly.io | Tích hợp S3 hoặc Cloudflare R2 |
| 2 | **Default admin `Admin123` trong appsettings** | Credential lộ nếu commit lên GitHub | Dùng `dotnet user-secrets` + đổi pass sau deploy |
| 3 | **Không có FluentValidation** | Slug format sai, pageSize=999999 không bị chặn | Thêm FluentValidation cho DTOs |
| 4 | **Admin pages không có Error Boundaries** | Network fail → blank page, không có fallback | Wrap `OnInitializedAsync` với try/catch + error UI |

### Medium (cần xử lý trong thời gian tới)

| # | Vấn đề | Ảnh hưởng |
|---|--------|-----------|
| 5 | **API key hết quota → trả null, silent fail** | Job chạy nhưng không có prediction, không alert |
| 6 | **Quill HTML không sanitize** | XSS risk nếu admin account bị chiếm |
| 7 | **LiveScore polling mỗi phút cứng** | Scale lên 100+ match live → DB write pressure |
| 8 | **Season start hardcoded tháng 7** | Giải đá tháng 8 hoặc tháng 1 fetch thiếu fixture |
| 9 | **`/api/categories` và `/api/tags` không cache** | DB hit mỗi page request |

### Low (design decisions, acceptable)

| # | Vấn đề | Ghi chú |
|---|--------|---------|
| 10 | **JWT 7 ngày absolute (không sliding)** | User phải login lại sau 7 ngày active |
| 11 | **FullTime prediction phase chưa có** | Chỉ PreMatch + HalfTime; post-match AI summary thiếu |
| 12 | **Prediction accuracy naive** | Chỉ đúng/sai, không breakdown Win/Draw/Loss |
| 13 | **SignalR hub không [Authorize]** | Public live score → OK cho design hiện tại |
| 14 | **Không có pagination max cap** | `pageSize=999999` không bị reject |
| 15 | **AdminApiClient trả null khi lỗi** | Admin UI hiển thị blank thay vì error message |

---

## 11. Migrations Timeline

| Migration | Ngày | Thay đổi chính |
|-----------|------|---------------|
| `InitialCreate` | 2026-04-01 | Blog schema (Post, Category, Tag, User) |
| `AddMatchAndPrediction` | 2026-04-03 | Match, MatchPrediction |
| `IdentityMigration` | 2026-04-03 | ASP.NET Identity tables |
| `FixLiveMatchSchema` | 2026-04-05 | LiveMatch refinements |
| `RefactorMatchSchema` | 2026-04-15 | FK normalization (Teams, Leagues as entities) |
| `AddEventTypeEnum` | 2026-04-15 | MatchEvent.Type enum |
| `AddApiKeyConfig` | 2026-04-17 | API key rotation |
| `AddPromptTemplate` | 2026-04-18 | AI prompt versioning |
| `AddVenueStandingPlayerSquad` | 2026-04-20 | Venue, Standing, Player, SquadMember |
| `AddApiUsageDaily` | 2026-05-06 | Daily quota tracking |
| `AddApiUsageDailyUniqueIndex` | 2026-05-06 | Unique (Date, Service) |
| `AddHalfTimePrediction` | 2026-05-07 | PredictionPhase enum (PreMatch/HalfTime) |
| `RemoveBlogPostFromPrediction` | 2026-05-08 | Cleanup: bỏ PostId FK từ MatchPrediction |
| `AddMatchHtScore` | 2026-05-09 | HtHomeScore, HtAwayScore columns |
| `AddMatchEtPenScores` | 2026-05-09 | Extra-time và penalty score columns |
| `AddMatchStatusKickoffIndex` | 2026-05-10 | Composite index `(Status, KickoffUtc)` + StatsJson/EventsJson columns |
