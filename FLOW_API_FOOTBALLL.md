# API-FOOTBALL v3.9.3 — Workflow & Implementation Guide

## Overview

**API Football** (api-sports.io) cung cấp dữ liệu bóng đá toàn cầu theo hình thức **phân cấp (hierarchy)** từ Seasons → Leagues → Fixtures. Tất cả dữ liệu được cập nhật theo thời gian thực (every 15 seconds cho fixtures).

**URL Base**: `https://v3.football.api-sports.io/`

---

## 1. Authentication & Rate Limiting

### Header Required
```
x-apisports-key: YOUR_API_KEY
```
- API chỉ accept **GET requests**
- Không được thêm extra headers (nếu có sẽ bị reject)

### Rate Limit Headers (Response)
```
x-ratelimit-requests-limit       // Request/day (theo subscription)
x-ratelimit-requests-remaining   // Request còn lại/day
X-RateLimit-Limit                // Request/minute (max)
X-RateLimit-Remaining            // Request còn lại/minute
```

**Chính sách**:
- Vượt quá limit per minute → IP bị block tạm thời/vĩnh viễn
- Khuyến nghị: **Cache dữ liệu**, không call liên tục

### Lưu ý
- **Logos/Images**: Không tính vào quota, free nhưng có rate limit/second
- **Status endpoint** (`/status`): Không tính quota

---

## 2. Data Hierarchy (Từ Diagram)

```
┌─────────────────────────────────────────────────────┐
│                    TOP LEVEL                        │
├──────────────────────┬──────────────────────────────┤
│   Seasons (năm)      │   Countries (quốc gia)      │
│   (2024, 2025...)    │   (England, France...)      │
└──────────────────────┴──────────────────────────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │   LEAGUES (Giải đấu) │
            │  (PL, La Liga...)   │
            └──────────────────────┘
                       │
         ┌─────────────┼─────────────┬──────────┬──────────┬─────────────┐
         ▼             ▼             ▼          ▼          ▼             ▼
    ┌────────┐   ┌──────────┐  ┌─────────┐ ┌──────┐  ┌──────────┐  ┌────────┐
    │Fixtures│   │Standings │  │ Teams   │ │Venues│  │Top Scorer│  │Odds    │
    └────────┘   └──────────┘  └─────────┘ └──────┘  └──────────┘  └────────┘
        │
    ┌───┴────┬──────────┬──────────┬──────────────┐
    ▼        ▼          ▼          ▼              ▼
  Events  Lineups  Statistics  H2H         Live Odds
```

### Chi tiết cấu trúc

| Layer | Resource | Mô tả | Update Frequency |
|-------|----------|-------|------------------|
| **Top** | `seasons` | Danh sách năm có dữ liệu | Mỗi ngày |
| | `countries` | Danh sách quốc gia (dùng filter) | Khi có league mới |
| **Base** | `leagues` | Danh sách giải đấu (kèm coverage info) | Vài lần/ngày |
| **Main** | `fixtures` | Danh sách/chi tiết trận đấu | **15 seconds** |
| | `standings` | Bảng xếp hạng giải | **Mỗi giờ** |
| | `teams` | Danh sách đội (kèm stats, coach, players) | Vài lần/tuần |
| | `venues` | Sân vận động | Vài lần/tuần |
| **Fixture Detail** | `fixtures/events` | Bàn thắng, thẻ, substitutions | **15 seconds** |
| | `fixtures/lineups` | Đội hình, formation | **15 phút** |
| | `fixtures/statistics` | Thống kê: shots, possession,... | **Mỗi phút** |
| | `fixtures/players` | Player performance in fixture | **Mỗi phút** |
| | `fixtures/headtohead` | Lịch sử đối đầu 2 đội | On demand |
| **Player Data** | `players/profiles` | Hồ sơ cầu thủ | Vài lần/tuần |
| | `players/squads` | Danh sách cầu thủ theo đội | Vài lần/tuần |
| | `players/topscorers` | Top 20 ghi bàn | Vài lần/tuần |
| **Live** | `predictions` | AI predictions | Mỗi giờ |
| | `injuries` | Danh sách cầu thủ chấn thương | 4 giờ/lần |
| | `odds` & `odds/live` | Tỷ lệ cá cược pre-match & in-play | 3 giờ / 5 giây |

