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
| UI Design | Figma → Blazor components |
| Local Dev | Docker Compose |
| Hosting (dev) | Railway (free tier) |
| Hosting (prd) | AWS EC2 + RDS + S3 + CloudFront |
| CDN / DNS | Cloudflare |
| IDE | VS Code (AI + edit) + Visual Studio (debug) |

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

## Database Schema (Draft)

- **Posts** — id, title, slug, content, thumbnail, category_id, author_id, published_at, created_at
- **Categories** — id, name, slug
- **Tags** — id, name, slug
- **PostTags** — post_id, tag_id
- **Users** — id, username, email, password_hash, role (admin/author)
- **LiveMatches** — id, home_team, away_team, score, status, started_at
- **MatchEvents** — id, match_id, minute, type (goal/card/sub), description

---

## Phases

### Phase 1 — Setup & Foundation
- [ ] Khởi tạo solution ASP.NET Core 8 + Blazor
- [ ] Cấu hình Docker Compose (PostgreSQL + Redis)
- [ ] Setup Entity Framework Core + migrations
- [ ] Cấu hình môi trường dev (appsettings, secrets)

### Phase 2 — Blog Core (SEO)
- [ ] Blazor SSR pages: Home, Bài viết, Danh mục, Tag
- [ ] SEO: meta tags, Open Graph, sitemap.xml, robots.txt
- [ ] Schema.org JSON-LD cho bài viết thể thao
- [ ] Upload ảnh lên S3 (hoặc local khi dev)

### Phase 3 — Admin Panel
- [ ] Authentication / Authorization (role: admin, author)
- [ ] CRUD bài viết (editor Markdown hoặc rich text)
- [ ] Quản lý danh mục, tag
- [ ] Dashboard traffic cơ bản

### Phase 4 — Realtime Football
- [ ] Tích hợp Football API bên ngoài (API-Football / SportMonks)
- [ ] Hangfire job polling live score mỗi 30 giây
- [ ] SignalR Hub broadcast live score xuống client
- [ ] Blazor LiveScore widget (InteractiveServer)
- [ ] Tường thuật trực tiếp realtime

### Phase 5 — Deploy & DevOps
- [ ] Deploy lên Railway (free, từ GitHub)
- [ ] Cấu hình domain + Cloudflare DNS + HTTPS
- [ ] GitHub Actions CI/CD pipeline
- [ ] Chuyển sang AWS: EC2 + RDS + S3 + CloudFront
- [ ] Monitoring logs + alerts cơ bản

---

## UI / Design Workflow

1. Thiết kế màn hình trên **Figma**
2. Dùng **Figma MCP** trong Claude Code để đọc design trực tiếp
3. Claude generate Blazor component từ Figma spec
4. Apply Tailwind CSS / Bootstrap theo design token từ Figma

---

## Pages cần design trên Figma

- [ ] Home — danh sách bài viết nổi bật, live score widget
- [ ] Trang bài viết — nội dung, SEO layout
- [ ] Trang danh mục / tag
- [ ] Live Score / Tường thuật trực tiếp
- [ ] Admin Dashboard
- [ ] Admin — Tạo / Sửa bài viết

---

## Notes

- Ưu tiên SSR cho tất cả trang blog → SEO tốt
- Chỉ dùng InteractiveServer cho widget cần realtime hoặc admin
- Không dùng Blazor WASM (bundle nặng, SEO kém)
- Football API free tier: API-Football (100 req/ngày)
