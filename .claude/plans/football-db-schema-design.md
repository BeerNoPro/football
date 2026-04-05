# Football Database Schema Design — Reference Document

## Context

Phase 1 đã tạo `Match` entity với team/league lưu dưới dạng **string** (không normalized).
Trước khi Phase 4 (Football API integration) bắt đầu, cần refactor schema để:
- Query "form 5 trận gần nhất của team X" được qua FK
- Thêm giải đấu mới chỉ cần INSERT 1 row `League`
- Cung cấp đủ context cho AI prediction (H2H, form, lineup, fatigue)

**Table `Matches` hiện tại: rỗng** → migration sạch, không cần data migration script.
**Thời điểm implement**: Đầu Phase 4.

---

## Phân Cấp Dữ Liệu (tham khảo FlashScore)

```
Country  →  League  →  [Season: string]  →  Match
                                              ├── HomeTeam (FK → Team)
                                              ├── AwayTeam (FK → Team)
                                              └── MatchContextData (JSONB)
                                                   ├── H2H
                                                   ├── HomeForm / AwayForm
                                                   ├── Lineup
                                                   ├── Referee
                                                   └── Fatigue
```

> Season và Round **không phải entity** — lưu string trong `Match` ("2024/2025", "Round 10").
> Thêm mùa giải mới = tự động có khi fetch data từ API.

---

## Schema Mới — Entities Cần Tạo/Sửa

### 1. `Country` (mới — reference table đơn giản)

```csharp
// FootballBlog.Core/Models/Country.cs
public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // "England", "Vietnam"
    public string Code { get; set; } = string.Empty;   // "GB-ENG", "VN" (ISO)
    public string? FlagUrl { get; set; }

    public ICollection<League> Leagues { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
}
```
DB: `UNIQUE(Code)`, `Name VARCHAR(100) NOT NULL`, `Code VARCHAR(10) NOT NULL`

---

### 2. `League` (mới)

```csharp
// FootballBlog.Core/Models/League.cs
public class League
{
    public int Id { get; set; }
    public int ExternalId { get; set; }                 // api-football league ID
    public string Name { get; set; } = string.Empty;   // "Premier League"
    public string? LogoUrl { get; set; }
    public int CountryId { get; set; }
    public bool IsActive { get; set; } = true;          // false = archive giải đấu không theo dõi nữa

    public Country Country { get; set; } = null!;
    public ICollection<Match> Matches { get; set; } = [];
}
```
DB: `UNIQUE(ExternalId)`, FK `CountryId → Countries`

> **IsActive thay vì DELETE**: Matches cũ đã FK đến League — dùng soft-disable để archive mà không vi phạm FK.

---

### 3. `Team` (mới)

```csharp
// FootballBlog.Core/Models/Team.cs
public class Team
{
    public int Id { get; set; }
    public int ExternalId { get; set; }                 // api-football team ID
    public string Name { get; set; } = string.Empty;   // "Manchester United"
    public string? ShortName { get; set; }              // "Man Utd"
    public string? LogoUrl { get; set; }
    public int? CountryId { get; set; }                 // nullable (club vs national team)

    public Country? Country { get; set; }
    public ICollection<Match> HomeMatches { get; set; } = [];
    public ICollection<Match> AwayMatches { get; set; } = [];
}
```
DB: `UNIQUE(ExternalId)`, FK `CountryId → Countries` (optional)

---

### 4. `Match` (sửa lại)

```csharp
// FootballBlog.Core/Models/Match.cs — THAY THẾ toàn bộ
public class Match
{
    public int Id { get; set; }
    public int ExternalId { get; set; }

    // FK thay vì strings
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int LeagueId { get; set; }                   // FK → Leagues.Id (internal)

    public string Season { get; set; } = string.Empty; // "2024/2025"
    public string? Round { get; set; }                  // "Round 10"

    public DateTime KickoffUtc { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public string? VenueName { get; set; }
    public string? RefereeName { get; set; }
    public DateTime FetchedAt { get; set; }

    // Navigations
    public Team HomeTeam { get; set; } = null!;
    public Team AwayTeam { get; set; } = null!;
    public League League { get; set; } = null!;
    public MatchPrediction? Prediction { get; set; }
    public MatchContextData? ContextData { get; set; } // AI input — lazy loaded
}
```