---

## 3. Request Strategy (Tối ưu Quota)

### ❌ KHÔNG NÊN
```http
# ❌ Gọi liên tục mà không cache
GET /fixtures?league=39&season=2024      // Mỗi giây 1 lần
```

### ✅ NÊN
```http
# 1️⃣ Lấy 1 lần
GET /leagues?season=2024                 // (1 lần/ngày)
GET /teams?league=39&season=2024         // (1 lần/ngày)

# 2️⃣ Cache 15 phút cho live matches
GET /fixtures?league=39&date=2024-04-18  // Lưu Redis 15m

# 3️⃣ Khi có fixture live, gọi event real-time
GET /fixtures/events?fixture=12345       // Mỗi phút khi live
```

### Call Frequency (Khuyến nghị API)
| Endpoint | Live Match | Normal | Mỗi Tuần |
|----------|-----------|--------|----------|
| `fixtures` | 1 call/phút | 1 call/ngày | - |
| `fixtures/events` | 1 call/phút | 1 call/ngày | - |
| `fixtures/statistics` | 1 call/phút | 1 call/ngày | - |
| `fixtures/players` | 1 call/phút | 1 call/ngày | - |
| `standings` | 1 call/giờ | 1 call/ngày | - |
| `teams/statistics` | - | 1 call/ngày | 1 call/tuần |
| `players` | - | 1 call/ngày | - |
| `leagues` | - | 1 call/giờ | - |
| `injuries` | - | 1 call/ngày | - |

---

## 4. Fixture Workflow (Chi tiết)

### Status Flow

```
TBD / NS (Not Started)
    ↓
1H (First Half)  ─┐
HT (Halftime)    │─ IN PLAY
2H (Second Half) │
ET (Extra Time)  ─┘
    ↓
FT / AET / PEN (Match Finished)

PST (Postponed) → reschedule
CANC (Cancelled) → no reschedule
ABD (Abandoned) → possibly reschedule
```

### Dữ liệu Available By Status

```javascript
{
  "fixture": {
    "id": 215662,
    "date": "2023-08-12T15:00:00+00:00",
    "timezone": "UTC",
    "timestamp": 1691844000,
    "periods": {              // 📌 Available khi FT
      "first": 1691844000,
      "second": 1691847600
    },
    "venue": {
      "id": 356,
      "name": "Old Trafford",
      "city": "Manchester"
    },
    "status": {
      "long": "Match Finished",
      "short": "FT",
      "elapsed": 90
    },
    "league": 39,
    "season": 2023,
    "round": "Regular Season - 1"
  },
  "goals": {
    "home": 2,
    "away": 1
  },
  "score": {
    "halftime": { "home": 1, "away": 0 },
    "fulltime": { "home": 2, "away": 1 },
    "extratime": null,
    "penalty": null
  }
}
```

### Khi Call `/fixtures?id=123` (With Events, Lineups, Stats)

```javascript
// Response bao gồm:
{
  "fixture": {...},
  "league": {...},
  "teams": { "home": {...}, "away": {...} },
  "players": [              // ✅ players stats
    {
      "team": "home",
      "players": [
        {
          "player": {...},
          "statistics": [
            {
              "games": { "minutes": 90, "number": 1, "position": "G", "rating": "7.5" },
              "offsides": 0,
              "shots": { "total": 0, "on": 0 },
              "goals": { "total": 0, "conceded": 1, "assists": 0 },
              "passes": { "total": 25, "key": 0, "accuracy": "84%" },
              "tackles": { "total": 2, "blocks": 1, "interceptions": 3 },
              "duels": { "total": 5, "won": 2 },
              "dribbles": { "attempts": 0, "success": 0 },
              "fouls": { "drawn": 0, "committed": 0 },
              "cards": { "yellow": 0, "red": 0 },
              "penalty": { "won": 0, "commited": 0, "scored": 0, "missed": 0, "saved": 0 }
            }
          ]
        }
      ]
    }
  ],
  "events": [...],          // ✅ events
  "lineups": [...]          // ✅ lineups
}
```

---

## 5. Coverage Field (Quan trọng!)

### Kiểm tra Feature Availability

