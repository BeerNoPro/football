# BA/SA Analysis — Prototype Gap & Data Architecture

## Context

Prototype hiện có 19 files (15 HTML + 3 data .js + 2 CSS + 2 JS assets). Cần:
1. Xác định trang còn thiếu theo user flow
2. Chuyển toàn bộ mock data từ `window.MOCK_*` (JS global) sang `.json` files load qua `fetch()` — mô phỏng đúng cách API/database sẽ hoạt động

---

## Phần 1 — Inventory Hiện Tại

### Public Pages (8/11)
| Page | File | Status |
|------|------|--------|
| Home — Match schedule + live | home.html | ✅ Có |
| League detail (fixtures/results/standings/scorers) | league-page.html | ✅ Có |
| Match detail (lineups/stats/events) | match-detail.html | ✅ Có |
| Post detail (AI prediction article) | post-detail.html | ✅ Có |
| Predictions listing | predictions.html | ✅ Có |
| Search results | search-results.html | ✅ Có |
| Team profile | team-profile.html | ✅ Có |
| Category / Tag listing | category-tag.html | ✅ Có |
| **Player profile** | player-profile.html | ❌ THIẾU |
| **News / Articles listing** | news.html | ❌ THIẾU |
| **404 / Error page** | 404.html | ❌ THIẾU |

### Admin Pages (7/9)
| Page | File | Status |
|------|------|--------|
| Login | admin-login.html | ✅ Có |
| Dashboard | admin-dashboard.html | ✅ Có |
| Posts list | admin-posts.html | ✅ Có |
| Post editor (create/edit) | admin-post-editor.html | ✅ Có |
| Predictions management | admin-predictions.html | ✅ Có |
| Categories & Tags | admin-categories.html | ✅ Có |
| Job monitor (Hangfire) | admin-job-monitor.html | ✅ Có |
| **Matches management** | admin-matches.html | ❌ THIẾU |
| **System settings** | admin-settings.html | ❌ THIẾU |

### Data Files (3/10)
| Data | File hiện tại | Trạng thái |
|------|--------------|-----------|
| Leagues (sidebar tree) | data/leagues.js (window.MOCK_LEAGUES) | ⚠️ Cần đổi → .json |
| Matches (today schedule) | data/matches.js (window.MOCK_MATCHES) | ⚠️ Cần đổi → .json |
| Posts (AI blog) | data/posts.js (window.MOCK_POSTS) | ⚠️ Cần đổi → .json |
| Match detail (lineups, stats, events) | ❌ Không có | ❌ THIẾU |
| League detail (standings, scorers) | ❌ Không có | ❌ THIẾU |
| Team profile (squad, fixtures) | ❌ Không có | ❌ THIẾU |
| Player profile (bio, stats, career) | ❌ Không có | ❌ THIẾU |
| Predictions listing | ❌ Không có | ❌ THIẾU |
| Categories & Tags | ❌ Không có | ❌ THIẾU |
| Search results (sample) | ❌ Không có | ❌ THIẾU |

---

## Phần 2 — Missing Pages Cần Tạo

### Priority 1 — Có link từ trang hiện có

#### `player-profile.html`
- **Link từ**: match-detail.html (lineup player names), team-profile.html (squad tab)
- **Sections**: Player header (ảnh, tên, quốc tịch, số áo), Thống kê mùa (goals/assists/apps), Bio/Thông tin cá nhân, Lịch sử câu lạc bộ, Số liệu chi tiết (passes, tackles, ratings), Upcoming matches của team

#### `admin-matches.html`
- **Link từ**: admin-job-monitor.html (FetchUpcomingMatchesJob), admin-predictions.html
- **Sections**: Filter bar (league, date range), Matches table (team A vs B, date, status, prediction status), Action buttons (trigger prediction, view match detail, link prediction post)
- **Lý do quan trọng**: Admin cần thấy match nào đã có prediction, match nào chưa — đây là core workflow của hệ thống AI prediction

#### `admin-settings.html`
- **Link từ**: admin sidebar menu (chưa có)
- **Sections**: 4 tab — AI Config (Claude/Gemini API keys, model, max tokens), Football API (API key, daily limit quota), Telegram (bot token, channel IDs), General (site name, timezone, auto-publish toggle)
- **Lý do quan trọng**: Phase 4-6 cần config nhiều service — cần màn hình để quản lý

### Priority 2 — Nice to have

#### `news.html`
- General articles/news listing (không phải AI prediction)
- Sections: Filter bar (All/Breaking/Analysis/Transfer), Post grid 3 columns, Pagination, Featured post hero

#### `404.html`
- Error page theo design system — background đen, accent lime, nút "Về trang chủ"

---

## Phần 3 — Data Architecture: Migration sang JSON

