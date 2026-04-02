# Football Blog — Claude Context

## Stack
ASP.NET Core 8 + Blazor (SSR + InteractiveServer) | PostgreSQL | Redis + SignalR | Hangfire | EF Core | Tailwind CSS (public) | MudBlazor (admin)

## CRITICAL: Render Mode
- Blog / Home / Post / Category / Tag → **Static SSR** — KHÔNG đặt @rendermode (SEO)
- Live score widget, tường thuật, admin → `@rendermode InteractiveServer`
- TUYỆT ĐỐI KHÔNG dùng Blazor WASM

## Architecture
- Web gọi API qua typed HttpClient (`IPostApiClient`, v.v.)
- Service Layer trong Core, inject qua `IUnitOfWork`
- Repository chỉ modify ChangeTracker — commit duy nhất qua `IUnitOfWork.CommitAsync()`
- DTOs trong Core/DTOs/ — KHÔNG expose domain entity ra ngoài service layer

## Key Service Abstractions (đã quyết định)
```
IAIPredictionProvider   — abstraction cho Claude/Gemini (swap không sửa business logic)
INotificationChannel    — abstraction cho Telegram/Email/webhook
ITelegramService        — gửi + edit message theo chatId
IFootballApiClient      — wrapper api-football.com với rate limit counter
```

## AI Prediction Pipeline
**Flow:** Football API → Hangfire collect job → DB → Hangfire trigger job → AI API → MatchPrediction → Blog post + Telegram notify

**Nguồn dữ liệu cho AI prompt:**
- Lineup/đội hình dự kiến (players ra sân)
- H2H 5 trận gần nhất
- Form hiện tại (5 trận gần nhất mỗi đội)
- Thông tin trọng tài
- Lịch thi đấu / mức độ mệt mỏi

**Entities:**
- `Match` — từ Football API, status: Scheduled | Live | Finished
- `MatchPrediction` — kết quả AI: provider, score dự đoán, confidence, analysis markdown, TelegramMessageId

**AI Providers:** Claude (claude-opus-4-6 default) hoặc Gemini — configurable qua appsettings

## Telegram Integration
- Package: `Telegram.Bot` (official NuGet)
- Gửi prediction khi AI xong → edit message khi kết quả thực tế về
- Có Channel riêng cho predictions + bot command query lịch đấu
- TelegramMessageId lưu trong MatchPrediction để edit sau

## Hangfire Jobs Pipeline
```
[Cron 6h]   FetchUpcomingMatchesJob    — lấy trận 48h tới từ Football API
[Cron 1h]   GeneratePredictionJob      — query Match chưa có prediction, gọi AI
[Trigger]   PublishPredictionJob        — tạo blog post SSR + gửi Telegram
[Cron 30s]  LiveScorePollingJob         — chỉ chạy khi có live match
[Cron 6h]   MatchScheduleJob            — đồng bộ lịch tổng quát
```

## Rate Limit Football API
- Free tier: 100 req/ngày
- Đếm request trong Redis, alert khi gần hết quota
- Cache response với TTL phù hợp — KHÔNG fetch real-time

## Dev Environment
- DB: `docker compose up` (postgres:5432, redis:6379)
- Tailwind: `npm install` + `npm run watch:css` trong FootballBlog.Web/
- Logs: solution root `/logs/` — xem `.claude/rules/logging.md`
- Secrets: `dotnet user-secrets` cho local, AWS Parameter Store cho production

## appsettings Structure (các section đã quyết định)
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

## Current Phase
Xem TODO.md. **Phase 1 xong.** Tiếp theo: Phase 2 (Blog SSR + SEO).

## Deploy
Railway (dev) → AWS EC2 + RDS + S3 + CloudFront (prod) | CI/CD: GitHub Actions
