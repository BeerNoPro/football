# Flow new

```
05:00 VN  FetchUpcomingMatchesJob (cron "0 5 * * *", timezone Asia/Ho_Chi_Minh)
           GET /fixtures?date=yyyy-MM-dd&timezone=Asia/Ho_Chi_Minh  ← 1 request/ngày (tất cả giải)
           Fetch 3 ngày: hôm qua / hôm nay / ngày mai
           Filter kết quả theo LeagueIds trong config (client-side)
           → hôm qua: UPDATE status + score (FT/AET/PEN)
           → hôm nay: UPSERT live/kết quả
           → ngày mai: INSERT mới → Schedule FetchH2HAsync tại KickoffUtc − 5h
           Commit sau mỗi ngày (granularity: nếu ngày 2 lỗi, ngày 1 vẫn lưu)

KO − 5h   PreMatchDataJob.FetchH2HAsync  [1 API req/trận]
           ├─ H2H 10 trận gần nhất → API
           ├─ HomeForm + AwayForm (5 trận) → DB, 0 req
           ├─ Fatigue (ngày nghỉ) → DB, 0 req
           ├─ Referee → Match.RefereeName, 0 req
           ├─ Save MatchContextData
           └─ Enqueue GeneratePredictionJob.ExecuteForMatchAsync

~ngay sau  GeneratePredictionJob.ExecuteForMatchAsync  (Cron.Hourly quét match chưa có prediction)
           Claude primary → Gemini fallback
           → Save MatchPrediction
           → Enqueue TelegramNotificationJob.SendPredictionAsync

06:00 VN  TelegramNotificationJob gửi prediction lên channel
```

**Timezone:** Toàn bộ dùng VN timezone (`SE Asia Standard Time` Windows / `Asia/Ho_Chi_Minh` Linux). `KickoffUtc` trong DB vẫn lưu UTC — chỉ convert khi hiển thị UI.

**`FetchLineupsAsync`** — giữ trong `PreMatchDataJob` nhưng **không tự động schedule**. Có thể trigger thủ công từ Hangfire dashboard nếu cần. Bỏ qua để tiết kiệm quota.

---

# Rate Limit & Quota Tracking

**Giới hạn Football API:** Daily 100 req, per-minute 10 req.

## Hai lớp bảo vệ

| Lớp | Impl | Scope | Mục đích |
|-----|------|-------|---------|
| Per-minute | `RedisFootballApiRateLimiter` | Redis, in-memory TTL | Chặn burst > 10 req/phút |
| Daily | `ApiUsageTracker` + bảng `ApiUsageDaily` | PostgreSQL | Enforce hard limit 100 req/ngày |

## Flow trong `FootballApiClient`

```
1. keyRotator.GetAvailableKeyAsync()     → null nếu không có key
2. rateLimiter.TryConsumeAsync()         → false nếu > 10 req/phút (Redis)
3. usageTracker.CanCallAsync("FootballAPI") → false nếu đã đủ 100 req hôm nay (DB)
4. Gọi API
5. HandleRateLimitAsync()                → parse 429/403, block key trong Redis
6. usageTracker.IncrementAsync("FootballAPI") → ghi +1 vào DB
```

`CanCallAsync` + `IncrementAsync` áp dụng cho: `GetFixturesByDateAsync`, `GetHeadToHeadAsync`.

**`ApiUsageTracker` dùng `IDbContextFactory`** — tạo `DbContext` riêng biệt, không share với UoW của job → `SaveChangesAsync` chỉ commit đúng record usage, không flush ChangeTracker của business transaction.

## `ApiUsageDaily` — bảng DB

Limits cấu hình tại `appsettings.json > ApiLimits`:
```json
"ApiLimits": {
  "FootballAPI": 100,
  "Gemini": 1500,
  "Telegram": 0
}
```
`DailyLimit = 0` = unlimited (Telegram không cần track).

## Budget thực tế

| Nguồn | Req/ngày |
|-------|---------|
| Date fetch (3 ngày × 1 req) | 3 (cố định) |
| H2H | ~5–15 (1 req × trận mới có H2H time > now) |
| **Tổng** | **~8–18 / 100** |

