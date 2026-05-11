# Flow chính (Production) ⚠️ chưa bật — đang test thủ công

> **Trạng thái hiện tại (2026-05-10):** Tất cả Recurring Jobs đã **tắt** trong `appsettings.json` (`Jobs.*: false`). Các chuỗi tự trigger giữa jobs đã **bị cắt** để test từng bước độc lập qua Admin UI. Bật lại khi production-ready.

```
05:00 VN  FetchUpcomingMatchesJob (cron "0 5 * * *", timezone Asia/Ho_Chi_Minh)
           GET /fixtures?date=yyyy-MM-dd  ← 1 request/ngày (tất cả giải)
           Fetch 3 ngày: hôm qua / hôm nay / ngày mai
           Filter kết quả theo LeagueIds trong config (client-side)
           → hôm qua: UPDATE status + score (FT/AET/PEN) + HT/ET/Pen score + Round/Venue/Referee
           → hôm nay: UPSERT live/kết quả
           → ngày mai: INSERT mới
           Commit sau mỗi ngày (nếu ngày 2 lỗi, ngày 1 vẫn lưu)
           [production] → Schedule PreMatchDataJob.FetchH2HAsync tại KickoffUtc − 5h (chỉ PremiumLeagues)
           [production] → Enqueue FetchPostMatchDataJob cuối run (trận đã FT chưa có stats)

KO − 5h   PreMatchDataJob.FetchH2HAsync  [1 API req/trận — chỉ PremiumLeagues]
           ├─ H2H 10 trận gần nhất → API
           ├─ HomeForm + AwayForm (5 trận) → DB, 0 req
           ├─ Fatigue (ngày nghỉ) → DB, 0 req
           ├─ Referee → Match.RefereeName, 0 req
           ├─ Save MatchContextData
           └─ [production] Enqueue GeneratePredictionJob.ExecuteForMatchAsync

~ngay sau  GeneratePredictionJob.ExecuteForMatchAsync  (+ Cron.Hourly quét match chưa có prediction)
           Gemini primary → Claude fallback  [chỉ PremiumLeagues]
           → Save MatchPrediction (Phase=PreMatch, RawResponse)
           → Throw nếu cả 2 fail → Hangfire retry
           → [production] Schedule TelegramNotificationJob.SendPredictionAsync lúc 06:00 VN

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
           ├─ Gemini primary → Claude fallback → PredictHalfTimeAsync
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

**`FetchLineupsAsync`** — giữ trong `PreMatchDataJob` nhưng không schedule. Trigger thủ công từ Hangfire dashboard nếu cần. Bỏ qua để tiết kiệm quota.

**`FetchSquadJob`** — trigger thủ công từ Admin UI. Chỉ fetch đội có trận trong 7 ngày tới, chỉ premium leagues, bỏ qua đội đã có squad.

### Bật lại production flow (khi test xong)

1. `appsettings.json` → set `Jobs.FetchUpcomingMatches: true`, `Jobs.GeneratePrediction: true`, `Jobs.LiveScorePolling: true`
2. `FetchUpcomingMatchesJob` → uncomment `BackgroundJob.Schedule<PreMatchDataJob>` (×2) và `Enqueue<FetchPostMatchDataJob>`
3. `PreMatchDataJob` → uncomment `BackgroundJob.Enqueue<GeneratePredictionJob>`
4. `GeneratePredictionJob` → uncomment `BackgroundJob.Schedule<TelegramNotificationJob>`

---

# Flow test thủ công (Admin `/admin/jobs`)

6 buttons, mỗi button làm đúng 1 việc, **không tự kéo theo job khác**. Bấm theo thứ tự khi cần debug:

```
[Fetch Matches] → [Fetch Squads] → [H2H] → [Gemini] → [Telegram]
                                              ↑
                                   [Post-Match Stats] (độc lập, dùng sau FT)