**Bỏ**: `HomeTeam (string)`, `AwayTeam (string)`, `HomeTeamExternalId`, `AwayTeamExternalId`, `LeagueName (string)`

DB indexes thêm:
```sql
INDEX (HomeTeamId, KickoffUtc)   -- query form home team
INDEX (AwayTeamId, KickoffUtc)   -- query form away team
INDEX (LeagueId, Season)         -- query by league + season
```

---

### 5. `MatchContextData` (mới — tách ra để không load khi chỉ cần match list)

```csharp
// FootballBlog.Core/Models/MatchContextData.cs
public class MatchContextData
{
    public int Id { get; set; }
    public int MatchId { get; set; }

    /// <summary>JSONB blob chứa toàn bộ context cho AI prediction.</summary>
    public string ContextJson { get; set; } = "{}";

    public DateTime FetchedAt { get; set; }

    public Match Match { get; set; } = null!;
}
```
DB: `UNIQUE(MatchId)`, `ContextJson JSONB NOT NULL`, 1-to-1 với Match

---

### 6. `MatchContext` POCO (không phải entity — dùng để serialize/deserialize ContextJson)

```csharp
// FootballBlog.Core/Models/MatchContext.cs
public class MatchContext
{
    public H2HContext H2H { get; set; } = new();
    public TeamFormContext HomeForm { get; set; } = new();
    public TeamFormContext AwayForm { get; set; } = new();
    public LineupContext? Lineup { get; set; }
    public RefereeContext? Referee { get; set; }
    public FatigueContext? Fatigue { get; set; }
}

public class H2HContext
{
    public List<H2HMatch> RecentMatches { get; set; } = []; // 5 trận gần nhất
    public int HomeWins { get; set; }
    public int Draws { get; set; }
    public int AwayWins { get; set; }
}

public class H2HMatch
{
    public DateTime Date { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public string Competition { get; set; } = string.Empty;
}

public class TeamFormContext
{
    public string TeamName { get; set; } = string.Empty;
    public List<FormMatch> RecentMatches { get; set; } = []; // 5 trận gần nhất
    public string FormString { get; set; } = string.Empty;   // "WWDLW"
    public int GoalsScored { get; set; }
    public int GoalsConceded { get; set; }
}

public class FormMatch
{
    public DateTime Date { get; set; }
    public string Opponent { get; set; } = string.Empty;
    public bool IsHome { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public string Result { get; set; } = string.Empty; // "W", "D", "L"
    public string Competition { get; set; } = string.Empty;
}

public class LineupContext
{
    public List<string> HomeProbableXI { get; set; } = [];
    public List<string> AwayProbableXI { get; set; } = [];
    public List<string> HomeInjuries { get; set; } = [];  // treo giò / chấn thương
    public List<string> AwayInjuries { get; set; } = [];
}

public class RefereeContext
{
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }                 // "strict referee, 4.2 yellows/game"
}

public class FatigueContext
{
    public int? HomeDaysSinceLastMatch { get; set; }
    public int? AwayDaysSinceLastMatch { get; set; }
    public bool HomePlayingEurope { get; set; }
    public bool AwayPlayingEurope { get; set; }
    public string? Notes { get; set; }                 // "Home played Thu Europa League"
}
```

---

## Workflow Data Ingestion

### Job 1: `FetchUpcomingMatchesJob` [Cron 6h — Phase 4]

```
Football API /fixtures?next=20&league={id}
    |
    v
Với mỗi fixture (thứ tự bắt buộc do FK):
  1. Upsert Country    → by Code
  2. Upsert League     → by ExternalId
  3. Upsert Team (Home)→ by ExternalId
  4. Upsert Team (Away)→ by ExternalId
  5. Upsert Match      → by ExternalId (UPDATE nếu score/status thay đổi)
    |
    v
CommitAsync() — 1 transaction cho toàn bộ batch
    |
    v
Redis: INCR "football_api:requests:{date}" (rate limit counter)
```

