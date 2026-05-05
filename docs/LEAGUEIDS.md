# League IDs — api-football.com

Danh sách 30 giải đấu dự kiến fetch. ✅ = đang có trong config | ⬜ = chưa thêm

---

## UEFA / FIFA
- Champions League: 2 ✅
- Europa League: 3 ✅
- Conference League: 848 ✅
- Nations League: 531 ✅
- World Cup: 1 ✅

## Anh
- Premier League: 39 ✅
- FA Cup: 45 ✅
- EFL Cup (Carabao): 48 ✅

## Tây Ban Nha
- La Liga: 140 ✅

## Ý
- Serie A: 135 ✅

## Đức
- Bundesliga: 78 ✅

## Pháp
- Ligue 1: 61 ✅

## Bồ Đào Nha
- Primeira Liga: 94 ✅

## Saudi Arabia
- Pro League: 307 ✅

## Mỹ
- MLS: 253 ✅

## Việt Nam
- V.League 1: 340 ⬜
- V.League 2: 637 ⬜
- Cup: 341 ⬜
- Super Cup: 831 ⬜

---

## Hà Lan
- Eredivisie: 88 ⬜

## Bỉ
- Pro League: 144 ⬜

## Scotland
- Premiership: 179 ⬜

## Thổ Nhĩ Kỳ
- Süper Lig: 203 ⬜

## Mexico
- Liga MX: 262 ⬜

## Brazil
- Série A: 71 ⬜

## Argentina
- Liga Profesional: 128 ⬜

## Nhật Bản
- J1 League: 98 ⬜

## Hàn Quốc
- K League 1: 292 ⬜

## Úc
- A-League: 188 ⬜

## Nga
- Premier League: 235 ⬜

---

## Tổng: 30 giải | Request/ngày: 30 req (trong giới hạn free 100 req/ngày)

---

# TODO — Triển khai session mới

## 1. Fix FootballApiClient.cs — query theo date + skip ngày đã có trong DB

**File:** `FootballBlog.API/ApiClients/FootballApi/FootballApiClient.cs`
**Vấn đề:** Free plan không hỗ trợ param `next` và `season` mới hơn 2024.

### Logic tối ưu request

```
Ngày 1 (DB trống):  fetch today, +1  → 60 req (30 leagues × 2 ngày)
Ngày 2:             today đã có → chỉ fetch +1 (ngày mới) → 30 req
Ngày 3:             chỉ fetch ngày mới → 30 req
Còn dư:             40 req/ngày cho mục đích khác
```

Job cần làm trước khi gọi API:
1. Query DB lấy danh sách các ngày đã có data: `SELECT DISTINCT DATE(kickoff_utc) FROM matches`
2. So sánh với window [today, today+1]
3. Chỉ gọi API cho các ngày **chưa có trong DB**

### Thay đổi cần làm

**`IFootballApiClient`** — thêm method mới:
```csharp
Task<IEnumerable<FixtureRawDto>?> GetFixturesByDateAsync(int leagueId, DateOnly date);
```

**`FootballApiClient`** — implement method mới:
```csharp
// fixtures?league={leagueId}&date=2026-05-05
```

**`FetchUpcomingMatchesJob`** — sửa logic chính:
```csharp
// 1. Lấy các ngày đã có data trong DB
var fetchedDates = await uow.Matches.GetFetchedDatesAsync(); // trả Set<DateOnly>

// 2. Tính các ngày cần fetch
var today = DateOnly.FromDateTime(DateTime.UtcNow);
var datesToFetch = Enumerable.Range(0, 2)  // today + tomorrow only
    .Select(i => today.AddDays(i))
    .Where(d => !fetchedDates.Contains(d))
    .ToList();

if (!datesToFetch.Any())
{
    logger.LogInformation("Tất cả ngày trong window đã có data, skip.");
    return;
}

// 3. Fetch từng ngày × từng league
foreach (var date in datesToFetch)
    foreach (var leagueId in opts.LeagueIds)
        await FetchAndUpsertAsync(leagueId, date);
```

**`IMatchRepository`** — thêm method:
```csharp
Task<HashSet<DateOnly>> GetFetchedDatesAsync();
```

**Lưu ý:** `GetFetchedDatesAsync` chỉ cần query đơn giản:
```sql
SELECT DISTINCT DATE(kickoff_utc) FROM matches WHERE kickoff_utc >= today
```

---

## 2. Update LeagueIds trong appsettings.json