```javascript
// GET /leagues?id=39
{
  "id": 39,
  "name": "Premier League",
  "type": "League",
  "logo": "https://media.api-sports.io/football/leagues/39.png",
  "country": {...},
  "season": 2023,
  "coverage": {
    "fixtures": {
      "events": true,              // ✅ Có events
      "lineups": true,             // ✅ Có lineups
      "statistics_fixtures": true, // ✅ Có fixture stats
      "statistics_players": true   // ✅ Có player stats
    },
    "standings": true,             // ✅ Có standings
    "players": true,               // ✅ Có player data
    "top_scorers": true,           // ✅ Có top scorers
    "top_assists": true,
    "top_cards": true,
    "injuries": true,
    "predictions": true,
    "odds": false                  // ❌ Không có odds
  }
}
```

**Lưu ý**: `coverage: false` không đảm bảo 100% không có dữ liệu (friendlies có exception).

---

## 6. Endpoints Chi tiết (Cho Project)

### 6.1 Fixtures (Trận đấu)

```bash
# ✅ Lấy 1 trận (kèm events, lineups, stats, players)
GET /fixtures?id=215662

# ✅ Lấy nhiều trận 1 lần (max 20)
GET /fixtures?ids=215662-215663-215664-215665

# ✅ Lấy trận live
GET /fixtures?live=all
GET /fixtures?live=39-61-48  # Specific leagues only

# ✅ Lấy theo ngày
GET /fixtures?date=2024-04-18

# ✅ Lấy Next X upcoming
GET /fixtures?next=15

# ✅ Filter by league + season
GET /fixtures?league=39&season=2024

# ✅ Filter by team
GET /fixtures?team=33&season=2024&from=2024-01-01&to=2024-12-31

# ✅ Filter by round
GET /fixtures?league=39&season=2024&round=Regular%20Season%20-%201

# ✅ Filter by status
GET /fixtures?league=39&status=ft     # Only finished
GET /fixtures?league=39&status=ns-ft  # Not started or finished
```

**Status Codes**: `NS`, `1H`, `HT`, `2H`, `ET`, `BT`, `P`, `SUSP`, `INT`, `FT`, `AET`, `PEN`, `PST`, `CANC`, `ABD`, `AWD`, `WO`

### 6.2 Standings (Bảng xếp hạng)

```bash
# ✅ Lấy bảng xếp hạng
GET /standings?league=39&season=2024

# ✅ Lấy xếp hạng của 1 đội
GET /standings?league=39&season=2024&team=33
```

### 6.3 Rounds (Vòng đấu)

```bash
# ✅ Lấy danh sách vòng
GET /fixtures/rounds?league=39&season=2024

# ✅ Kèm dates của mỗi vòng
GET /fixtures/rounds?league=39&season=2024&dates=true

# ✅ Lấy vòng hiện tại
GET /fixtures/rounds?league=39&season=2024&current=true
```

### 6.4 H2H (Đối đầu)

```bash
# ✅ Lịch sử đối đầu
GET /fixtures/headtohead?h2h=33-34

# ✅ Kèm filter
GET /fixtures/headtohead?h2h=33-34&last=10&status=ft
GET /fixtures/headtohead?h2h=33-34&league=39&season=2024
```

### 6.5 Teams

```bash
# ✅ Lấy 1 đội
GET /teams?id=33

# ✅ Lấy đội theo league + season
GET /teams?league=39&season=2024

# ✅ Team statistics
GET /teams/statistics?league=39&season=2024&team=33
```

### 6.6 Players

```bash
# ✅ Lấy profile 1 cầu thủ
GET /players/profiles?player=276

# ✅ Tìm theo tên
GET /players/profiles?search=ney

# ✅ Stats cầu thủ (with pagination)
GET /players?id=276&season=2024
GET /players?league=39&season=2024&page=1

# ✅ Squad đội
GET /players/squads?team=33

# ✅ Top scorers
GET /players/topscorers?league=39&season=2024

# ✅ Top assists
GET /players/topassists?league=39&season=2024
```

### 6.7 Predictions (AI Dự đoán)

```bash
# ✅ Dự đoán 1 trận
GET /predictions?fixture=215662
```

**Output**:
- `match.winner` (Home/Draw/Away ID)
- `match.win_or_draw` (True/False)
- `goals.for` (Goal predictions)
- `goals.against`
- `advice` (Recommend prediction)
- `comparison.form`, `goals`, `possession`,... (So sánh đội)