**Upsert pattern (GetOrCreate):**
```csharp
var team = await uow.Teams.GetByExternalIdAsync(extId);
if (team == null)
    await uow.Teams.AddAsync(new Team { ExternalId = extId, Name = name });
else
    team.Name = name;  // update nếu API đổi tên chính thức
```

---

### Job 2: `FetchMatchContextJob` [Cron 1h, 24h trước kickoff — Phase 4]

```
Query: Match WHERE Status=Scheduled
             AND KickoffUtc <= NOW()+24h
             AND ContextData IS NULL
    |
    v
Với mỗi match (3-4 API calls, kiểm tra Redis quota trước mỗi call):
  a. GET /fixtures/headtohead?h2h={homeExtId}-{awayExtId}&last=5  → H2H
  b. GET /fixtures?team={homeExtId}&last=5&status=FT              → Home form
  c. GET /fixtures?team={awayExtId}&last=5&status=FT              → Away form
  d. GET /fixtures/lineups?fixture={fixtureId}                    → Lineup (có thể null)
    |
    v
Build MatchContext POCO từ responses
Serialize → JSON.Serialize(context)
Upsert MatchContextData { MatchId, ContextJson, FetchedAt }
    |
    v
CommitAsync()
```

**Rate limit guard (Redis):**
```csharp
var today = DateTime.UtcNow.ToString("yyyyMMdd");
var count = await redis.StringIncrementAsync($"football_api:requests:{today}");
if (count > 90)
{
    logger.LogWarning("Football API quota near limit ({Count}/100), skipping", count);
    return;
}
```

---

### Job 3: `GeneratePredictionJob` [Cron 1h — Phase 5]

```
Query: Match WHERE Status=Scheduled
             AND Prediction IS NULL
             AND ContextData IS NOT NULL      ← context đã sẵn sàng
             AND KickoffUtc <= NOW()+24h
    |
    v
Deserialize ContextJson → MatchContext
Build AI prompt từ MatchContext
    |
    v
Call IAIPredictionProvider.GenerateAsync(prompt)
    |
    v
Tạo MatchPrediction { MatchId, AIProvider, scores, confidence, analysis }
CommitAsync()
    |
    v
Trigger PublishPredictionJob
```

---

## Extensibility — Trả Lời Các Câu Hỏi Maintenance

| Tình huống | Cách xử lý |
|-----------|-----------|
| Thêm giải đấu mới (Serie A) | INSERT 1 row vào `Leagues` → `FetchUpcomingMatchesJob` tự upsert matches |
| Mùa giải mới 2025/2026 | Season = string "2025/2026" → tự sinh khi fetch API, không cần schema change |
| Đổi luật (VAR, thẻ mới) | Chỉ sửa AI prompt template — KHÔNG ảnh hưởng DB schema |
| Thêm sport mới (tennis) | Thêm `SportId` vào `League` entity + migration nhỏ khi cần |
| Trọng tài có thống kê | Thêm vào `RefereeContext.Notes` trong JSONB — không cần migration |
| AI cần thêm signal mới | Thêm field vào POCO `MatchContext` → serialize lại — không cần migration |
| Disable giải đấu cũ | `league.IsActive = false` — matches lịch sử vẫn giữ nguyên |

---

## Repositories Cần Thêm (Phase 4)

### Interfaces mới trong `IUnitOfWork`
```csharp
ICountryRepository Countries { get; }
ILeagueRepository Leagues { get; }
ITeamRepository Teams { get; }
IMatchContextRepository MatchContexts { get; }
```

### `IMatchRepository` — thêm 3 methods
```csharp
// 5 trận H2H gần nhất giữa 2 đội
Task<IEnumerable<Match>> GetH2HAsync(int homeTeamId, int awayTeamId, int count = 5);

// N trận gần nhất của 1 đội (home + away) — dùng để tính form
Task<IEnumerable<Match>> GetRecentByTeamAsync(int teamId, int count = 5);

// Trận chưa có ContextData và sắp đấu trong X giờ
Task<IEnumerable<Match>> GetWithoutContextAsync(int hoursAhead = 24);
```