```

| Button | Endpoint | Job được enqueue | Làm gì |
|--------|----------|-----------------|--------|
| **Fetch Matches** | `POST /api/admin/matches/fetch` | `FetchUpcomingMatchesJob` | Upsert fixtures 3 ngày vào DB |
| **Fetch Squads** | `POST /api/admin/matches/fetch-squads` | `FetchSquadJob` | Fetch squad cho đội có trận 7 ngày tới (premium leagues) |
| **H2H** | `POST /api/admin/matches/trigger-h2h` | `PreMatchDataJob.FetchH2HAsync` | Fetch H2H + form → lưu MatchContextData |
| **Gemini** | `POST /api/admin/matches/predict-all` | `GeneratePredictionJob` | Batch gen prediction cho match chưa có (cần ContextData) |
| **Telegram** | `POST /api/admin/matches/trigger-telegram` | `TelegramNotificationJob.SendPredictionAsync` | Gửi prediction chưa có TelegramMessageId |
| **Post-Match Stats** | `POST /api/admin/matches/fetch-post-match` | `FetchPostMatchDataJob` | Fetch stats + events cho trận đã FT chưa có data |

> **HalfTimePredictionJob** không có button riêng — tự trigger từ LiveScorePollingJob khi phát hiện HT. Test thủ công qua Hangfire Dashboard.

> **SeedLeagueDataJob** — trigger thủ công từ Hangfire Dashboard nếu cần seed lại league/team data.

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

Áp dụng cho: `GetFixturesByDateAsync`, `GetHeadToHeadAsync`, `GetFixtureHalfTimeDataAsync` (2 lần/trận), `GetSquadByTeamAsync`.

## Budget thực tế

| Nguồn | Req/ngày |
|-------|---------|
| Date fetch (3 ngày × 1 req) | 3 (cố định) |
| H2H | ~5–15 (1 req × trận mới, premium only) |
| HT statistics + events | ~10–30 (2 req × trận live) |
| Squad fetch | ~0–20 (thủ công, 1 req/đội chưa có squad) |
| **Tổng** | **~18–68 / 100** |

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

## Match score fields

| Field | Điền khi nào |
|-------|-------------|
| `HomeScore` / `AwayScore` | Full-time (luôn có sau FT) |
| `HtHomeScore` / `HtAwayScore` | Sau half-time (API điền lúc HT trở đi) |
| `EtHomeScore` / `EtAwayScore` | Chỉ khi status = AET hoặc PEN |
| `PenHomeScore` / `PenAwayScore` | Chỉ khi status = PEN |

Tất cả nullable. Update logic dùng null-guard: chỉ ghi đè khi API trả về giá trị không null.

---

# Trạng Thái Thực Tế (2026-05-10)


## Checklist việc còn lại

- [ ] **[P0]** Khởi động lại API để `ApiKeySeeder` seed Claude/Gemini keys vào DB
- [ ] **[P0]** Trigger [Fetch Squads] từ Admin UI để seed squad data
- [ ] **[P0]** Trigger [Gemini] từ Admin UI để gen prediction cho matches hôm nay
- [ ] **[P0]** Apply migration `AddMatchStatusKickoffIndex` trên production (`dotnet ef database update`)
- [ ] **[P1]** Nâng Gemini `BuildPrompt` ngang Claude (thêm H2H detail, form matches, fatigue, referee)
- [ ] **[P1]** Extract `ParseResult` ra shared `AIPredictionResultParser` (bỏ duplicate giữa 2 providers)
- [ ] **[P2]** Bổ sung Standing data vào `MatchContext` và AI prompt (rank, points — 0 API call)
- [ ] **[P2]** Fix `BuildFatigue()`: tính `HomePlayingEurope`/`AwayPlayingEurope` từ DB thay vì hardcode false

---

# Hướng dẫn khởi động sau fix

```powershell
# Bước 1 — Đảm bảo user-secrets đã set
dotnet user-secrets set "Gemini:ApiKey" "<your-gemini-key>" --project FootballBlog.API
dotnet user-secrets set "Claude:ApiKey" "<your-claude-key>" --project FootballBlog.API
dotnet user-secrets set "Telegram:BotToken" "<your-bot-token>" --project FootballBlog.API
dotnet user-secrets set "Telegram:ChannelId" "<channel-id>" --project FootballBlog.API

# Bước 2 — Restart API → ApiKeySeeder tự seed Claude/Gemini vào DB
# (Không cần xóa row cũ nữa — fix đã check per-provider)

# Bước 3 — Admin UI → test theo thứ tự
# [Fetch Matches] → [Fetch Squads] → [H2H] → [Gemini] → [Telegram]
```

---

# Deploy (Phase 7)

```powershell
fly secrets set --app footballblog-api "FootballApi__ApiKey=<key>"
fly secrets set --app footballblog-api "Gemini__ApiKey=<key>"
fly secrets set --app footballblog-api "Claude__ApiKey=<key>"
fly secrets set --app footballblog-api "Telegram__BotToken=<token>"
fly secrets set --app footballblog-api "Telegram__ChannelId=<id>"
git push origin master
fly logs --app footballblog-api
```

Trigger thủ công sau deploy: `/hangfire` → Recurring Jobs → `fetch-upcoming-matches` → Trigger now.
