# Plan: Phase 4 — Football API Integration + Hangfire Jobs

## Context
Dự án cần data thực từ api-football.com để render UI và chuẩn bị cho Phase 5 (AI Prediction).
Chiến lược: fetch + lưu DB, chỉ gọi API 2 lần pre-match (5h + 15min trước kickoff) để tối ưu quota 100 req/ngày.
Live polling batch toàn bộ trận trong 1 request duy nhất.

## Packages Cần Cài

**FootballBlog.API.csproj** — thêm 5 packages:
```
Hangfire.AspNetCore                  1.8.x
Hangfire.PostgreSql                  1.20.x
Microsoft.Extensions.Http.Polly      8.0.x
StackExchange.Redis                  2.7.x
AspNetCore.HealthChecks.Redis        9.0.x
```

**FootballBlog.Infrastructure.csproj** — thêm 1 package:
```
StackExchange.Redis                  2.7.x
```

## Files Mới Cần Tạo (9 files)

```
FootballBlog.Core/Options/FootballApiOptions.cs
FootballBlog.Core/Interfaces/IFootballApiClient.cs
FootballBlog.Core/Interfaces/IFootballApiRateLimiter.cs
FootballBlog.Infrastructure/Services/RedisFootballApiRateLimiter.cs
FootballBlog.API/ApiClients/FootballApi/FootballApiResponses.cs
FootballBlog.API/ApiClients/FootballApi/FootballApiClient.cs
FootballBlog.API/Jobs/FetchUpcomingMatchesJob.cs
FootballBlog.API/Jobs/PreMatchDataJob.cs
FootballBlog.API/Jobs/LiveScorePollingJob.cs
```

## Files Cần Sửa (5 files)

```
FootballBlog.API/FootballBlog.API.csproj                        — thêm packages
FootballBlog.Infrastructure/FootballBlog.Infrastructure.csproj  — thêm Redis
FootballBlog.API/appsettings.json                               — thêm FootballApi + Redis
FootballBlog.API/appsettings.Development.json                   — mirror
FootballBlog.API/Program.cs                                     — DI + Hangfire + log filter fix
```

---

## Chi Tiết Từng File Mới

### 1. `FootballApiOptions.cs`
Options class ánh xạ section `"FootballApi"` trong appsettings.
```csharp
public class FootballApiOptions
{
    public const string SectionName = "FootballApi";
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int DailyRequestLimit { get; set; } = 100;
    public int FixturesPerLeague { get; set; } = 20;
    public int[] LeagueIds { get; set; } = [];
}
```

### 2. `IFootballApiClient.cs` — trong Core/Interfaces
```csharp
public interface IFootballApiClient
{
    Task<IEnumerable<Match>?> GetUpcomingFixturesAsync(int leagueId, int next = 20);
    Task<IEnumerable<LiveMatch>?> GetAllLiveFixturesAsync();           // GET /fixtures?live=all
    Task<IEnumerable<Match>?> GetHeadToHeadAsync(int homeTeamId, int awayTeamId, int last = 10);
    Task<string?> GetLineupsRawAsync(int fixtureId);                   // raw JSON cho Phase 5
}
```

### 3. `IFootballApiRateLimiter.cs` — trong Core/Interfaces
```csharp
public interface IFootballApiRateLimiter
{
    Task<bool> TryConsumeAsync();      // INCR Redis key, return false nếu vượt limit
    Task<int> GetTodayUsageAsync();    // GET Redis key
}
```

### 4. `RedisFootballApiRateLimiter.cs` — trong Infrastructure/Services
- Constructor: `(IConnectionMultiplexer redis, IOptions<FootballApiOptions> options, ILogger<...> logger)`
- Redis key: `football_api:requests:{DateTime.UtcNow:yyyy-MM-dd}`
- Algorithm:
  1. `INCR` key → lấy `newCount`
  2. Nếu `newCount == 1` → set `EXPIREAT` đến midnight UTC ngày hôm sau
  3. Nếu `newCount > DailyRequestLimit` → `DECR` (hoàn lại), log Warning, return `false`
  4. Return `true`
- Fail open: nếu Redis down → catch, log Warning, return `true` (không block job)

### 5. `FootballApiResponses.cs` — trong API/ApiClients/FootballApi
Internal records chỉ dùng trong `FootballApiClient`, không lộ ra ngoài:
```csharp
internal record FootballApiEnvelope<T>(
    [property: JsonPropertyName("response")] T[] Response);

internal record FixtureResponse(
    [property: JsonPropertyName("fixture")] FixtureInfo Fixture,
    [property: JsonPropertyName("league")]  LeagueInfo  League,
    [property: JsonPropertyName("teams")]   TeamsInfo   Teams,
    [property: JsonPropertyName("goals")]   GoalsInfo   Goals);

// + FixtureInfo, FixtureStatus, LeagueInfo, TeamsInfo, TeamInfo, GoalsInfo records
```

