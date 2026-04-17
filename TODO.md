# Football Blog — Project Plan

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8 (C#) |
| Frontend | Blazor (SSR + InteractiveServer) |
| Database | PostgreSQL |
| Cache / Realtime | Redis + SignalR |
| Background Jobs | Hangfire |
| ORM | Entity Framework Core |
| UI Design | HTML Prototype → Blazor components |
| Local Dev | Docker Compose |
| Hosting (dev) | Railway (free tier) |
| Hosting (prd) | AWS EC2 + RDS + S3 + CloudFront |
| CDN / DNS | Cloudflare |
| IDE | VS Code (AI + edit) + Visual Studio (debug) |
| AI APIs | Claude API (claude-opus-4-6) / Google Gemini |
| Notification | Telegram Bot API |

---

## Project Structure

```
FootballBlog/
├── FootballBlog.Web/              # Blazor UI + Razor Pages
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Blog/              # SSR - SEO tốt
│   │   │   ├── LiveScore/         # InteractiveServer - Realtime
│   │   │   └── Admin/             # InteractiveServer - CRUD
│   │   └── Layout/
│   └── wwwroot/
├── FootballBlog.API/              # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Hubs/                      # SignalR Hub
│   └── Jobs/                      # Hangfire background jobs
├── FootballBlog.Core/             # Business Logic
│   ├── Services/
│   ├── Models/
│   └── Interfaces/
└── FootballBlog.Infrastructure/   # Data Access
    ├── Data/                      # EF Core DbContext
    └── Repositories/
```

---

## Phases

### Phase 1 — Setup & Foundation ✅
- [x] Solution structure 4 projects (Web/API/Core/Infrastructure)
- [x] Docker Compose (PostgreSQL + Redis) với healthchecks
- [x] EF Core + InitialCreate migration (7 entities)
- [x] Serilog multi-sink (app/error/api/jobs)
- [x] IUnitOfWork + UnitOfWork pattern
- [x] DTOs: PostSummaryDto, PostDetailDto, CategoryDto, LiveMatchDto, MatchSummaryDto, MatchPredictionDto, PagedResult<T>
- [x] Service Layer: IPostService/PostService, ICategoryService
- [x] Typed HttpClient IPostApiClient + ICategoryApiClient trong Web
- [x] API Controllers: PostsController, CategoriesController (CRUD + filter by category/tag)
- [x] SlugService (static, hỗ trợ tiếng Việt + GenerateUnique)
- [x] Tailwind CSS setup (npm build pipeline)
- [x] Claude hooks: build-check, dbcontext-check, stop-notify

### Phase 2 — Blog Core (SEO) ✅
- [x] Blazor SSR pages: Home, PostDetail, CategoryDetail, TagDetail, News, SearchResults
- [x] SEO: meta tags, Open Graph (SeoHead.razor)
- [x] Schema.org JSON-LD cho bài viết (PostDetail.razor + SeoHead.razor)
- [x] sitemap.xml (Sitemap.razor), robots.txt
- [x] Shared components: LeftSidebar, RightSidebar, PostCard, PostCardCompact, Pagination, TagPill
- [x] Layout: PublicLayout2Col, PublicLayout3Col
- [ ] Upload ảnh lên S3 (hoặc local khi dev)

### Phase 3 — Admin Panel ✅
- [x] Replace ApplicationUser → extend IdentityUser<int> + migration
- [x] ASP.NET Core Identity đã register (AddIdentity<ApplicationUser, IdentityRole<int>>)
- [x] Cookie Auth cho Blazor + JWT cho API (AuthController wire, Login/Logout pages)
- [x] Install MudBlazor 8.0.0 (chỉ load CSS/JS cho /admin routes)
- [x] AdminLayout.razor (MudBlazor shell — dark theme)
- [x] Admin pages: Dashboard, Posts list, Categories CRUD, Tags list
- [x] Posts CRUD đầy đủ (Create/Edit pages + IAdminApiClient mở rộng)
- [x] Rich text editor (Quill.js qua JS interop — QuillEditor.razor + quill-interop.js)
- [x] Upload ảnh → MediaController (local/dev) + InputFile trong Create/Edit

### Phase 4 — Realtime Football 🔄 (In Progress) ← NEXT
- [x] FootballApiClient (IHttpClientFactory + Polly retry)
- [x] Redis rate limit counter (100 req/ngày)
- [x] Match + MatchContext + MatchContextData entities
- [x] Country, League, Team entities + repositories (upsert by ExternalId)
- [x] MatchStatus enum
- [x] Hangfire jobs: FetchUpcomingMatchesJob (cron 6h), LiveScorePollingJob (1min adaptive), PreMatchDataJob (H2H + Lineups)
- [x] EF migration: RefactorMatchSchema (Country/League/Team/MatchContextData)
- [x] MatchEvent.Type → EventType enum + migration
- [x] ILiveScoreService implementation (LiveScoreService) + register DI
- [x] SignalR Hub (LiveScoreHub) + Redis backplane
- [x] Blazor LiveScore pages + widget (InteractiveServer)

### Phase 5 — AI Match Prediction ⬜
- [x] Domain: MatchContext (H2H, TeamForm, Lineup, Referee, Fatigue POCOs)
- [x] MatchContextData entity (JSONB blob, 1-to-1 với Match)
- [x] IMatchContextRepository + implementation
- [x] FixtureRawDto (raw API response mapping)
- [x] IAIPredictionProvider interface + Claude implementation
- [ ] Prompt template lưu DB để A/B test
- [x] Hangfire GeneratePredictionJob (trigger 24h trước kickoff)
- [x] PublishPredictionJob → tạo blog post từ prediction
- [x] Gemini implementation (fallback provider)

### Phase 6 — Telegram + Auto-publish ✅ (core done)
- [x] Install Telegram.Bot NuGet
- [x] ITelegramService: SendPredictionAsync, EditMessageAsync
- [x] TelegramNotificationJob (Hangfire) — gửi prediction + edit kết quả
- [ ] TelegramNotificationChannel implement INotificationChannel
- [ ] Bot command: /lichdat (query lịch đấu upcoming)
- [x] Edit Telegram message khi kết quả thực tế về
- [ ] Admin page: xem prediction history, manual retrigger

### Phase 6.5 — API Key Management ⬜ ([plan](.claude/plans/phase-6.5-api-key-management.md))
- [ ] Entity `ApiKeyConfig` (Provider, KeyValue, Priority, IsActive, DailyLimit, UsedToday, LastResetAt)
- [ ] EF migration: AddApiKeyConfig
- [ ] `IApiKeyRotator<TProvider>` — thử key theo Priority, skip key IsActive=false hoặc UsedToday≥DailyLimit
- [ ] Redis cache key list (TTL 5 phút) — tránh query DB mỗi request
- [ ] Migrate config: FootballApi/Claude/Gemini từ appsettings → DB (giữ appsettings làm seed lần đầu)
- [ ] Admin page: `/admin/api-keys` — CRUD (thêm/xóa/enable/disable key, xem usage hôm nay)
- [ ] Auto-reset `UsedToday = 0` lúc 00:00 UTC (Hangfire cron job)
- [ ] Cập nhật `FootballApiClient`, `ClaudeAIPredictionProvider`, `GeminiAIPredictionProvider` dùng `IApiKeyRotator`

> **Không lưu DB:** `ConnectionStrings`, `Jwt:Key`, `Redis` — đây là infra config, không phải business config.
> **Khi implement:** review toàn bộ `appsettings.json` + `appsettings.Development.example.json` để phân loại lại — key nào migrate sang DB, key nào giữ nguyên config. Cập nhật `appsettings.Development.example.json` sau khi xong.

### Phase 7 — Deploy & DevOps ⬜
- [ ] Dockerfile (multi-stage, Web + API)
- [ ] Railway deploy (dev/staging)
- [ ] GitHub Actions CI/CD pipeline
- [ ] AWS EC2 + RDS + S3 + CloudFront
- [ ] CloudWatch logging
- [ ] Monitoring: alert khi Football API gần hết quota

---

## Config Setup

> **Quy tắc:** Secrets KHÔNG bao giờ commit. Điền vào `appsettings.Development.json` (gitignored).
> Template: xem `appsettings.Development.example.json`.

### FootballBlog.API — `appsettings.Development.json`

| Section | Key | Mô tả | Bắt buộc |
|---------|-----|--------|-----------|
| `ConnectionStrings` | `DefaultConnection` | PostgreSQL connection string | ✅ |
| `ConnectionStrings` | `Redis` | Redis URL (mặc định `localhost:6379`) | ✅ |
| `Jwt` | `Key` | JWT signing key — **tối thiểu 32 ký tự** | ✅ |
| `WebBaseUrl` | — | URL Blazor Web (CORS whitelist) | ✅ |
| `FootballApi` | `ApiKey` | API key từ api-sports.io | ✅ |
| `Claude` | `ApiKey` | Anthropic API key — console.anthropic.com | Phase 5 |
| `Gemini` | `ApiKey` | Google AI Studio API key — aistudio.google.com | Phase 5 |
| `Telegram` | `BotToken` | Token từ @BotFather | Phase 6 |
| `Telegram` | `ChannelId` | Channel ID âm (ví dụ `-1001234567890`) | Phase 6 |
| `Prediction` | `BlogCategoryId` | ID category "Nhận định" trong DB | Phase 5 |
| `Prediction` | `SystemAuthorId` | ID user admin/system để tạo bài viết | Phase 5 |

### FootballBlog.Web — `appsettings.Development.json`

| Key | Mô tả | Bắt buộc |
|-----|--------|-----------|
| `ApiBaseUrl` | URL API (ví dụ `https://localhost:7007`) | ✅ |

### Cách lấy các key

- **Jwt:Key** — tự tạo: `openssl rand -base64 32`
- **Claude:ApiKey** — [console.anthropic.com](https://console.anthropic.com) → API Keys
- **Gemini:ApiKey** — [aistudio.google.com](https://aistudio.google.com) → Get API Key

#### FootballApi:ApiKey — api-sports.io (free 100 req/ngày)

1. Vào [api-sports.io](https://api-sports.io) → **Sign Up** (hoặc Login nếu đã có)
2. Xác nhận email → vào Dashboard
3. Chọn product **API-Football** → click **Subscribe** → chọn plan **Free**
4. Vào **Dashboard → API Key** → copy key (dạng `abc123def456...`)
5. Set vào project:
   ```bash
   # Chạy từ thư mục gốc football/
   dotnet user-secrets set "FootballApi:ApiKey" "KEY_CUA_BAN" --project FootballBlog.API
   ```
6. Kiểm tra quota: Dashboard → Usage (free = 100 req/ngày, reset 00:00 UTC)

> **Lưu ý:** Header gửi là `x-apisports-key` (không phải `X-RapidAPI-Key`). Code đã đúng.

#### Telegram Bot + Channel

**Bước 1 — Tạo bot:**
1. Mở Telegram → tìm **@BotFather** → `/start`
2. Gõ `/newbot` → nhập tên bot (ví dụ: `Football Prediction Bot`)
3. Nhập username bot (phải kết thúc bằng `bot`, ví dụ: `myfootball_prediction_bot`)
4. BotFather trả về token dạng `7123456789:AAHxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx` → copy lại

**Bước 2 — Tạo channel và lấy ChannelId:**
1. Telegram → **New Channel** → đặt tên (ví dụ: `Football Predictions`)
2. Chọn **Private** channel
3. Vào channel → **Add Members** → tìm và thêm bot vừa tạo vào channel → set quyền **Admin** (để bot post được)
4. Lấy Channel ID:
   - Thêm bot [@userinfobot](https://t.me/userinfobot) vào channel (tạm thời)
   - Nó sẽ tự reply ID âm dạng `-1001234567890` → copy lại
   - Có thể remove @userinfobot sau khi lấy được ID

**Bước 3 — Set secrets:**
```bash
dotnet user-secrets set "Telegram:BotToken" "7123456789:AAHxxx..." --project FootballBlog.API
dotnet user-secrets set "Telegram:ChannelId" "-1001234567890" --project FootballBlog.API
```

**Kiểm tra bot hoạt động:**
```bash
curl "https://api.telegram.org/bot<TOKEN>/getMe"
# Kỳ vọng: {"ok":true,"result":{"username":"myfootball_prediction_bot",...}}
```

---

## UI / Design Workflow

1. Lấy tham khảo từ **URL web** (WebFetch) hoặc **ảnh screenshot** (paste vào chat)
2. Claude tạo **file HTML tĩnh** trong `FootballBlog.Web/wwwroot/prototype/`
3. Review trực tiếp trên browser — chỉnh DevTools nếu cần
4. Sau khi approve → tách thành **Blazor component**

**Design system hiện có:** xem `.claude/rules/ui-design.md`
**File prototype chính:** `wwwroot/prototype/home.html`

---

## Notes

- Ưu tiên SSR cho tất cả trang blog → SEO tốt
- Chỉ dùng InteractiveServer cho widget cần realtime hoặc admin
- Không dùng Blazor WASM (bundle nặng, SEO kém)
- Football API free tier: API-Football (100 req/ngày) — cache + rate limit ngay từ Phase 4
- AI Provider dùng interface (IAIPredictionProvider) để swap Claude/Gemini không cần sửa business logic
- Telegram message lưu ID để edit sau khi có kết quả thực