---

## Status mapping

| API short | MatchStatus |
|-----------|-------------|
| NS | Scheduled |
| 1H, 2H, HT, ET, P, LIVE, BT | Live |
| FT, AET, PEN | Finished |
| PST | Postponed |
| SUSP, CANC, ABD, WO | Cancelled |

---

# TODO còn lại

## 1. Refactor 4 buttons Admin Job Monitor (làm trước — session mới)

Admin page `/admin/jobs` hiện có 3 buttons, cần đổi thành 4 buttons theo flow thực tế:

### Button 1 — "Fetch Matches" ✅ Giữ nguyên
- Logic: `FetchUpcomingMatchesJob.ExecuteAsync()`
- Endpoint: `POST api/admin/matches/fetch` ✅ đã có

### Button 2 — "H2H" ❌ Cần đổi từ "Seed League Data"
- Đổi tên button + method trong: `Index.razor`, `IAdminApiClient`, `AdminApiClient`, `AdminMatchesController`
- Endpoint cũ `seed-leagues` → đổi thành `trigger-h2h`
- Logic mới: quét tất cả Match `Scheduled`, `KickoffUtc > now`, chưa có `MatchContextData`
  → với mỗi match: `BackgroundJob.Enqueue<PreMatchDataJob>(j => j.FetchH2HAsync(externalId, homeExtId, awayExtId))`
- Cần inject `IUnitOfWork` + `IMatchRepository` vào controller (hoặc tạo service method)
- **Lưu ý:** `SeedLeagueDataJob` vẫn giữ trong code, chỉ bỏ button admin (có thể trigger từ Hangfire nếu cần seed)

### Button 3 — "Gemini" ❌ Cần đổi tên từ "Run Predictions"
- Logic đã đúng: `GeneratePredictionJob.ExecuteAsync()` (batch AI prediction, Claude primary → Gemini fallback)
- Chỉ cần đổi tên ở: `Index.razor` (label), `IAdminApiClient` (method name), `AdminApiClient`, `AdminMatchesController`
- Endpoint: `predict-all` → giữ nguyên hoặc đổi thành `trigger-gemini`

### Button 4 — "Telegram" ❌ Cần build mới
- Endpoint mới: `POST api/admin/matches/trigger-telegram`
- Logic: quét tất cả `MatchPrediction` có `TelegramMessageId == null`
  → với mỗi prediction: `BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendPredictionAsync(p.Id))`
- Thêm method `TriggerTelegramAsync()` vào `IAdminApiClient` + `AdminApiClient`
- Thêm button "Telegram" vào `Index.razor`

### Files cần sửa
```
FootballBlog.API/Controllers/AdminMatchesController.cs   ← đổi seed-leagues, thêm trigger-h2h + trigger-telegram
FootballBlog.Web/ApiClients/IAdminApiClient.cs           ← đổi tên method, thêm TriggerTelegramAsync + TriggerH2HAsync
FootballBlog.Web/ApiClients/AdminApiClient.cs            ← tương tự IAdminApiClient
FootballBlog.Web/Components/Pages/Admin/Jobs/Index.razor ← đổi tên 2 button, thêm button Telegram
```

---

## 2. Nâng cấp AI Prediction (sau khi test xong)
- Nâng Gemini prompt ngang Claude (thêm H2H detail, Form detail, Fatigue, Referee)
- Extract `ParseResult` ra shared helper (duplicate giữa Claude + Gemini)
- Thêm `IApiUsageTracker` vào `GeminiAIPredictionProvider` (track Gemini quota)
- Tăng Claude `max_tokens` từ 1024 → 2048

## 3. Set secrets lên Fly.io
```powershell
fly secrets set --app footballblog-api "FootballApi__ApiKey=<key>"
fly secrets set --app footballblog-api "AI__Gemini__ApiKey=<key>"
```

## 4. Deploy + verify
```powershell
git push origin master
fly logs --app footballblog-api
```
Trigger thủ công: `/hangfire` → Recurring Jobs → `fetch-upcoming-matches` → Trigger now.
