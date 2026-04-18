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

### Phase 6.5 — API Key Management ✅
- [x] Entity `ApiKeyConfig` (Provider, KeyValue, Priority, IsActive, DailyLimit, Note, CreatedAt)
- [x] EF migration: `AddApiKeyConfig`
- [x] `IApiKeyRotator` — query DB theo Priority, skip key bị block (Redis flag) hoặc vượt DailyLimit
- [x] Redis cache key list (TTL 5 phút) + blocked flag (TTL đến midnight UTC khi nhận 429/403)
- [x] `ApiKeySeeder` — seed từ appsettings/user-secrets vào DB lần đầu startup (guard: chỉ chạy nếu bảng trống)
- [x] Admin page: `/admin/api-keys` — list theo provider, toggle IsActive, thêm/xóa key
- [x] Cập nhật `FootballApiClient`, `ClaudeAIPredictionProvider`, `GeminiAIPredictionProvider` dùng `IApiKeyRotator`
- [x] Xóa `ApiKey` khỏi `appsettings.json` và `FootballApiOptions`

> **Để re-seed:** xóa tất cả rows trong bảng `ApiKeyConfigs` → restart API → seeder tự chạy lại.
> **Không lưu DB:** `ConnectionStrings`, `Jwt:Key`, `Redis` — infra config, giữ nguyên trong appsettings/user-secrets.

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
| `FootballApi` | `ApiKey` | API key từ api-sports.io — **chỉ dùng để seed lần đầu, sau đó xóa** | Phase 6.5 seed |
| `Claude` | `ApiKey` | Anthropic API key — **chỉ dùng để seed lần đầu, sau đó xóa** | Phase 6.5 seed |
| `Gemini` | `ApiKey` | Google AI Studio API key — **chỉ dùng để seed lần đầu, sau đó xóa** | Phase 6.5 seed |
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
-> dotnet user-secrets set "Jwt:Key" "footballblog-jwt-secret-key-dev-2026!!" --project FootballBlog.API
- **Claude:ApiKey** — [console.anthropic.com](https://console.anthropic.com) → API Keys
- **Gemini:ApiKey** — [aistudio.google.com](https://aistudio.google.com) → Get API Key
b
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

### CSS Cleanup: Dùng MudBlazor Theme (không hardcode)

**Approach: kết hợp cả 2**
- MudBlazor props (`Color=`, `Typo=`, `Variant=`) cho **màu chữ và nút**
- CSS class (`.admin-card`, `.admin-header`) trong `app.css` cho **layout pattern** (border, border-radius, background card)
- Inline `Style=` chỉ giữ lại cho **spacing động** (padding/margin/gap cụ thể)

**Mapping màu theme → MudBlazor enum:**
| Hex hiện tại | Thay bằng |
|---|---|
| `color:#c8f04d` | `Color="Color.Primary"` |
| `color:#efefef` | `Color="Color.Default"` (default text) |
| `color:#666` | `Color="Color.Dark"` hoặc `Typo.caption` |
| `background:#1c1c1c` | → dùng `MudPaper` với `Elevation="0"` (theme tự đặt surface) |
| `background:#c8f04d;color:#0d0d0d` | `Color="Color.Primary" Variant="Variant.Filled"` |

**Trường hợp vẫn cần inline Style=:**
- Layout spacing: `Style="padding:24px;margin-bottom:16px"`
- Border custom: `Style="border:1px solid #242424;border-radius:10px"` — extract sang CSS isolation hoặc CSS var

**File sẽ thay đổi:**
- `Pages/Admin/Dashboard.razor`
- `Pages/Admin/Posts/Index.razor`
- `Pages/Admin/Posts/Create.razor`
- `Pages/Admin/Posts/Edit.razor`
- `Pages/Admin/Categories/Index.razor`
- `Pages/Admin/Tags/Index.razor`
- `Pages/Admin/Matches/Index.razor`
- `Pages/Admin/Predictions/Index.razor`
- `Pages/Admin/Prompts/Index.razor`
- `Pages/Admin/Settings/Index.razor`
- `Pages/Admin/ApiKeys/Index.razor`
- `Components/Layout/AdminLayout.razor` (thêm CSS vars cho border/radius nếu cần)

---

### Extract Shared Admin Components

**Tạo trong `FootballBlog.Web/Components/Admin/`:**

**Thứ tự: làm AdminPageHeader trước → confirm visual → mới tiếp.**

#### 3a. `AdminPageHeader.razor`
```razor
@* Parameters: Title, Description, Buttons (RenderFragment) *@
<div style="display:flex;align-items:center;justify-content:space-between;margin-bottom:24px;flex-wrap:wrap;gap:12px;">
    <div>
        <MudText Typo="Typo.h5" Color="Color.Default" Style="font-weight:700;margin-bottom:4px;">@Title</MudText>
        <MudText Typo="Typo.body2" Color="Color.Dark">@Description</MudText>
    </div>
    <div style="display:flex;gap:8px;align-items:center;">
        @Buttons
    </div>
</div>
```
→ Dùng tại: Dashboard, Posts, Categories, Tags, ApiKeys, Matches, Predictions, Prompts, Settings

#### 3b. `AdminStatCard.razor`
```razor
@* Parameters: Label, Value (string), Icon (optional) *@
<MudItem xs="6" sm="3">
    <MudPaper Class="admin-stat-card">
        <MudText Typo="Typo.caption" Color="Color.Dark" Style="text-transform:uppercase;letter-spacing:1px;">@Label</MudText>
        <MudText Typo="Typo.h5" Color="Color.Default" Style="font-weight:700;margin-top:6px;">@Value</MudText>
    </MudPaper>
</MudItem>
```
→ Dùng tại: Matches/Index, Predictions/Index

#### 3c. CSS Isolation cho border/card pattern
Tạo `Components/Admin/Admin.razor.css` (hoặc global trong `app.css`):
```css
.admin-card { background: var(--mud-palette-surface); border: 1px solid #242424; border-radius: 10px; }
.admin-stat-card { background: var(--mud-palette-surface); border: 1px solid #242424; border-radius: 10px; padding: 16px 20px; }
```

---

## Notes

- Ưu tiên SSR cho tất cả trang blog → SEO tốt
- Chỉ dùng InteractiveServer cho widget cần realtime hoặc admin
- Không dùng Blazor WASM (bundle nặng, SEO kém)
- Football API free tier: API-Football (100 req/ngày) — cache + rate limit ngay từ Phase 4
- AI Provider dùng interface (IAIPredictionProvider) để swap Claude/Gemini không cần sửa business logic
- Telegram message lưu ID để edit sau khi có kết quả thực
