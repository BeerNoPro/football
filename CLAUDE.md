# Football Blog — Claude Context

## Stack
ASP.NET Core 8 + Blazor (SSR + InteractiveServer) | PostgreSQL | Redis + SignalR | Hangfire | EF Core | Tailwind CSS (public) | MudBlazor (admin)

## Architecture Nhanh
- **4 projects**: Web → API → Core ← Infrastructure
- Web gọi API qua typed HttpClient (`IPostApiClient`, `ICategoryApiClient`)
- Repository chỉ modify ChangeTracker — commit duy nhất qua `IUnitOfWork.CommitAsync()`
- DTOs trong `Core/DTOs/` — KHÔNG expose entity ra ngoài service layer
- `GetBySlugAsync (Post)`: chỉ trả published (`PublishedAt != null`) — draft KHÔNG lộ public

## IUnitOfWork Properties (quick ref)
```csharp
uow.Posts           // IPostRepository
uow.Categories      // ICategoryRepository
uow.Tags            // ITagRepository
uow.LiveMatches     // ILiveMatchRepository
uow.Matches         // IMatchRepository
uow.MatchPredictions // IMatchPredictionRepository
```

## Dev Environment
- DB: `docker compose up` (postgres:5432, redis:6379)
- Tailwind: `npm install` + `npm run watch:css` trong `FootballBlog.Web/`
- Logs: solution root `/logs/` — xem `.claude/rules/logging.md`
- Secrets: `dotnet user-secrets` (local) | AWS Parameter Store (prod)
- **Dev ports**: API `https://localhost:7007` | Web `https://localhost:7241`
- **EF migration**: `--project FootballBlog.Infrastructure --startup-project FootballBlog.API`

## appsettings Hiện Tại (thực tế đã có)
```json
{
  "ConnectionStrings": { "DefaultConnection": "" },
  "WebBaseUrl": "https://localhost:7241",
  "ApiBaseUrl": "https://localhost:7007",
  "Serilog": { "MinimumLevel": { "Default": "Information" } }
}
```

## appsettings Phase 4-6 (chưa có, sẽ thêm khi implement)
```json
{
  "FootballApi": { "BaseUrl": "", "ApiKey": "", "DailyRequestLimit": 100 },
  "AI": {
    "DefaultProvider": "Claude",
    "Claude": { "ApiKey": "", "Model": "claude-opus-4-6", "MaxTokens": 2000 },
    "Gemini": { "ApiKey": "", "Model": "gemini-2.0-flash" }
  },
  "Telegram": { "BotToken": "", "DefaultChatId": "", "PredictionChannelId": "" },
  "Prediction": { "GenerateHoursBeforeKickoff": 24, "AutoPublishPost": true }
}
```

## Service Abstractions Phase 4-6 (CHƯA implement)
```
IAIPredictionProvider   — [Phase 5] abstraction Claude/Gemini
INotificationChannel    — [Phase 6] abstraction Telegram/Email
ITelegramService        — [Phase 6] gửi + edit message theo chatId
IFootballApiClient      — [Phase 4] wrapper api-football.com + rate limit
```

## Hangfire Jobs Pipeline (CHƯA implement — Phase 4-5)
```
[Cron 6h]   FetchUpcomingMatchesJob    — lấy trận 48h tới từ Football API
[Cron 1h]   GeneratePredictionJob      — query Match chưa có prediction, gọi AI
[Trigger]   PublishPredictionJob        — tạo blog post + gửi Telegram
[Cron 30s]  LiveScorePollingJob         — chỉ chạy khi có live match
```

## Cleanup Sau Khi Implement

Khi implement xong một feature từ plan file trong `.claude/plans/`:
1. Xóa file plan đó — source code là source of truth, plan đã implement là dead weight
2. Nếu plan chỉ implement một phần, ghi chú phần còn lại vào TODO.md rồi xóa plan

Khi thêm config/rule mới vào `.claude/`:
- Kiểm tra xem thông tin đó đã có ở file khác chưa (CLAUDE.md, rules/, commands/)
- Nếu trùng lặp → gộp hoặc xóa bản cũ

## Current Phase
Xem **TODO.md** để biết phase hiện tại và task cụ thể.
Xem **Bugs.md** để biết architectural decisions và known issues.

## Deploy
Railway (dev) → AWS EC2 + RDS + S3 + CloudFront (prod) | CI/CD: GitHub Actions
