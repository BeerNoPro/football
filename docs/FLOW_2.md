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

---

# Chi Tiết Từng Job

## Bảng tổng hợp

| Job | File | Schedule | Football API calls | Downstream jobs |
|-----|------|----------|--------------------|----------------|
| FetchUpcomingMatchesJob | Jobs/FetchUpcomingMatchesJob.cs | Cron `0 5 * * *` (05:00 VN) | 3 req/ngày (`fixtures?date`) | PreMatchDataJob (per match, KO−5h), FetchPostMatchDataJob |
| PreMatchDataJob | Jobs/PreMatchDataJob.cs | BackgroundJob.Schedule (KO−5h) | 1 req/match (H2H) | GeneratePredictionJob |
| GeneratePredictionJob | Jobs/GeneratePredictionJob.cs | Per-match + Cron hourly batch | 0 req (AI: Gemini→Claude) | TelegramNotificationJob (06:00 VN) |
| TelegramNotificationJob | Jobs/TelegramNotificationJob.cs | 06:00 VN (PreMatch) / immediate (HT) | 0 req (Telegram Bot API) | — |
| LiveScorePollingJob | Jobs/LiveScorePollingJob.cs | Cron.Minutely() | 1 req/phút (`live=all`), skip nếu không có live match trong DB | HalfTimePredictionJob (khi HT), FetchPostMatchDataJob (khi FT) |
| HalfTimePredictionJob | Jobs/HalfTimePredictionJob.cs | BackgroundJob.Enqueue (khi HT detect) | 2 req/match (stats + events) | TelegramNotificationJob.EditHalfTimeAsync |
| FetchPostMatchDataJob | Jobs/FetchPostMatchDataJob.cs | Ad-hoc (enqueue từ FetchUpcoming/LiveScore hoặc Admin UI) | 2 req/match, max 15 matches (30 req max) | — |
| SeedLeagueDataJob | Jobs/SeedLeagueDataJob.cs | Manual (Admin UI / Hangfire Dashboard) | 3 req/league (teams + fixtures + standings) | — |
| FetchSquadJob | Jobs/FetchSquadJob.cs | Manual (Admin UI / Hangfire Dashboard) | 1 req/team chưa có squad | — |

---

## FetchUpcomingMatchesJob

**Schedule:** Cron `0 5 * * *` (05:00 VN, UTC+7)

**Logic:**
- Fetch fixtures cho 3 ngày: hôm qua / hôm nay / ngày mai
- Filter theo `FootballApi.LeagueIds` trong config (client-side)
- Upsert: Countries → Leagues → Teams → Matches
- Dùng in-memory cache per-run (`countryCache`, `leagueCache`, `teamCache`) để tránh duplicate DB lookup
- Commit sau mỗi ngày (nếu ngày 2 lỗi, ngày 1 vẫn được lưu)
- Abort nếu API trả null (quota hit)

**Downstream (production — hiện bị comment):**
- `BackgroundJob.Schedule<PreMatchDataJob>` per upcoming match tại `KickoffUtc − 5h`
- `BackgroundJob.Enqueue<FetchPostMatchDataJob>` cho các trận đã FT chưa có stats

---

## PreMatchDataJob

**Schedule:** `BackgroundJob.Schedule` từ FetchUpcomingMatchesJob (KO−5h)

**Logic (FetchH2HAsync):**
1. Fetch H2H 10 trận gần nhất — 1 API call (`fixtures/headtohead?h2h=homeId-awayId`)
2. Build HomeForm + AwayForm (5 trận gần nhất) — 0 API call, từ DB
3. Tính Fatigue (ngày nghỉ kể từ trận trước) — 0 API call, từ DB
4. Lấy RefereeName từ `Match.RefereeName` — 0 API call
5. Serialize `MatchContext` → JSON → lưu vào `MatchContextData` table

