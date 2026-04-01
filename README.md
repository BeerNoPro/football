# Football Blog

Blog bóng đá fullstack — SEO tốt + Live score realtime + Admin panel.  
Xây dựng với ASP.NET Core 8 + Blazor, PostgreSQL, Redis, SignalR, Hangfire.

---

## Mục lục

- [Tổng quan](#tổng-quan)
- [Tech Stack](#tech-stack)
- [Cấu trúc dự án](#cấu-trúc-dự-án)
- [Yêu cầu môi trường](#yêu-cầu-môi-trường)
- [Cài đặt & Chạy lần đầu](#cài-đặt--chạy-lần-đầu)
- [Database & Migrations](#database--migrations)
- [Claude Code Agent](#claude-code-agent)
- [Hooks đã cấu hình](#hooks-đã-cấu-hình)
- [Lệnh thường dùng](#lệnh-thường-dùng)
- [Quy tắc Blazor Render Mode](#quy-tắc-blazor-render-mode)
- [Coding Conventions](#coding-conventions)
- [Lộ trình phát triển](#lộ-trình-phát-triển)
- [Deploy](#deploy)

---

## Tổng quan

| Tính năng | Mô tả |
|-----------|-------|
| Blog SEO | Trang bài viết render Static SSR — HTML đầy đủ cho Google |
| Live Score | Widget realtime qua SignalR, poll từ API-Football mỗi 30 giây |
| Admin Panel | CRUD bài viết, upload ảnh, quản lý danh mục/tag |
| Background Jobs | Hangfire polling Football API, tự động update live score |

---

## Tech Stack

| Layer | Công nghệ |
|-------|-----------|
| Backend | ASP.NET Core 8 (C#) |
| Frontend | Blazor (SSR + InteractiveServer) |
| Database | PostgreSQL 16 |
| Cache + Realtime | Redis 7 + SignalR |
| Background Jobs | Hangfire + Hangfire.PostgreSql |
| ORM | Entity Framework Core 8 + Npgsql |
| CSS | Tailwind CSS + @tailwindcss/typography |
| Football API | API-Football (100 req/ngày free tier) |
| Logging | Serilog → Console + File |
| Local Dev | Docker Compose |
| Deploy Dev | Railway (auto từ GitHub) |
| Deploy Prod | AWS EC2 + RDS + S3 + CloudFront |
| CI/CD | GitHub Actions |

---

## Cấu trúc dự án

```
FootballBlog/
├── FootballBlog.Web/                  # Blazor UI (SSR + InteractiveServer)
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Blog/                  # SSR — SEO pages
│   │   │   │   ├── Index.razor        # /blog
│   │   │   │   ├── PostDetail.razor   # /bai-viet/{slug}
│   │   │   │   └── Category.razor     # /chu-de/{slug}
│   │   │   ├── LiveScore/             # InteractiveServer — Realtime
│   │   │   │   ├── Index.razor        # /live
│   │   │   │   └── MatchDetail.razor  # /live/{matchId}
│   │   │   ├── Admin/                 # InteractiveServer — CRUD
│   │   │   │   ├── Dashboard.razor
│   │   │   │   ├── Posts/
│   │   │   │   └── Account/Login.razor
│   │   │   ├── Home.razor             # / — SSR
│   │   │   ├── Sitemap.razor          # /sitemap.xml
│   │   │   └── Robots.razor           # /robots.txt
│   │   ├── Layout/
│   │   └── Shared/
│   │       ├── SeoHead.razor          # <title>, og:image, canonical
│   │       ├── ArticleSchema.razor    # JSON-LD schema.org
│   │       ├── LiveScoreWidget.razor  # Widget nhúng vào Home
│   │       └── RichTextEditor.razor   # Quill.js editor
│   └── wwwroot/
│       ├── css/app.css                # Tailwind build output
│       └── js/quill-interop.js
│
├── FootballBlog.API/                  # Web API + SignalR + Hangfire
│   ├── Controllers/
│   │   └── MediaController.cs        # POST /api/media/upload
│   ├── Hubs/
│   │   └── LiveScoreHub.cs           # SignalR hub
│   └── Jobs/
│       ├── LiveScorePollingJob.cs    # Hangfire — poll 30s khi có live match
│       └── MatchScheduleJob.cs       # Hangfire — lấy schedule hàng ngày
│
├── FootballBlog.Core/                 # Business Logic (không phụ thuộc gì ngoài .NET)
│   ├── Models/                        # Domain models (POCO)
│   │   ├── Post.cs
│   │   ├── Category.cs
│   │   ├── Tag.cs
│   │   ├── PostTag.cs
│   │   ├── ApplicationUser.cs
│   │   ├── LiveMatch.cs
│   │   └── MatchEvent.cs
│   ├── Interfaces/                    # Contracts
│   │   ├── IRepository.cs
│   │   ├── IPostRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── ITagRepository.cs
│   │   ├── ILiveMatchRepository.cs
│   │   ├── IPostService.cs
│   │   ├── ICacheService.cs
│   │   ├── IStorageService.cs
│   │   └── IFootballApiClient.cs
│   ├── Services/                      # Business logic
│   │   ├── PostService.cs
│   │   ├── SlugService.cs
│   │   └── CategoryService.cs
│   └── DTOs/
│       ├── PostSummaryDto.cs
│       ├── PostDetailDto.cs
│       └── CategoryDto.cs
│
├── FootballBlog.Infrastructure/       # Data Access + External Services
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── BaseRepository.cs
│   │   ├── PostRepository.cs
│   │   ├── CategoryRepository.cs
│   │   ├── TagRepository.cs
│   │   └── LiveMatchRepository.cs
│   └── Services/
│       ├── RedisCacheService.cs
│       ├── FootballApiClient.cs
│       ├── LocalStorageService.cs     # Dev
│       └── S3StorageService.cs        # Production
│
├── .claude/                           # Claude Code config
│   ├── settings.json                  # Hooks, permissions (commit vào git)
│   ├── hooks/                         # Hook scripts
│   │   ├── build-check.sh
│   │   ├── dbcontext-check.sh
│   │   └── stop-notify.sh
│   └── rules/                         # Coding rules cho Claude
│
├── docker-compose.yml                 # PostgreSQL + Redis local
├── FootballBlog.sln
├── CLAUDE.md                          # Context cho Claude agent
└── TODO.md                            # Roadmap theo phases
```

---

## Yêu cầu môi trường

| Tool | Version | Ghi chú |
|------|---------|---------|
| .NET SDK | 8.x | `dotnet --version` |
| Docker Desktop | 4.x+ | Cần WSL2 trên Windows |
| Node.js | 18+ | Cho Tailwind CSS build |
| Git | 2.x+ | |
| DBeaver (tuỳ chọn) | | Xem database |

**Cài EF Core CLI** (một lần duy nhất):
```bash
dotnet tool install --global dotnet-ef
```

---

## Cài đặt & Chạy lần đầu

### 1. Clone repo

```bash
git clone <repo-url>
cd football
```

### 2. Khởi động database local

```bash
docker compose up -d
```

Kiểm tra:
```bash
docker ps
# football-postgres-1   Up   0.0.0.0:5432->5432/tcp
# football-redis-1      Up   0.0.0.0:6379->6379/tcp
```

### 3. Tạo database schema

```bash
dotnet ef database update \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### 4. Build Tailwind CSS (Phase 2 trở đi)

```bash
cd FootballBlog.Web/wwwroot
npm install
npm run build:css
```

### 5. Chạy ứng dụng

**Chạy API:**
```bash
cd FootballBlog.API
dotnet run
# API: https://localhost:7001
# Health: https://localhost:7001/health
```

**Chạy Web:**
```bash
cd FootballBlog.Web
dotnet run
# Web: https://localhost:7000
```

### 6. Kết nối DBeaver

```
Host:     localhost
Port:     5432
Database: footballblog
User:     admin
Password: localpass
```

---

## Database & Migrations

### Schema hiện tại

| Table | Mô tả |
|-------|-------|
| `Posts` | Bài viết — slug unique, PublishedAt nullable (null = draft) |
| `Categories` | Danh mục — slug unique |
| `Tags` | Tag — slug unique |
| `PostTags` | Many-to-many Posts ↔ Tags |
| `Users` | Tài khoản — role: Admin/Author |
| `LiveMatches` | Trận đấu live — ExternalId từ Football API |
| `MatchEvents` | Sự kiện trận đấu (bàn thắng, thẻ, thay người) |

### Tạo migration mới

```bash
dotnet ef migrations add <TenMigration> \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API \
  --output-dir Data/Migrations
```

### Apply migration

```bash
dotnet ef database update \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### Rollback migration

```bash
# Rollback về migration trước
dotnet ef database update <TenMigrationTruoc> \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API

# Xoá migration cuối
dotnet ef migrations remove \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### Xem SQL trước khi apply (khuyến nghị cho production)

```bash
dotnet ef migrations script \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API \
  --output migration.sql
```

---

## Claude Code Agent

Dự án này được phát triển cùng **Claude Code** — AI coding agent của Anthropic.

### Cài đặt Claude Code

```bash
npm install -g @anthropic-ai/claude-code
claude
```

### Context files quan trọng

| File | Mục đích |
|------|---------|
| `CLAUDE.md` | Context tổng quan dự án — Claude đọc mỗi session |
| `TODO.md` | Roadmap và trạng thái từng phase |
| `.claude/rules/` | Coding rules (code-style, logging, blazor, database, api, security, testing) |
| `.claude/commands/` | Custom slash commands |
| `.claude/settings.json` | Hooks và permissions |

### Custom Slash Commands

| Command | Tác dụng |
|---------|---------|
| `/new-feature` | Tạo feature mới theo đúng cấu trúc project |
| `/migration` | Hướng dẫn tạo EF Core migration |
| `/review` | Review code trước khi commit |
| `/test` | Chạy tests |
| `/debug-log` | Đọc và phân tích log files |

Ví dụ:
```
/new-feature Tạo trang danh sách bài viết theo tag
/review FootballBlog.Core/Services/PostService.cs
/migration Thêm cột ViewCount vào bảng Posts
```

---

## Hooks đã cấu hình

Hooks chạy tự động, không cần làm gì thêm. Xem/sửa tại `.claude/settings.json`.

### Hook 1 — Auto Build Check

**Trigger:** Sau mỗi lần Claude edit/write file `.cs`  
**Tác dụng:** Chạy `dotnet build` trong background, hiển thị kết quả nếu có lỗi  
**Script:** `.claude/hooks/build-check.sh`

```
Editing Post.cs...
  [Building...]
  Build succeeded.        ← hoặc: error CS0246: Type not found
```

### Hook 2 — DbContext Migration Reminder

**Trigger:** Sau khi Claude sửa file chứa `DbContext` trong tên  
**Tác dụng:** Hiển thị reminder tạo EF migration  
**Script:** `.claude/hooks/dbcontext-check.sh`

```
DbContext changed -- remember to add EF migration:
  dotnet ef migrations add <Name> --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

### Hook 3 — Stop Notification

**Trigger:** Khi Claude dừng làm việc  
**Tác dụng:** Nhắc nhở verify build trước khi tiếp tục  
**Script:** `.claude/hooks/stop-notify.sh`

```
Claude stopped. Verify with: dotnet build --no-restore -v q
```

### Vô hiệu hoá hook tạm thời

Thêm vào `.claude/settings.json`:
```json
"disableAllHooks": true
```

Hoặc xoá riêng từng hook trong mảng `hooks.PostToolUse`.

---

## Lệnh thường dùng

### Build & Run

```bash
# Build toàn bộ solution
dotnet build

# Build nhanh (không restore)
dotnet build --no-restore -v q

# Chạy API
dotnet run --project FootballBlog.API

# Chạy Web
dotnet run --project FootballBlog.Web

# Chạy với watch (hot reload)
dotnet watch --project FootballBlog.Web
```

### Docker

```bash
# Khởi động services
docker compose up -d

# Dừng services
docker compose down

# Xem logs
docker compose logs -f postgres
docker compose logs -f redis

# Xoá data (reset hoàn toàn)
docker compose down -v
```

### Database

```bash
# Tạo migration
dotnet ef migrations add <TenMigration> \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API

# Apply migration
dotnet ef database update \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API

# Xem danh sách migrations
dotnet ef migrations list \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### Tailwind CSS (Phase 2+)

```bash
cd FootballBlog.Web/wwwroot

# Build một lần
npm run build:css

# Watch mode (dev)
npm run watch:css
```

### Logs

Logs được ghi tự động bởi Serilog:

```
logs/
├── app/app-YYYYMMDD.log      # Toàn bộ (Information+)
└── error/error-YYYYMMDD.log  # Chỉ Error + Fatal
```

```bash
# Xem log realtime
tail -f logs/app/app-$(date +%Y%m%d).log

# Tìm lỗi
grep "error\|ERR\|FTL" logs/app/app-$(date +%Y%m%d).log
```

---

## Quy tắc Blazor Render Mode

**QUAN TRỌNG — Ảnh hưởng trực tiếp đến SEO.**

| Trang | Render Mode | Cách khai báo |
|-------|-------------|---------------|
| Home `/` | Static SSR | Không khai báo `@rendermode` |
| Blog `/blog` | Static SSR | Không khai báo `@rendermode` |
| Bài viết `/bai-viet/{slug}` | Static SSR | Không khai báo `@rendermode` |
| Danh mục, Tag | Static SSR | Không khai báo `@rendermode` |
| Sitemap, Robots | Static SSR | Không khai báo `@rendermode` |
| Live Score widget | InteractiveServer | `@rendermode InteractiveServer` trên component |
| Admin tất cả pages | InteractiveServer | `@rendermode InteractiveServer` trong `@attribute` |

**Quy tắc:**
- KHÔNG set `@rendermode` global ở `App.razor` hay `Routes.razor`
- KHÔNG inject `IHttpContextAccessor` vào component dùng chung SSR + InteractiveServer
- Một SSR page CÓ THỂ chứa InteractiveServer child component (ví dụ: LiveScoreWidget trên Home)

---

## Coding Conventions

### Naming

```csharp
// Class, Method, Property → PascalCase
public class PostService { }
public async Task<Post> GetBySlugAsync(string slug) { }

// Variable, parameter → camelCase
var postId = 1;
string slugText = "bai-viet-moi";

// Interface → prefix I
public interface IPostRepository { }

// Private field → prefix _
private readonly ApplicationDbContext _dbContext;
```

### Async/Await

```csharp
// ĐÚNG
public async Task<Post?> GetBySlugAsync(string slug)
    => await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);

// SAI — gây deadlock
var post = repository.GetBySlugAsync(slug).Result;
```

### Logging (Serilog)

```csharp
// ĐÚNG — structured, searchable
_logger.LogInformation("Post created {@Post}", new { Id = post.Id, Slug = post.Slug });
_logger.LogWarning("Post not found for slug {Slug}", slug);

// SAI — không filter được
_logger.LogInformation("Post created: " + post.Id + " - " + post.Slug);
```

### Error Handling

```csharp
// Bắt lỗi ở tầng Controller/API, không ở Service
try
{
    var post = await _postService.CreateAsync(dto);
    return Ok(post);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to create post for user {UserId}", userId);
    return StatusCode(500, "Internal server error");
}
```

---

## Lộ trình phát triển

### Phase 1 — Setup & Foundation ✅

- [x] Solution 4 projects + references
- [x] Docker Compose (PostgreSQL + Redis)
- [x] Core Models (Post, Category, Tag, User, LiveMatch, MatchEvent)
- [x] Repository Pattern (IRepository + implementations)
- [x] EF Core + migration `InitialCreate`
- [x] Serilog logging (Console + File)
- [x] Claude Code hooks

### Phase 2 — Blog Core (SEO) ⬜

- [ ] DTOs (PostSummaryDto, PostDetailDto)
- [ ] PostService, SlugService (tiếng Việt → slug)
- [ ] Blazor SSR pages: Home, Blog list, Post detail, Category
- [ ] SEO: SeoHead component, Open Graph, canonical URL
- [ ] Schema.org JSON-LD cho bài viết
- [ ] Sitemap.xml, robots.txt
- [ ] Tailwind CSS setup
- [ ] LocalStorageService (upload ảnh local dev)

### Phase 3 — Admin Panel ⬜

- [ ] ASP.NET Core Identity (cookie auth)
- [ ] Admin pages: Dashboard, Posts CRUD
- [ ] Rich text editor (Quill.js qua JS interop)
- [ ] Upload ảnh → MediaController → S3/Local
- [ ] Slug auto-generate từ title tiếng Việt

### Phase 4 — Realtime Football ⬜

- [ ] FootballApiClient (IHttpClientFactory + Polly retry)
- [ ] Redis rate limit counter (API-Football 100 req/ngày)
- [ ] Hangfire jobs (LiveScorePollingJob, MatchScheduleJob)
- [ ] SignalR Hub (LiveScoreHub) + Redis backplane
- [ ] Blazor LiveScore pages + widget (InteractiveServer)

### Phase 5 — Deploy & DevOps ⬜

- [ ] Dockerfile (multi-stage, Web + API)
- [ ] Railway deploy (dev/staging)
- [ ] GitHub Actions CI/CD
- [ ] AWS EC2 + RDS + S3 + CloudFront
- [ ] CloudWatch logging

---

## Deploy

### Local Development

```
Web:      https://localhost:7000
API:      https://localhost:7001
PostgreSQL: localhost:5432
Redis:    localhost:6379
```

### Staging — Railway

Railway tự động deploy khi push lên branch `main`.

**Environment variables cần set trên Railway:**
```
ConnectionStrings__DefaultConnection=postgresql://...
ConnectionStrings__Redis=redis://...
FootballApi__ApiKey=<key>
ASPNETCORE_ENVIRONMENT=Staging
```

### Production — AWS

| Service | Mục đích |
|---------|---------|
| EC2 t3.small | Chạy Web + API containers |
| RDS PostgreSQL t3.micro | Database |
| S3 | Media uploads |
| CloudFront | CDN cho static assets |
| ACM | SSL certificate |

**Lưu ý bảo mật:**
- Không commit `appsettings.Production.json`
- Dùng AWS Secrets Manager hoặc SSM Parameter Store cho secrets
- EC2 Instance Profile cho S3 access (không dùng access key)

---

## Cấu trúc Connection String

**Local** (`appsettings.json` trong API project):
```
Host=localhost;Port=5432;Database=footballblog;Username=admin;Password=localpass
```

**Production** (environment variable):
```
Host=<rds-endpoint>;Port=5432;Database=footballblog;Username=<user>;Password=<pass>;SSL Mode=Require
```