### 6.8 Injuries (Chấn thương)

```bash
# ✅ Injuries theo fixture
GET /injuries?fixture=686314

# ✅ Injuries theo league + season
GET /injuries?league=2&season=2024

# ✅ Injuries theo team
GET /injuries?team=85&season=2024

# ✅ Multiple fixtures at once
GET /injuries?ids=686314-686315-686316
```

### 6.9 Odds (Tỷ lệ cá cược)

```bash
# ✅ Pre-match odds (1-14 days before)
GET /odds?fixture=164327
GET /odds?league=39&season=2024&page=1

# ✅ In-play odds (live)
GET /odds/live?fixture=721238
GET /odds/live?league=39

# ✅ Odds mapping (fixtures available)
GET /odds/mapping?page=1

# ✅ Available bookmakers
GET /odds/bookmakers

# ✅ Available bets
GET /odds/bets
GET /odds/live/bets
```

---

## 7. Best Practices (Dự án)

### 🔄 Caching Strategy

```csharp
// Project sử dụng Redis + Hangfire
// Khuyến nghị:

1. Seasons, Countries, Leagues
   - Cache: 24 giờ
   - Update: Background job mỗi ngày lúc 2AM

2. Teams, Standings, Rounds
   - Cache: 1 giờ
   - Update: Hangfire job 1 lần/giờ khi league active

3. Fixtures (Scheduled)
   - Cache: 15 phút
   - Update: Background job 5 phút/lần

4. Fixtures (Live)
   - Cache: 30 giây
   - Update: SignalR Hub real-time
   - Gọi API: mỗi 1 phút

5. Predictions, Injuries
   - Cache: 4 giờ
   - Update: Hangfire job 4 lần/ngày

6. Odds (Pre-match)
   - Cache: 3 giờ
   - Odds (In-play)
   - Cache: 5 phút (update frequent)
```

### 📊 Error Handling

```csharp
// Rate limit exceeded (429 response)
→ Implement exponential backoff
→ Deactivate live polling
→ Send alert to admin

// Timeout (499)
→ Retry 3 lần
→ Use cached data fallback

// Data missing
→ Check league coverage before call
→ Log chi tiết (fixture ID, league, season)
```

### 🔑 API Key Management

```csharp
// Hiện tại:
// - Store in AWS Parameter Store
// - Never expose in frontend
// - Rotate key regularly

// Protect against abuse:
// - Whitelist IP chỉ API server
// - Monitor dashboard.api-football.com
// - Set up alerts khi quota gần hết
```

### 📈 Monitoring

```csharp
// Log these metrics:
- API calls/day (vs. quota)
- Response time per endpoint
- Cache hit rate (%)
- Failed requests (retry count)
- Rate limit responses

// Dashboard:
// .claude/rules/logging.md → Log to `/logs/api/`
```

---

## 8. Timezone & Formatting

```bash
# ✅ Specify timezone (default: UTC)
GET /fixtures?league=39&date=2024-04-18&timezone=Europe/London
GET /fixtures?team=85&timezone=Asia/Bangkok

# ⏰ Available timezones
GET /timezone
# Returns 425+ timezones (IANA format)
```

**Date format**: `YYYY-MM-DD` (ISO 8601)
**Timestamp**: Unix timestamp (seconds)

---

## 9. Pagination

```bash
# ✅ Page-based pagination
GET /players?league=39&season=2024&page=1  # 20 per page
GET /players?league=39&season=2024&page=2

GET /players/profiles?search=ney&page=1    # 250 per page

GET /odds?league=39&season=2024&page=1    # 10 per page
```

---

## 10. Media URLs (Không tính quota)

```
Team logo:   https://media.api-sports.io/football/teams/{id}.png
Player foto: https://media.api-sports.io/football/players/{id}.png
League logo: https://media.api-sports.io/football/leagues/{id}.png
Coach foto:  https://media.api-sports.io/football/coachs/{id}.png
Venue image: https://media.api-sports.io/football/venues/{id}.png
Country flag: https://media.api-sports.io/flags/{code}.svg
```

**⚠️ LƯỚI**: Cache images to CDN (recommended: BunnyCDN) để:
- Không tính quota
- Load nhanh
- Giảm tải API

---