### 6. `FootballApiClient.cs` — trong API/ApiClients/FootballApi
Pattern: giống `PostApiClient.cs` (primary constructor, return null on error).
- Constructor: `(HttpClient httpClient, IOptions<FootballApiOptions> options, IFootballApiRateLimiter rateLimiter, ILogger<FootballApiClient> logger)`
- **Mọi method**: gọi `await rateLimiter.TryConsumeAsync()` TRƯỚC khi HTTP call. Nếu `false` → return `null`.
- Status mapping (private static):
```csharp
private static MatchStatus MapStatus(string s) => s switch
{
    "NS"                                     => MatchStatus.Scheduled,
    "1H" or "2H" or "HT" or "ET" or "P"
        or "LIVE" or "BT"                    => MatchStatus.Live,
    "FT" or "AET" or "PEN"                  => MatchStatus.Finished,
    "PST"                                    => MatchStatus.Postponed,
    "SUSP" or "CANC" or "ABD" or "WO"      => MatchStatus.Cancelled,
    _                                        => MatchStatus.Scheduled
};
```
- Mapper `MapToMatch(FixtureResponse r)` → domain `Match`
- Mapper `MapToLiveMatch(FixtureResponse r)` → domain `LiveMatch`

### 7. `FetchUpcomingMatchesJob.cs` — trong API/Jobs
- Constructor: `(IFootballApiClient apiClient, IUnitOfWork uow, IOptions<FootballApiOptions> options, ILogger<FetchUpcomingMatchesJob> logger)`
- Method: `public async Task ExecuteAsync()`
- Algorithm:
  ```
  foreach leagueId in options.LeagueIds:
      fixtures = await apiClient.GetUpcomingFixturesAsync(leagueId)
      if null: continue  // skip, thử league tiếp theo

      foreach fixture:
          existing = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId)
          if null:
              await uow.Matches.AddAsync(fixture)
              // Schedule pre-match jobs — chỉ nếu thời gian còn trong tương lai
              if kickoff - 5h > now:
                  BackgroundJob.Schedule<PreMatchDataJob>(
                      j => j.FetchH2HAsync(externalId, homeTeamId, awayTeamId),
                      kickoff.AddHours(-5))
              if kickoff - 15min > now:
                  BackgroundJob.Schedule<PreMatchDataJob>(
                      j => j.FetchLineupsAsync(externalId),
                      kickoff.AddMinutes(-15))
          else:
              // Idempotent update — chỉ cập nhật status + score
              existing.Status = fixture.Status; existing.HomeScore = ...; existing.FetchedAt = now
              await uow.Matches.UpdateAsync(existing)

  await uow.CommitAsync()  // 1 commit duy nhất
  LogInformation("Done. New={new}, Updated={upd}")
  ```

### 8. `PreMatchDataJob.cs` — trong API/Jobs
- Constructor: `(IFootballApiClient apiClient, IUnitOfWork uow, ILogger<PreMatchDataJob> logger)`
- Method 1: `public async Task FetchH2HAsync(int fixtureExternalId, int homeTeamId, int awayTeamId)`
  - Kiểm tra match tồn tại → gọi `apiClient.GetHeadToHeadAsync()`
  - Log kết quả (Phase 5 sẽ persist vào DB sau)
- Method 2: `public async Task FetchLineupsAsync(int fixtureExternalId)`
  - Kiểm tra match tồn tại → gọi `apiClient.GetLineupsRawAsync()`
  - Log kết quả (Phase 5 lưu vào Match.LineupsJson)
- Cả 2 methods: `if match == null → LogWarning → return` (idempotent)