**File:** `FootballBlog.API/appsettings.json`
**Sửa** `FootballApi.LeagueIds` thành đủ 30 giải (bỏ các ID cũ, thêm mới):

```json
"LeagueIds": [ 2, 3, 848, 531, 1, 39, 45, 48, 140, 135, 78, 61, 94, 307, 253, 340, 637, 341, 831, 88, 144, 179, 203, 262, 71, 128, 98, 292, 188, 235 ]
```

---

## 3. Set Football API key lên Fly.io

**Key:** `55559dd1d3600c15def362616f1e53ab`
```powershell
fly secrets set --app footballblog-api "FootballApi__ApiKey=55559dd1d3600c15def362616f1e53ab"
```
Sau đó cũng lưu vào `appsettings.Development.json` > `FootballApi.ApiKey` để test local.

---

## 4. Kiểm tra schedule job đã đúng chưa

**File:** `FootballBlog.API/Program.cs` dòng ~269
Schedule hiện tại đã sửa thành `"0 6 * * *"` (6h UTC = 13h Việt Nam) — chạy 1 lần/ngày.
Verify lại đúng chưa trước khi deploy.

---

## 5. Deploy lên server + test flow hoàn chỉnh

```powershell
git add .
git commit -m "fix: fetch fixtures by date for free plan, add 30 leagues"
git push origin master
```

Sau khi GitHub Actions deploy xong (~5 phút), verify trên Fly.io:
```powershell
fly logs --app footballblog-api   # xem job chạy lúc 6h UTC
```

Trigger job thủ công để test ngay (không cần đợi 6h sáng):
```
Vào https://footballblog-api.fly.dev/hangfire
→ Recurring Jobs → fetch-upcoming-matches → Trigger now
```

Nếu job chạy thành công → kiểm tra Neon DB có data trận đấu không.

---

## 6. Flow Telegram — Hướng B (bỏ bước đăng blog)

**Mục tiêu:** Fetch trận → Gemini sinh nhận định → gửi thẳng Telegram, không đăng blog.

### Flow mới
```
FetchUpcomingMatchesJob (6h UTC)
        ↓
  Lấy trận ngày mai từ DB
        ↓
  GeneratePredictionJob — gọi Gemini Free (2.0 Flash)
        ↓
  TelegramNotificationJob — gửi thẳng Telegram
        ↗ (bỏ PublishPredictionJob)
```

### Thay đổi cần làm trong code

**`GeneratePredictionJob`** — sau khi sinh prediction xong, trigger Telegram trực tiếp:
```csharp
// Thay vì:
BackgroundJob.Enqueue<PublishPredictionJob>(j => j.ExecuteAsync(predictionId));

// Đổi thành:
BackgroundJob.Enqueue<TelegramNotificationJob>(j => j.SendPredictionAsync(predictionId));
```

**`appsettings.json`** — cập nhật Jobs:
```json
"Jobs": {
  "FetchUpcomingMatches": true,
  "LiveScorePolling": false,
  "GeneratePrediction": true,
  "PublishPrediction": false,
  "TelegramNotification": true
}
```

**Gemini API key** — lấy tại https://aistudio.google.com/apikey (free, không cần thẻ)
```powershell
fly secrets set --app footballblog-api "AI__Gemini__ApiKey=<key>"
```
Đảm bảo `AI.DefaultProvider = "Gemini"` trong appsettings để ưu tiên dùng Gemini thay Claude.

### Req/ngày ước tính
| Nguồn | Req/ngày | Limit free |
|-------|---------|-----------|
| Football API | ~30 req | 100 req ✅ |
| Gemini 2.0 Flash | ~30 req (1 req/trận) | 1,500 req ✅ |
| Telegram Bot | ~30 req | Không giới hạn ✅ |

### Jobs trạng thái sau khi áp dụng
| Job | Schedule | Trạng thái | Ghi chú |
|-----|----------|-----------|---------|
| FetchUpcomingMatches | 6h UTC hàng ngày | ✅ bật | Cần fix date query (TODO #1) |
| LiveScorePolling | mỗi phút | ⏸ tắt | Chờ API key live data |
| GeneratePrediction | theo trigger sau fetch | ✅ bật | Cần Gemini API key |
| PublishPrediction | — | ⏸ tắt | Bỏ qua, không dùng |
| TelegramNotification | theo trigger sau Gemini | ✅ bật | Cần Telegram BotToken + ChannelId |