### Query mẫu `GetH2HAsync`
```csharp
return await _dbSet
    .AsNoTracking()
    .Include(m => m.HomeTeam).Include(m => m.AwayTeam).Include(m => m.League)
    .Where(m => m.Status == MatchStatus.Finished &&
           ((m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId) ||
            (m.HomeTeamId == awayTeamId && m.AwayTeamId == homeTeamId)))
    .OrderByDescending(m => m.KickoffUtc)
    .Take(count)
    .ToListAsync();
```

---

## DTOs — Contract Không Đổi

`MatchSummaryDto` và `MatchPredictionDto` giữ nguyên các string property `HomeTeam`, `AwayTeam`, `LeagueName`.
Chỉ thay đổi **nguồn mapping**:

```csharp
// Trước: m.HomeTeam (string field)
// Sau:   m.HomeTeam.Name (navigation property)
new MatchSummaryDto(
    HomeTeam: m.HomeTeam.Name,
    AwayTeam: m.AwayTeam.Name,
    LeagueName: m.League.Name,
    ...
);
```

API response format không thay đổi — không break Web clients.

---

## Migration Plan (Thực Hiện Đầu Phase 4)

Table `Matches` hiện **rỗng** → 1 migration duy nhất `RefactorMatchSchema`:

```bash
dotnet ef migrations add RefactorMatchSchema \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

Migration sẽ:
1. `CREATE TABLE Countries`
2. `CREATE TABLE Leagues` (FK → Countries)
3. `CREATE TABLE Teams` (FK → Countries nullable)
4. `CREATE TABLE MatchContextData` (FK → Matches, JSONB column)
5. `ALTER TABLE Matches` — drop string columns, thêm FK columns

---

## Files Cần Tạo/Sửa (Phase 4 checklist)

| File | Action |
|------|--------|
| `FootballBlog.Core/Models/Country.cs` | Tạo mới |
| `FootballBlog.Core/Models/League.cs` | Tạo mới |
| `FootballBlog.Core/Models/Team.cs` | Tạo mới |
| `FootballBlog.Core/Models/MatchContextData.cs` | Tạo mới (entity) |
| `FootballBlog.Core/Models/MatchContext.cs` | Tạo mới (POCO) |
| `FootballBlog.Core/Models/Match.cs` | Sửa — thay strings bằng FKs |
| `FootballBlog.Core/Interfaces/ICountryRepository.cs` | Tạo mới |
| `FootballBlog.Core/Interfaces/ILeagueRepository.cs` | Tạo mới |
| `FootballBlog.Core/Interfaces/ITeamRepository.cs` | Tạo mới |
| `FootballBlog.Core/Interfaces/IMatchContextRepository.cs` | Tạo mới |
| `FootballBlog.Core/Interfaces/IMatchRepository.cs` | Thêm 3 methods |
| `FootballBlog.Core/Interfaces/IUnitOfWork.cs` | Thêm 4 properties |
| `FootballBlog.Infrastructure/Data/ApplicationDbContext.cs` | Thêm DbSets + config |
| `FootballBlog.Infrastructure/Repositories/CountryRepository.cs` | Tạo mới |
| `FootballBlog.Infrastructure/Repositories/LeagueRepository.cs` | Tạo mới |
| `FootballBlog.Infrastructure/Repositories/TeamRepository.cs` | Tạo mới |
| `FootballBlog.Infrastructure/Repositories/MatchContextRepository.cs` | Tạo mới |
| `FootballBlog.Infrastructure/Repositories/MatchRepository.cs` | Thêm 3 methods |
| `FootballBlog.Infrastructure/Data/UnitOfWork.cs` | Thêm 4 properties |
| Migration `RefactorMatchSchema` | Tạo + apply |
| `CLAUDE.md` — IUnitOfWork quick ref | Cập nhật 4 repos mới |
