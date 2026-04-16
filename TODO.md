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

### Phase 3 — Admin Panel 🔄 (In Progress)
- [x] Replace ApplicationUser → extend IdentityUser<int> + migration
- [x] ASP.NET Core Identity đã register (AddIdentity<ApplicationUser, IdentityRole<int>>)
- [x] Cookie Auth cho Blazor + JWT cho API (AuthController wire, Login/Logout pages)
- [x] Install MudBlazor 8.0.0 (chỉ load CSS/JS cho /admin routes)
- [x] AdminLayout.razor (MudBlazor shell — dark theme)
- [x] Admin pages: Dashboard, Posts list, Categories CRUD, Tags list
- [ ] Posts CRUD đầy đủ (create/edit với rich text editor)
- [ ] Rich text editor (Quill.js qua JS interop)
- [ ] Upload ảnh → MediaController → S3/Local

### Phase 4 — Realtime Football 🔄 (In Progress)
- [x] FootballApiClient (IHttpClientFactory + Polly retry)
- [x] Redis rate limit counter (100 req/ngày)
- [x] Match + MatchContext + MatchContextData entities
- [x] Country, League, Team entities + repositories (upsert by ExternalId)
- [x] MatchStatus enum
- [x] Hangfire jobs: FetchUpcomingMatchesJob (cron 6h), LiveScorePollingJob (1min adaptive), PreMatchDataJob (H2H + Lineups)
- [x] EF migration: RefactorMatchSchema (Country/League/Team/MatchContextData)
- [x] MatchEvent.Type → EventType enum + migration
- [x] ILiveScoreService implementation (LiveScoreService) + register DI
- [ ] SignalR Hub (LiveScoreHub) + Redis backplane
- [ ] Blazor LiveScore pages + widget (InteractiveServer)

### Phase 5 — AI Match Prediction ⬜
- [x] Domain: MatchContext (H2H, TeamForm, Lineup, Referee, Fatigue POCOs)
- [x] MatchContextData entity (JSONB blob, 1-to-1 với Match)
- [x] IMatchContextRepository + implementation
- [x] FixtureRawDto (raw API response mapping)
- [ ] IAIPredictionProvider interface + Claude implementation
- [ ] Prompt template lưu DB để A/B test
- [ ] Hangfire GeneratePredictionJob (trigger 24h trước kickoff)
- [ ] PublishPredictionJob → tạo blog post từ prediction
- [ ] Gemini implementation (fallback provider)

### Phase 6 — Telegram + Auto-publish ⬜
- [ ] Install Telegram.Bot NuGet
- [ ] ITelegramService: SendPredictionAsync, EditMessageAsync
- [ ] TelegramNotificationChannel implement INotificationChannel
- [ ] Bot command: /lichdat (query lịch đấu upcoming)
- [ ] Edit Telegram message khi kết quả thực tế về
- [ ] Admin page: xem prediction history, manual retrigger

### Phase 7 — Deploy & DevOps ⬜
- [ ] Dockerfile (multi-stage, Web + API)
- [ ] Railway deploy (dev/staging)
- [ ] GitHub Actions CI/CD pipeline
- [ ] AWS EC2 + RDS + S3 + CloudFront
- [ ] CloudWatch logging
- [ ] Monitoring: alert khi Football API gần hết quota

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
