# Plan: Audit & Update — TODO.md + Config Files

## Context
Phase 4 vừa được implement (Football API Client + Hangfire Jobs). Cần:
1. Cập nhật TODO.md để phản ánh đúng trạng thái thực tế (các checkbox sai)
2. Fix các bugs/issues tìm thấy qua audit
3. Đồng bộ config files (appsettings.Development.example.json lỗi thời + CLAUDE.md chưa cập nhật)

---

## Bugs / Issues Tìm Được

### BUG-1 — `ILiveScoreService` là orphan interface (không có impl)
- **File**: `FootballBlog.Core/Interfaces/Services/ILiveScoreService.cs`
- **Vấn đề**: Interface tồn tại với 2 method (`GetLiveMatchesAsync`, `GetMatchByIdAsync`) nhưng không có implementation class, không được register trong Program.cs
- **Fix**: Thêm vào TODO.md Phase 4 pending — cần implement `LiveScoreService` + register trước khi build Blazor live widget

### BUG-2 — `appsettings.Development.example.json` thiếu sections mới
- **File**: `FootballBlog.API/appsettings.Development.example.json`
- **Vấn đề**: Thiếu `"Redis"` connection string và toàn bộ `"FootballApi"` section — đã có trong `appsettings.Development.json` nhưng example chưa được cập nhật
- **Fix**: Sync example file với Development file (chừa ApiKey trống)

### BUG-3 — TODO.md Phase 4 checkboxes sai
- **Vấn đề**: Tất cả Phase 4 tasks vẫn `[ ]` dù đã implement xong phần lớn
- **Thực tế**:
  - ✅ FootballApiClient + Polly retry — DONE
  - ✅ Redis rate limit counter — DONE  
  - ✅ Hangfire jobs: FetchUpcomingMatchesJob (cron 6h) + LiveScorePollingJob (1 min) — DONE
  - ✅ Match + MatchEvent schema đã có từ Phase 5 entity work — DONE
  - ❌ SignalR Hub (LiveScoreHub) + Redis backplane — chưa
  - ❌ Blazor LiveScore pages + widget — chưa
  - ❌ ILiveScoreService implementation — chưa (BUG-1)
  - ⚠️ Note: cron LiveScorePolling thực tế là **1 phút** (Cron.Minutely()), TODO.md ghi 30s → cần sửa

### BUG-4 — CLAUDE.md "appsettings Hiện Tại" chưa cập nhật
- **Vấn đề**: Section này chỉ liệt kê `ConnectionStrings`, `WebBaseUrl`, `Serilog` — thiếu `FootballApi` section và `Redis` connection string đã có thực tế
- **Fix**: Update CLAUDE.md để reflect đúng trạng thái appsettings.json hiện tại

---

## Files Cần Sửa (4 files)

| File | Thay đổi |
|------|---------|
| `TODO.md` | Tick ✅ Phase 4 items đã xong; sửa "30s" → "1 min"; thêm ILiveScoreService vào pending |
| `FootballBlog.API/appsettings.Development.example.json` | Thêm `Redis` + `FootballApi` sections |
| `CLAUDE.md` | Update "appsettings Hiện Tại" section |
| Xóa plan `soft-waddling-penguin.md` | Plan Phase 4 đã hoàn tất (theo cleanup rule) |

---

## Chi Tiết Thay Đổi

### TODO.md — Phase 4 update
```markdown
### Phase 4 — Realtime Football ⬜ (In Progress)
- [x] FootballApiClient (IHttpClientFactory + Polly retry)
- [x] Redis rate limit counter (100 req/ngày)
- [x] Match + MatchEvent schema: enum MatchStatus, EventType  ← đã có từ trước
- [x] Hangfire jobs: FetchUpcomingMatchesJob (cron 6h), LiveScorePollingJob (1 min, adaptive gate)
- [ ] ILiveScoreService implementation (LiveScoreService) + register DI
- [ ] SignalR Hub (LiveScoreHub) + Redis backplane
- [ ] Blazor LiveScore pages + widget (InteractiveServer)
```

### appsettings.Development.example.json — thêm vào
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=footballblog;Username=admin;Password=localpass",
    "Redis": "localhost:6379"
  },
  "WebBaseUrl": "https://localhost:7241",
  "FootballApi": {
    "BaseUrl": "https://v3.football.api-sports.io",
    "ApiKey": "YOUR_API_KEY_HERE",
    "DailyRequestLimit": 100,
    "FixturesPerLeague": 20,
    "LeagueIds": [39, 140, 135, 78, 61, 94, 2, 3, 848, 531, 1, 45, 48, 253, 307]
  },
  "Serilog": { ... giữ nguyên phần Debug overrides ... }
}
```

### CLAUDE.md — "appsettings Hiện Tại" section
Cập nhật để bao gồm `FootballApi` + `Redis` connection string.

---

## Không Cần Làm

- Không sửa Program.cs (đúng rồi — DI, Hangfire, Polly, jobs filter đều ok)
- Không sửa .csproj (packages đã đúng)
- Không tạo migration mới
- Không implement ILiveScoreService ngay (chỉ add vào TODO.md)

---

## Verification

1. `cat TODO.md` → Phase 4 items reflect đúng trạng thái
2. `cat appsettings.Development.example.json` → có đủ Redis + FootballApi sections
3. `cat CLAUDE.md` → "appsettings Hiện Tại" khớp với file thực tế
4. `dotnet build FootballBlog.sln` → 0 errors (không code changes → không break gì)