### Vấn đề hiện tại
```js
// Cách cũ — window global, không giống API thật
<script src="data/leagues.js"></script>  // window.MOCK_LEAGUES = [...]
renderLeagueTree(el, window.MOCK_LEAGUES);
```

### Cách mới — fetch() JSON
```js
// Giống API call thật — chỉ thay URL là xong
async function loadData(url) {
  const res = await fetch(url);
  return res.json();
}

// Trong render.js
const leagues = await loadData('data/leagues.json');
renderLeagueTree(el, leagues);
```

### JSON Files Cần Tạo

#### `data/leagues.json`
```json
{
  "countries": [
    {
      "id": "vn", "name": "Việt Nam", "flag": "🇻🇳",
      "leagues": [
        { "id": 1, "name": "V.League 1", "slug": "v-league-1", "liveCount": 2 }
      ]
    }
  ]
}
```

#### `data/matches.json`
```json
{
  "date": "2026-04-09",
  "liveCount": 3,
  "byLeague": [
    {
      "leagueId": 1, "leagueName": "V.League 1", "country": "Việt Nam",
      "round": "Vòng 12",
      "matches": [
        {
          "id": 101, "kickoff": "19:00", "status": "LIVE", "elapsed": 67,
          "homeTeam": { "id": 10, "name": "Hà Nội FC", "logo": "🏆" },
          "awayTeam": { "id": 11, "name": "HAGL", "logo": "⚽" },
          "score": { "home": 2, "away": 1 },
          "predictionUrl": "/predictions/ha-noi-vs-hagl",
          "detailUrl": "/matches/101"
        }
      ]
    }
  ]
}
```

#### `data/posts.json`
```json
{
  "featured": { "id": 1, "title": "...", "tag": "AI Prediction", ... },
  "items": [ ... ]
}
```

#### `data/match-detail.json` (MỚI)
```json
{
  "id": 101,
  "status": "LIVE", "elapsed": 67,
  "homeTeam": { "id": 10, "name": "Hà Nội FC", "form": ["W","W","D","L","W"] },
  "awayTeam": { "id": 11, "name": "HAGL", "form": ["L","W","W","D","L"] },
  "score": { "home": 2, "away": 1 },
  "venue": "Sân Mỹ Đình", "referee": "Nguyễn Văn A", "attendance": 18500,
  "lineups": {
    "home": [
      { "number": 1, "name": "Nguyễn Văn Cường", "position": "GK", "playerId": 201 }
    ],
    "away": [ ... ]
  },
  "stats": {
    "possession": { "home": 58, "away": 42 },
    "shots": { "home": 12, "away": 7 },
    "shotsOnTarget": { "home": 5, "away": 3 },
    "corners": { "home": 6, "away": 3 },
    "fouls": { "home": 9, "away": 14 }
  },
  "events": [
    { "minute": 23, "type": "GOAL", "team": "home", "player": "Nguyễn Tiến Linh", "assist": "Đỗ Hùng Dũng" },
    { "minute": 45, "type": "YELLOW", "team": "away", "player": "Nguyễn Công Phượng" }
  ]
}
```

#### `data/league-detail.json` (MỚI)
```json
{
  "leagueId": 1, "name": "V.League 1", "season": "2026",
  "standings": [
    { "rank": 1, "teamId": 10, "teamName": "Hà Nội FC", "logo": "🏆",
      "played": 11, "won": 8, "drawn": 2, "lost": 1, "gf": 24, "ga": 8, "gd": 16, "points": 26,
      "form": ["W","W","W","D","W"], "zone": "CL" }
  ],
  "topScorers": [
    { "rank": 1, "playerId": 301, "playerName": "Nguyễn Tiến Linh",
      "teamName": "Hà Nội FC", "goals": 9, "assists": 4 }
  ]
}
```

#### `data/team.json` (MỚI)
```json
{
  "id": 10, "name": "Hà Nội FC", "logo": "🏆",
  "founded": 2010, "stadium": "Sân Hàng Đẫy", "capacity": 22000,
  "coach": "Đặng Trần Chỉnh",
  "season": { "played": 11, "won": 8, "drawn": 2, "lost": 1, "goals": 24, "conceded": 8, "points": 26, "rank": 1 },
  "form": ["W","W","W","D","W"],
  "squad": [
    { "id": 201, "name": "Nguyễn Văn Cường", "number": 1, "position": "GK", "nationality": "🇻🇳", "age": 28 }
  ],
  "fixtures": [ ... ]
}
```

