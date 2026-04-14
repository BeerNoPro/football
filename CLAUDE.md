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

## Token Optimization
- **Search**: `grep_search` (50 tokens) ≫ `semantic_search` (500+ tokens)
- **Read file**: Always grep first → read narrow range (not full file)
- **Conversation**: Close at 15-20 messages → use `/cleanup` → new tab (resets context)

---

## appsettings Hiện Tại (thực tế đã có — Phase 4)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "",
    "Redis": "localhost:6379"
  },
  "WebBaseUrl": "",
  "FootballApi": {
    "BaseUrl": "https://v3.football.api-sports.io",
    "ApiKey": "",
    "DailyRequestLimit": 100,
    "FixturesPerLeague": 20,
    "LeagueIds": [39, 140, 135, 78, 61, 94, 2, 3, 848, 531, 1, 45, 48, 253, 307]
  },
  "Serilog": { "MinimumLevel": { "Default": "Information" } }
}
```
> Dev overrides: `appsettings.Development.json` (không commit) — set `DefaultConnection`, `WebBaseUrl="https://localhost:7241"`, `FootballApi:ApiKey`.
> Secrets: `dotnet user-secrets set "FootballApi:ApiKey" "YOUR_KEY"`

## appsettings Phase 5-6 (chưa có, sẽ thêm khi implement)
```json
{
  "AI": {
    "DefaultProvider": "Claude",
    "Claude": { "ApiKey": "", "Model": "claude-opus-4-6", "MaxTokens": 2000 },
    "Gemini": { "ApiKey": "", "Model": "gemini-2.0-flash" }
  },
  "Telegram": { "BotToken": "", "DefaultChatId": "", "PredictionChannelId": "" },
  "Prediction": { "GenerateHoursBeforeKickoff": 24, "AutoPublishPost": true }
}
```

## Service Abstractions Phase 4-6
```
IFootballApiClient      — [Phase 4] ✅ implemented (FootballApiClient + RedisRateLimiter)
ILiveScoreService       — [Phase 4] ❌ interface exists, NO implementation yet
IAIPredictionProvider   — [Phase 5] ❌ abstraction Claude/Gemini
INotificationChannel    — [Phase 6] ❌ abstraction Telegram/Email
ITelegramService        — [Phase 6] ❌ gửi + edit message theo chatId
```

## Hangfire Jobs Pipeline
```
[Cron 6h]   FetchUpcomingMatchesJob    ✅ — lấy trận 48h tới từ Football API
[Cron 1min] LiveScorePollingJob        ✅ — adaptive gate, chỉ poll khi có live match trong DB
[Scheduled] PreMatchDataJob            ✅ — H2H (5h trước kickoff) + Lineups (15min trước)
[Cron 1h]   GeneratePredictionJob      ❌ — Phase 5: query Match chưa có prediction, gọi AI
[Trigger]   PublishPredictionJob        ❌ — Phase 5: tạo blog post + gửi Telegram
```

## Cleanup Sau Khi Implement

**Bắt buộc sau khi hoàn thành bất kỳ task/plan nào:**

```bash
# Xóa plan đã hoàn tất (chạy ngay, không hỏi lại)
rm .claude/plans/<plan-file>.md

# Xóa nhiều plan thừa cùng lúc
rm .claude/plans/file1.md .claude/plans/file2.md
```

Quy tắc:
1. **Plan đã implement xong** → xóa ngay bằng bash, không cần xin phép
2. **Plan implement một phần** → ghi phần còn lại vào TODO.md rồi xóa plan
3. **Plan trùng nội dung nhau** → giữ 1 cái mới nhất, xóa các bản cũ
4. **Plan là "analysis/reference" không còn dùng** → xóa, nội dung quan trọng đã nằm trong code/TODO
5. **Khi đọc HTML prototype để phân tích structure / logic / navigation**
- Đọc từ đầu file tới comment <!-- STYLES --> là đủ
- Không cần đọc <style> block trừ khi task yêu cầu sửa CSS cụ thể

Khi thêm config/rule mới vào `.claude/`:
- Kiểm tra xem thông tin đó đã có ở file khác chưa (CLAUDE.md, rules/, commands/)
- Nếu trùng lặp → gộp hoặc xóa bản cũ

## Current Phase
Xem **TODO.md** để biết phase hiện tại và task cụ thể.
Xem **Bugs.md** để biết architectural decisions và known issues.

## Deploy
Railway (dev) → AWS EC2 + RDS + S3 + CloudFront (prod) | CI/CD: GitHub Actions