## 11. Troubleshooting

| Issue | Nguyên nhân | Solution |
|-------|-----------|----------|
| 204 No Content | Fixture chưa có data | Check `coverage` field |
| 429 Too Many Requests | Vượt rate limit | Implement backoff, reduce polling |
| Data missing (stats, lineups) | League không support | Check league coverage |
| Lineups null 2h trước trận | Chưa công bố đội hình | Lineups 20-40m trước kickoff |
| Pagination slow | Too large page number | Giảm `page`, cache results |

---

## 12. Example: Football Blog Project

### Workflow gợi ý

```
Daily (2AM):
├─ Fetch all active leagues (cache 24h)
├─ Fetch standings (cache 1h)
└─ Fetch teams (cache 24h)

Hourly:
├─ Fetch upcoming fixtures (next 7 days)
├─ Fetch injuries
└─ Update standings

Every 5 minutes (when live):
├─ Fetch fixture events
├─ Fetch fixture statistics
├─ Broadcast via SignalR
└─ Publish to live score widget

Before kickoff (24h):
├─ Generate AI predictions
├─ Publish blog post
└─ Send Telegram notification

After match (Real-time):
├─ Fetch final score, events, lineups
├─ Update prediction accuracy
└─ Edit Telegram message with result
```

### Cách implement trong project

```csharp
// 1. FootballApiClient (typed HttpClient)
public class FootballApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FootballApiClient> _logger;

    public async Task<FixtureDto> GetFixtureAsync(int id)
    {
        const string cacheKey = $"fixture_{id}";

        if (_cache.TryGetValue(cacheKey, out FixtureDto cached))
            return cached;

        // Call API
        var response = await _httpClient.GetAsync($"/fixtures?id={id}");
        var data = await response.Content.ReadAsAsync<ApiResponse<FixtureDto>>();

        // Log headers
        LogRateLimit(response.Headers);

        // Cache 30 seconds if live, 15 min if finished
        var ttl = data.Response.Fixture.Status == "FT" ?
            TimeSpan.FromMinutes(15) :
            TimeSpan.FromSeconds(30);

        _cache.Set(cacheKey, data.Response, ttl);
        return data.Response;
    }
}

// 2. Hangfire Jobs
public class FetchFixturesJob
{
    private readonly FootballApiClient _client;
    private readonly IRepository<Match> _matchRepo;
    private readonly ILogger<FetchFixturesJob> _logger;

    [JobDisplayName("Fetch upcoming fixtures")]
    public async Task ExecuteAsync()
    {
        var fixtures = await _client.GetFixturesAsync(
            league: 39,
            season: 2024,
            next: 15,
            timezone: "UTC"
        );

        foreach (var fixture in fixtures)
        {
            await _matchRepo.UpsertAsync(new Match
            {
                ExternalId = fixture.Id,
                Status = fixture.Status,
                // ... map fields
            });
        }

        _logger.LogInformation("Fetched {Count} fixtures", fixtures.Count);
    }
}

// 3. Real-time Hub
public class LiveScoreHub : Hub
{
    private readonly FootballApiClient _client;

    public async Task Subscribe(int fixtureId)
    {
        await Groups.AddToGroupAsync(Connection.ConnectionId, $"fixture_{fixtureId}");
    }

    // Called by polling job
    public async Task BroadcastFixtureUpdate(int fixtureId)
    {
        var fixture = await _client.GetFixtureAsync(fixtureId);
        await Clients.Group($"fixture_{fixtureId}")
            .SendAsync("FixtureUpdated", fixture);
    }
}
```

---

## Summary

| Khía cạnh | Chi tiết |
|---------|---------|
| **Rate Limit** | Phụ thuộc subscription (free: 100 req/day, 30 req/min) |
| **Cache** | Redis: fixtures (15m live), standings (1h), leagues (24h) |
| **Real-time** | SignalR polling /fixtures/events mỗi 1 phút khi live |
| **Predictions** | Hangfire trigger 24h trước kickoff, publish blog post |
| **Logging** | `/logs/api/` - monitor quota, response times, errors |
| **Media** | CDN (BunnyCDN) - không tính quota |
| **Error** | Exponential backoff, fallback to cache |

👉 **Xem code hiện tại**: `FootballBlog.API/ApiClients/FootballApi/`
