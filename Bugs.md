# Bugs & Known Issues

---

## [BUG-5] Thiếu fixtures/standings endpoints và UI chưa đọc từ DB

**Status:** Chờ API seeding hoàn tất trước khi fix

**Context:**
Data đã được seed một phần (3 leagues đầy đủ, 12 leagues còn thiếu do 429). Sau khi re-run `SeedLeagueDataJob` thành công toàn bộ 15 leagues mới nên bắt đầu build UI.

**Hiện trạng:**
- Không có `IFixturesApiClient`, `IStandingsApiClient` trong Web project
- Prototype `FootballBlog.Web/wwwroot/prototype/home.html` đang dùng mock data
- Không có Blazor component nào đọc fixtures/standings/standings từ DB

**Data available khi seeding xong:**
- 15 leagues: Premier League, La Liga, Serie A, Bundesliga, Ligue 1, Primeira Liga, Champions League, Europa League, Conference League, Super Cup, World Cup, FA Cup, Carabao Cup, MLS, Saudi Pro League
- ~380 fixtures/league + standings 20 teams/league

**Fix plan (theo thứ tự):**

1. **API endpoints** — tạo 2 controllers mới:
   - `GET /api/fixtures?leagueId={id}&season={year}` → `FixturesController`
   - `GET /api/standings/{leagueId}?season={year}` → `StandingsController`
   - Skill: `/new-feature` hoặc thêm vào controller hiện có

2. **Typed HTTP clients** (Web project):
   - `IFixturesApiClient` + `FixturesApiClient`
   - `IStandingsApiClient` + `StandingsApiClient`
   - Skill: `/api-client`

3. **Blazor components** — tách từ `prototype/home.html`:
   - Match list component (center column)
   - Standings table component (sidebar hoặc modal)
   - Country/league tree (left sidebar)
   - Skill: `/blazor-page`

**Điều kiện để bắt đầu fix:**
- [ ] `SeedLeagueDataJob` chạy thành công toàn bộ 15 leagues (kiểm tra `jobs.log`)
- [ ] DB có đủ data: `SELECT COUNT(*) FROM "Matches"` nên ≥ 4000 rows
