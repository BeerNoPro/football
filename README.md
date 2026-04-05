# Football Blog

Blog bóng đá fullstack — SEO tốt · Live score realtime · Admin panel · AI match prediction.

**Stack:** ASP.NET Core 8 · Blazor SSR/InteractiveServer · PostgreSQL · Redis · SignalR · Hangfire

---

## Quick Start

```bash
# 1. Khởi động DB local
docker compose up -d

# 2. Apply migrations
dotnet ef database update --project FootballBlog.Infrastructure --startup-project FootballBlog.API

# 3. Chạy
dotnet run --project FootballBlog.API    # https://localhost:7007
dotnet run --project FootballBlog.Web    # https://localhost:7241
```

---

## Tài liệu

| File | Nội dung |
|------|---------|
| [docs/FLOW.md](docs/FLOW.md) | Cách hệ thống hoạt động — request flow, data pipeline, AI prediction |
| [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) | Cài đặt, chạy lần đầu, migrations, lệnh hay dùng |
| [docs/DEPLOY.md](docs/DEPLOY.md) | Deploy Railway/AWS, cấu hình API keys |
| [.claude/README.md](.claude/README.md) | Claude Code setup, hooks, slash commands |
| [TODO.md](TODO.md) | Roadmap theo phases, task hiện tại |
| [Bugs.md](Bugs.md) | Architectural decisions, known issues |

---

## Tech Stack

| Layer | Công nghệ |
|-------|-----------|
| Backend | ASP.NET Core 8 (C#) |
| Frontend | Blazor SSR + InteractiveServer |
| Database | PostgreSQL 16 |
| Cache + Realtime | Redis 7 + SignalR |
| Background Jobs | Hangfire |
| ORM | Entity Framework Core 8 + Npgsql |
| CSS (public) | Tailwind CSS |
| CSS (admin) | MudBlazor |
| Football API | API-Football (100 req/ngày free tier) |
| AI | Claude API / Google Gemini (switchable) |
| Notification | Telegram Bot |
| Logging | Serilog → Console + File |
| Local Dev | Docker Compose |
| CI/CD | GitHub Actions |
| Deploy | Railway (staging) · AWS EC2 + RDS (prod) |