**MatchContext structure:**
```
H2HContext: RecentMatches(10), HomeWins, Draws, AwayWins
HomeForm / AwayForm: TeamName, RecentMatches(5), FormString, GoalsScored, GoalsConceded
RefereeContext: Name
FatigueContext: HomeDaysSinceLastMatch, AwayDaysSinceLastMatch
LineupContext: null (FetchLineupsAsync bị skip để tiết kiệm quota)
```

**Downstream (production — hiện bị comment):**
- `BackgroundJob.Enqueue<GeneratePredictionJob>` per match

---

## GeneratePredictionJob

**Schedule:** Per-match (enqueue từ PreMatchDataJob) + Cron hourly batch scan

**Logic (ExecuteForMatchAsync):**
1. Load Match + MatchContextData từ DB
2. Idempotency check: skip nếu prediction đã tồn tại
3. Skip nếu match status ≠ Scheduled
4. Call AI: **Gemini (primary, 1500 req/ngày free)** → **Claude (fallback)**
5. Save `MatchPrediction` (Phase=PreMatch)
6. Throw nếu cả 2 fail → Hangfire auto-retry

**Logic (ExecuteAsync — hourly batch):**
- Query tất cả matches premium leagues chưa có PreMatch prediction
- Gọi ExecuteForMatchAsync per match

**Downstream (production — hiện bị comment):**
- `BackgroundJob.Schedule<TelegramNotificationJob.SendPredictionAsync>` lúc 06:00 VN

---

## TelegramNotificationJob

**Schedule:**
- `SendPredictionAsync`: BackgroundJob.Schedule lúc 06:00 VN từ GeneratePredictionJob
- `EditHalfTimeAsync`: BackgroundJob.Enqueue ngay từ HalfTimePredictionJob

**Logic (SendPredictionAsync):**
1. Load MatchPrediction
2. Idempotency: skip nếu `TelegramMessageId != null`
3. Gửi prediction lên Telegram channel (Telegram.Bot v22, MarkdownV2)
4. Lưu `TelegramMessageId` vào MatchPrediction

**Logic (EditHalfTimeAsync):**
1. Load HT MatchPrediction
2. Lấy `TelegramMessageId` từ PreMatch prediction
3. Edit message gốc: append section "Phân tích H2"

---

## LiveScorePollingJob

**Schedule:** `Cron.Minutely()` (UTC timezone)

**Adaptive gate:** Kiểm tra DB trước — exit early nếu không có live match (0 API cost)

**Logic:**
1. `GetAllLiveFixturesAsync()` — 1 req cho toàn bộ live matches
2. Upsert `LiveMatch` table (score, status, minute)
3. Detect HalfTime: status chuyển sang HT → enqueue HalfTimePredictionJob
4. Detect FullTime: match biến mất khỏi live feed → update `Match.Status = Finished`, update score
5. Broadcast via SignalR → group `match-{matchId}`

**Downstream:**
- `BackgroundJob.Enqueue<HalfTimePredictionJob>` per match khi phát hiện HT
- `BackgroundJob.Enqueue<FetchPostMatchDataJob>` nếu có bất kỳ trận nào FT

---

## HalfTimePredictionJob

**Schedule:** BackgroundJob.Enqueue từ LiveScorePollingJob (khi status = HalfTime)

**Logic:**
1. Idempotency: skip nếu HT prediction đã tồn tại
2. Load Match + MatchContextData (context từ PreMatch)
3. `GetFixtureHalfTimeDataAsync()` — 2 req (statistics + events H1)
4. Call AI: **Gemini (primary)** → **Claude (fallback)** với `PredictHalfTimeAsync`
5. Save `MatchPrediction` (Phase=HalfTime)

**HalfTimeContext:**
```
H1 statistics: possession%, shots on target, corners, fouls, cards
H1 events: goals, red/yellow cards, substitutions (với phút)
PreMatch context: H2H, form, fatigue (từ MatchContextData)
```