### 9. `LiveScorePollingJob.cs` — trong API/Jobs
- Constructor: `(IFootballApiClient apiClient, IUnitOfWork uow, ILogger<LiveScorePollingJob> logger)`
- Method: `public async Task ExecuteAsync()`
- Algorithm:
  ```
  // Adaptive gate — 0 API cost nếu không có live match
  liveInDb = await uow.LiveMatches.GetLiveMatchesAsync()
  if liveInDb.Count == 0:
      LogDebug("No live matches. Skipping."); return

  // 1 request duy nhất — lấy TẤT CẢ live matches
  liveFromApi = await apiClient.GetAllLiveFixturesAsync()
  if null: LogWarning(...); return

  foreach fixture in liveFromApi:
      existing = await uow.LiveMatches.GetByExternalIdAsync(fixture.ExternalId)
      if null:
          parentMatch = await uow.Matches.GetByExternalIdAsync(fixture.ExternalId)
          fixture.MatchId = parentMatch?.Id
          await uow.LiveMatches.AddAsync(fixture)
      else:
          existing.HomeScore = ...; existing.AwayScore = ...; existing.Minute = ...
          await uow.LiveMatches.UpdateAsync(existing)

  // Đánh dấu finished — trận không còn trong API response nhưng vẫn Live trong DB
  foreach dbLive in liveInDb where NOT in liveFromApi:
      dbLive.Status = MatchStatus.Finished
      await uow.LiveMatches.UpdateAsync(dbLive)
      parentMatch.Status = MatchStatus.Finished
      await uow.Matches.UpdateAsync(parentMatch)

  await uow.CommitAsync()
  LogInformation("Live poll done. Inserted={ins}, Updated={upd}")
  ```

---

## appsettings.json — Thêm Vào

```json
{
  "FootballApi": {
    "BaseUrl": "https://v3.football.api-sports.io",
    "ApiKey": "",
    "DailyRequestLimit": 100,
    "FixturesPerLeague": 20,
    "LeagueIds": [39, 140, 135, 78, 61, 94, 2, 3, 848, 531, 1, 45, 48, 253, 307]
  },
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "Redis": "localhost:6379"
  }
}
```
`ApiKey` để trống trong file — set qua `dotnet user-secrets set "FootballApi:ApiKey" "YOUR_KEY"`.

**LeagueIds mặc định (15 giải chính):**
39=Premier League, 140=La Liga, 135=Serie A, 78=Bundesliga, 61=Ligue 1,
94=Primeira Liga, 2=UEFA Champions League, 3=UEFA Europa League,
848=UEFA Conference League, 531=UEFA Super Cup, 1=World Cup, 45=FA Cup,
48=League Cup, 253=MLS, 307=Saudi Pro League

---

## Program.cs — Thứ Tự DI (quan trọng)

```csharp
// 1. Options — bind trước, các service khác phụ thuộc
builder.Services.Configure<FootballApiOptions>(
    builder.Configuration.GetSection(FootballApiOptions.SectionName));

// 2. Redis connection — singleton, thread-safe
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

// 3. Rate limiter — singleton (dùng Redis singleton)
builder.Services.AddSingleton<IFootballApiRateLimiter, RedisFootballApiRateLimiter>();

// 4. Football API typed HttpClient + Polly retry
builder.Services.AddHttpClient<IFootballApiClient, FootballApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FootballApi:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("x-apisports-key", builder.Configuration["FootballApi:ApiKey"]);
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i))));

// 5. Hangfire — PostgreSQL storage
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
        new PostgreSqlStorageOptions { SchemaName = "hangfire" }));

builder.Services.AddHangfireServer(opt => { opt.WorkerCount = 2; });

// 6. Health check Redis
builder.Services.AddHealthChecks()
    .AddNpgSql(...)          // đã có
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);
```

**Sau `var app = builder.Build()`:**
```csharp
if (app.Environment.IsDevelopment())
    app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<FetchUpcomingMatchesJob>(
    "fetch-upcoming-matches", j => j.ExecuteAsync(), "0 */6 * * *", TimeZoneInfo.Utc);

RecurringJob.AddOrUpdate<LiveScorePollingJob>(
    "live-score-polling", j => j.ExecuteAsync(), Cron.Minutely(), TimeZoneInfo.Utc);
```

**Fix Serilog jobs filter** (hiện chỉ bắt `"Hangfire"`, cần thêm jobs của project):
```csharp
// Trước:
sc.ToString().Contains("Hangfire")

// Sau:
sc.ToString().Contains("Hangfire") || sc.ToString().Contains(".Jobs.")
```

---

## Không Cần Migration Mới
Tất cả entity (`Match`, `LiveMatch`, `MatchEvent`, `MatchPrediction`) đã có trong DB.
Hangfire tự tạo schema `hangfire.*` qua `UsePostgreSqlStorage`.

---

## Verification

1. `dotnet build FootballBlog.sln` — 0 errors
2. `docker compose up` → `redis-cli ping` → PONG
3. `https://localhost:7007/hangfire` — dashboard load, thấy 2 recurring jobs
4. Trigger `FetchUpcomingMatchesJob` thủ công → kiểm tra bảng `Matches` có data
5. Kiểm tra `logs/jobs/jobs-*.log` có log từ cả job class và Hangfire internals
6. Verify rate limiter: sau nhiều lần trigger, log Warning "Daily limit reached"
7. Khi không có live match → log Debug "No live matches. Skipping." mỗi phút (0 API call)
