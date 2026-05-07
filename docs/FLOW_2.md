# Flow chính (Production)

```
05:00 VN  FetchUpcomingMatchesJob (cron "0 5 * * *", timezone Asia/Ho_Chi_Minh)
           GET /fixtures?date=yyyy-MM-dd  ← 1 request/ngày (tất cả giải)
           Fetch 3 ngày: hôm qua / hôm nay / ngày mai
           Filter kết quả theo LeagueIds trong config (client-side)
           → hôm qua: UPDATE status + score (FT/AET/PEN)
           → hôm nay: UPSERT live/kết quả
           → ngày mai: INSERT mới → Schedule FetchH2HAsync tại KickoffUtc − 5h
           Commit sau mỗi ngày (nếu ngày 2 lỗi, ngày 1 vẫn lưu)

KO − 5h   PreMatchDataJob.FetchH2HAsync  [1 API req/trận]
           ├─ H2H 10 trận gần nhất → API
           ├─ HomeForm + AwayForm (5 trận) → DB, 0 req
           ├─ Fatigue (ngày nghỉ) → DB, 0 req
           ├─ Referee → Match.RefereeName, 0 req
           ├─ Save MatchContextData
           └─ Enqueue GeneratePredictionJob.ExecuteForMatchAsync

~ngay sau  GeneratePredictionJob.ExecuteForMatchAsync  (+ Cron.Hourly quét match chưa có prediction)
           Claude primary → Gemini fallback
           → Save MatchPrediction (Phase=PreMatch, RawResponse)
           → Schedule TelegramNotificationJob.SendPredictionAsync lúc 06:00 VN

06:00 VN  TelegramNotificationJob.SendPredictionAsync
           Check TelegramMessageId == null (idempotent)
           → Gửi prediction lên Telegram channel (message mới)
           → Update TelegramMessageId vào MatchPrediction (PreMatch)

Khi HT    LiveScorePollingJob (Cron.Minutely) phát hiện status chuyển sang HalfTime
           → Enqueue HalfTimePredictionJob.ExecuteAsync(matchId)

           HalfTimePredictionJob  [2 API req/trận]
           ├─ Check idempotent: bỏ qua nếu đã có HT prediction
           ├─ GET /fixtures/statistics?fixture={id}  ← 1 req (possession, shots, corners, cards)
           ├─ GET /fixtures/events?fixture={id}      ← 1 req (goals, cards, subs H1)
           ├─ Load MatchContextData (H2H, form, fatigue từ pre-match)
           ├─ Claude primary → Gemini fallback → PredictHalfTimeAsync
           ├─ Save MatchPrediction (Phase=HalfTime, RawResponse)
           └─ Enqueue TelegramNotificationJob.EditHalfTimeAsync

           TelegramNotificationJob.EditHalfTimeAsync
           → Lấy TelegramMessageId từ PreMatch prediction
           → Edit message gốc: thêm section "Phân tích H2"

Khi FT    LiveScorePollingJob phát hiện match biến mất khỏi live feed
           → UPDATE Match.Status = Finished, Match.HomeScore/AwayScore → DB only
           → Không gọi Gemini, không đụng Telegram
```

**Timezone:** Toàn bộ dùng VN timezone (`SE Asia Standard Time` Windows / `Asia/Ho_Chi_Minh` Linux). `KickoffUtc` trong DB vẫn lưu UTC — chỉ convert khi hiển thị UI.

**`FetchLineupsAsync`** — giữ trong `PreMatchDataJob` nhưng **không tự động schedule**. Trigger thủ công từ Hangfire dashboard nếu cần. Bỏ qua để tiết kiệm quota.

---

# Flow test thủ công (Admin `/admin/jobs`)

4 buttons để test từng bước độc lập. Bấm theo thứ tự khi cần debug:

```
[Fetch Matches] → [H2H] → [Gemini] → [Telegram]
```

| Button | Endpoint | Logic |
|--------|----------|-------|
| **Fetch Matches** | `POST /api/admin/matches/fetch` | Enqueue `FetchUpcomingMatchesJob.ExecuteAsync()` |
| **H2H** | `POST /api/admin/matches/trigger-h2h` | Query matches `Scheduled + KickoffUtc > now + ContextData == null` → enqueue `PreMatchDataJob.FetchH2HAsync` cho từng match |
| **Gemini** | `POST /api/admin/matches/predict-all` | Enqueue `GeneratePredictionJob.ExecuteAsync()` — batch scan PreMatch prediction chưa có |
| **Telegram** | `POST /api/admin/matches/trigger-telegram` | Query PreMatch predictions có `TelegramMessageId == null` → enqueue `SendPredictionAsync` |

> **HalfTimePredictionJob** không có button riêng — tự trigger từ LiveScorePollingJob khi phát hiện HT. Có thể enqueue thủ công từ Hangfire dashboard nếu cần test.

> **SeedLeagueDataJob** vẫn còn trong code, trigger thủ công từ Hangfire dashboard nếu cần seed lại league/team data.

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
1. keyRotator.GetAvailableKeyAsync()        → null nếu không có key
2. rateLimiter.TryConsumeAsync()            → false nếu > 10 req/phút (Redis)
3. usageTracker.CanCallAsync("FootballAPI") → false nếu đã đủ 100 req hôm nay (DB)
4. Gọi API
5. HandleRateLimitAsync()                   → parse 429/403, block key trong Redis
6. usageTracker.IncrementAsync("FootballAPI") → ghi +1 vào DB
```

Áp dụng cho: `GetFixturesByDateAsync`, `GetHeadToHeadAsync`, `GetFixtureHalfTimeDataAsync` (2 lần/trận).

## Budget thực tế

| Nguồn | Req/ngày |
|-------|---------|
| Date fetch (3 ngày × 1 req) | 3 (cố định) |
| H2H | ~5–15 (1 req × trận mới) |
| HT statistics + events | ~10–30 (2 req × trận live) |
| **Tổng** | **~18–48 / 100** |

---

## Status mapping

| API short | MatchStatus |
|-----------|-------------|
| NS | Scheduled |
| 1H, 2H, ET, P, LIVE, BT | Live |
| HT | HalfTime |
| FT, AET, PEN | Finished |
| PST | Postponed |
| SUSP, CANC, ABD, WO | Cancelled |

---

## MatchPrediction schema

| Field | Mô tả |
|-------|-------|
| `Phase` | `PreMatch` (0) hoặc `HalfTime` (1) — unique index trên `(MatchId, Phase)` |
| `RawResponse` | Raw text từ AI trước khi parse — dùng để audit và tune prompt |
| `TelegramMessageId` | Chỉ có ở PreMatch prediction — dùng để edit message khi HT |

---

# TODO còn lại

## 1. Nâng cấp AI Prediction
- Nâng Gemini prompt PreMatch ngang Claude (thêm H2H detail, Form detail, Fatigue, Referee)
- Extract `ParseResult` ra shared helper (duplicate giữa Claude + Gemini)
- Thêm `IApiUsageTracker` vào `GeminiAIPredictionProvider` (track Gemini quota)
- Tăng Claude `max_tokens` từ 1024 → 2048

## 2. Deploy
```powershell
fly secrets set --app footballblog-api "FootballApi__ApiKey=<key>"
fly secrets set --app footballblog-api "AI__Gemini__ApiKey=<key>"
git push origin master
fly logs --app footballblog-api
```
Trigger thủ công sau deploy: `/hangfire` → Recurring Jobs → `fetch-upcoming-matches` → Trigger now.