#### `data/player.json` (MỚI)
```json
{
  "id": 301, "name": "Nguyễn Tiến Linh",
  "nationality": "🇻🇳", "dateOfBirth": "1997-09-12", "age": 28,
  "position": "ST", "number": 9, "height": "175cm", "weight": "68kg",
  "currentTeam": { "id": 10, "name": "Hà Nội FC" },
  "seasonStats": { "appearances": 11, "goals": 9, "assists": 4, "yellowCards": 2, "redCards": 0, "rating": 7.8 },
  "careerHistory": [
    { "teamName": "SLNA", "from": "2015", "to": "2019", "apps": 89, "goals": 32 }
  ]
}
```

#### `data/predictions.json` (MỚI)
```json
{
  "accuracy": { "total": 127, "correct": 89, "rate": 70.1 },
  "filters": { "leagues": [...], "confidenceLevels": ["HIGH", "MEDIUM", "LOW"] },
  "items": [
    {
      "id": 1, "matchId": 101,
      "homeTeam": "Hà Nội FC", "awayTeam": "HAGL",
      "kickoff": "2026-04-10T19:00:00",
      "predictedScore": "2-1", "confidence": 78, "confidenceLevel": "HIGH",
      "aiSummary": "Hà Nội FC có lợi thế sân nhà...",
      "postUrl": "/posts/ha-noi-vs-hagl-prediction",
      "status": "PENDING"
    }
  ]
}
```

#### `data/categories.json` (MỚI)
```json
{
  "categories": [
    { "id": 1, "name": "AI Prediction", "slug": "ai-prediction", "icon": "🤖", "postCount": 41 },
    { "id": 2, "name": "Phân tích", "slug": "phan-tich", "icon": "📊", "postCount": 18 }
  ],
  "tags": [
    { "id": 1, "name": "Premier League", "slug": "premier-league", "postCount": 23 },
    { "id": 2, "name": "V.League", "slug": "v-league", "postCount": 15 }
  ]
}
```

#### `data/search.json` (MỚI)
```json
{
  "query": "hà nội",
  "totalResults": 18,
  "matches": [
    { "id": 101, "homeTeam": "Hà Nội FC", "awayTeam": "HAGL", "date": "2026-04-10", "competition": "V.League 1" }
  ],
  "posts": [
    { "id": 1, "title": "Dự đoán Hà Nội FC vs HAGL", "excerpt": "...", "tag": "AI Prediction" }
  ],
  "teams": [
    { "id": 10, "name": "Hà Nội FC", "league": "V.League 1" }
  ],
  "suggestions": ["Hà Nội FC", "Hà Nội vs HAGL", "V.League 2026"]
}
```

---

## Phần 4 — render.js Refactor

### Thay đổi data loader
```js
// Thêm vào render.js — data loader utility
const DATA_BASE = 'data/';

async function fetchData(name) {
  const res = await fetch(`${DATA_BASE}${name}.json`);
  if (!res.ok) throw new Error(`Failed to load ${name}`);
  return res.json();
}

// Khởi tạo home page
async function initHomePage() {
  const [leagues, matches, posts] = await Promise.all([
    fetchData('leagues'),
    fetchData('matches'),
    fetchData('posts')
  ]);
  renderLeagueTree(document.querySelector('.league-tree'), leagues);
  renderMatches(document.querySelector('.matches-list'), matches);
  renderPosts(document.querySelector('.right-posts'), posts);
  updateLivePill(matches.liveCount);
}
```

---

## Phần 5 — Kế Hoạch Thực Hiện

### Bước 1 — Tạo JSON files (10 files)
Chuyển 3 file .js hiện tại → .json + tạo 7 file mới:
`leagues.json`, `matches.json`, `posts.json`, `match-detail.json`, `league-detail.json`, `team.json`, `player.json`, `predictions.json`, `categories.json`, `search.json`

### Bước 2 — Refactor render.js
- Xóa dependency vào `window.MOCK_*`
- Thêm `fetchData()` utility
- Mỗi page gọi `initXxxPage()` async khi DOMContentLoaded

### Bước 3 — Cập nhật HTML pages
Thay `<script src="data/leagues.js">` → không cần nữa (render.js tự fetch)

### Bước 4 — Tạo missing pages
Thứ tự: `player-profile.html` → `admin-matches.html` → `admin-settings.html` → `news.html` → `404.html`

---

## Critical Files
- `FootballBlog.Web/wwwroot/prototype/assets/render.js` — refactor data loading
- `FootballBlog.Web/wwwroot/prototype/data/` — toàn bộ data files
- `FootballBlog.Web/wwwroot/prototype/home.html` — remove script tags cũ

## Verification
1. Mở home.html qua Live Server → kiểm tra data load từ JSON (Network tab)
2. Click vào match → match-detail.html load từ match-detail.json
3. Click team name → team-profile.html load từ team.json
4. Click player → player-profile.html load từ player.json
5. Admin pages load categories/matches từ JSON tương ứng
