# Football Blog — Claude Context

## Project Overview
Trang blog bóng đá fullstack C#, vừa xây dựng vừa học.
Mục tiêu: Blog SEO tốt + Live score realtime + Admin panel.

## Tech Stack
- **Backend/Frontend**: ASP.NET Core 8 + Blazor (SSR + InteractiveServer)
- **Database**: PostgreSQL (Docker local, RDS trên AWS)
- **Cache + Realtime**: Redis + SignalR
- **Background Jobs**: Hangfire (polling Football API)
- **ORM**: Entity Framework Core
- **CSS**: Tailwind CSS
- **Design**: Figma → Blazor components

## Render Mode Rules (quan trọng cho SEO)
- Trang Blog, Home, Bài viết → **Static SSR** (HTML đầy đủ cho Google)
- Live score widget, tường thuật → **InteractiveServer** (SignalR)
- Admin panel → **InteractiveServer** (không cần SEO)
- KHÔNG dùng Blazor WASM

## Project Structure
```
FootballBlog/
├── FootballBlog.Web/            # Blazor UI
├── FootballBlog.API/            # Web API + SignalR Hubs + Hangfire Jobs
├── FootballBlog.Core/           # Business Logic, Models, Interfaces
└── FootballBlog.Infrastructure/ # EF Core, Repositories
```

## Development Environment
- Local DB: Docker Compose (PostgreSQL port 5432, Redis port 6379)
- Run app: `dotnet run` trong FootballBlog.Web
- IDE: VS Code (edit + AI) + Visual Studio 2022 (debug)
- DB client: DBeaver

## Coding Conventions
- Dùng async/await cho tất cả DB và HTTP calls
- Repository pattern cho data access
- Dependency Injection theo chuẩn ASP.NET Core
- Tên file, class theo PascalCase; biến theo camelCase
- Comment bằng tiếng Việt nếu logic phức tạp

## Current Phase
Xem TODO.md để biết phase hiện tại và tasks đang làm.

## Deploy
- Dev/staging: Railway (free tier, auto deploy từ GitHub)
- Production: AWS EC2 + RDS + S3 + CloudFront
- CI/CD: GitHub Actions

## Football API
- Provider: API-Football (https://www.api-football.com)
- Free tier: 100 requests/ngày
- Polling interval: 30 giây khi có live match
- Hangfire job xử lý polling, SignalR broadcast xuống client

## Key Files
- `TODO.md` — roadmap và tasks theo phase
- `CLAUDE.md` — file này, context cho Claude
- `docker-compose.yml` — local dev environment
- `appsettings.Development.json` — config local (không commit secret)
