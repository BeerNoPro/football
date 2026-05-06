# League IDs — api-football.com

Danh sách 30 giải đấu đang fetch. Tất cả đã được cấu hình trong `appsettings.json > FootballApi.LeagueIds`.

---

## UEFA / FIFA
- Champions League: 2
- Europa League: 3
- Conference League: 848
- Nations League: 531
- World Cup: 1

## Anh
- Premier League: 39
- FA Cup: 45
- EFL Cup (Carabao): 48

## Tây Ban Nha
- La Liga: 140

## Ý
- Serie A: 135

## Đức
- Bundesliga: 78

## Pháp
- Ligue 1: 61

## Bồ Đào Nha
- Primeira Liga: 94

## Saudi Arabia
- Pro League: 307

## Mỹ
- MLS: 253

## Việt Nam
- V.League 1: 340
- V.League 2: 637
- Cup: 341
- Super Cup: 831

## Hà Lan
- Eredivisie: 88

## Bỉ
- Pro League: 144

## Scotland
- Premiership: 179

## Thổ Nhĩ Kỳ
- Süper Lig: 203

## Mexico
- Liga MX: 262

## Brazil
- Série A: 71

## Argentina
- Liga Profesional: 128

## Nhật Bản
- J1 League: 98

## Hàn Quốc
- K League 1: 292

## Úc
- A-League: 188

## Nga
- Premier League: 235

---

## appsettings.json

```json
"LeagueIds": [ 2, 3, 848, 531, 1, 39, 45, 48, 140, 135, 78, 61, 94, 307, 253, 340, 637, 341, 831, 88, 144, 179, 203, 262, 71, 128, 98, 292, 188, 235 ]
```

---

# Flow hàng ngày

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

## 1. Test Football API local (làm trước)
Trigger thủ công qua Hangfire dashboard (`/hangfire`):
- `fetch-upcoming-matches` → Trigger now → kiểm tra log + DB có fixtures không
- `generate-predictions` → Trigger now → kiểm tra MatchPrediction được tạo không
- Xem bảng `ApiUsageDaily` có tăng đúng không

## 2. Nâng cấp AI Prediction (sau khi test API xong)
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