**Downstream:**
- `BackgroundJob.Enqueue<TelegramNotificationJob.EditHalfTimeAsync>`

---

## FetchPostMatchDataJob

**Schedule:** Ad-hoc — enqueue từ FetchUpcomingMatchesJob, LiveScorePollingJob, hoặc Admin UI

**Logic:**
1. Query tối đa 15 finished matches chưa có stats (premium leagues only)
2. `GetFixturePostMatchDataAsync(externalId)` — 2 req/match (stats + events)
3. Lưu `Match.StatsJson` + `Match.EventsJson` (raw JSON)
4. Commit sau mỗi match
5. Abort nếu API trả null (quota hit)

**Giới hạn 15 match:** 2 req/match × 15 = 30 req max, tránh exhaust daily quota (100/ngày)

---

## SeedLeagueDataJob

**Schedule:** Manual only (Admin UI hoặc Hangfire Dashboard)

**Logic (per league, 3 bước):**
1. `GetTeamsByLeagueAsync(leagueId, season)` — Upsert Venue + Team
2. `GetFixturesByRangeAsync(leagueId, season, from, to)` — Upsert Country, League, Team, Match
3. `GetStandingsAsync(leagueId, season)` — Upsert Standing (rank, points, W/D/L, goals, form)

**Retry logic (FetchWithRetryAsync):**
- Network error: exponential backoff 5s→10s→…→120s, deadline 10 phút
- Rate limit (null response): retry 3 lần, wait 65s mỗi lần, sau đó abort

---

## FetchSquadJob

**Schedule:** Manual only (Admin UI hoặc Hangfire Dashboard)

**Logic:**
1. Query matches trong 7 ngày tới (premium leagues only)
2. Extract unique teams (~20 max)
3. Skip team đã có squad trong DB
4. `GetSquadByTeamAsync(team.ExternalId)` — 1 req/team mới
5. Upsert Player + SquadMember

---

# Infrastructure Services

## RedisFootballApiRateLimiter
**File:** `FootballBlog.Infrastructure/Services/RedisFootballApiRateLimiter.cs`

- Redis key: `apikey:usage:FootballApi:perminute:{yyyy-MM-dd-HH-mm}`, TTL 65s
- Giới hạn: 10 req/phút (per-minute gate, chặn burst trước khi API trả 429)
- Fail-open: Redis unavailable → cho phép request (log warning)

## ApiKeyRotator
**File:** `FootballBlog.Infrastructure/Services/ApiKeyRotator.cs`

- Load keys từ `ApiKeyConfigs` table, cache Redis 5 phút
- Multi-key rotation: trả key đầu tiên còn available (order by Priority)
- Block key ngay lập tức trên Redis khi nhận 429/403 (65s cho per-minute, đến ngày mai cho daily)
- `InvalidateCacheAsync()`: force reload từ DB (gọi sau khi add/remove key qua Admin UI)

## ApiUsageTracker
**File:** `FootballBlog.Infrastructure/Services/ApiUsageTracker.cs`

- Daily hard limit lưu trong bảng `ApiUsageDaily` (Date + Service)
- Atomic `ExecuteUpdateAsync` để tránh race condition
- Limits từ config: `FootballAPI=100`, `Gemini=1500`, `Telegram=0` (unlimited)

## FootballApiClient — Flow mỗi API call
**File:** `FootballBlog.API/ApiClients/FootballApi/FootballApiClient.cs`

```
1. keyRotator.GetAvailableKeyAsync()     → null nếu không có key khả dụng
2. rateLimiter.TryConsumeAsync()         → false nếu >10 req/phút (Redis)
3. usageTracker.CanCallAsync()           → false nếu ≥100 req hôm nay (DB)
4. HTTP request với header x-apisports-key
5. HandleRateLimitAsync(response, key)   → parse 429/403, block key trong Redis
6. usageTracker.IncrementAsync()         → ghi +1 vào DB
```

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
