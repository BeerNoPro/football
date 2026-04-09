# FOLLOW.md — Bản đồ Prototype FootballBlog

> Dùng file này để hiểu toàn bộ flow prototype trước khi code.  
> Cập nhật khi thêm trang mới hoặc thay đổi logic điều hướng.

---

## Tổng quan

| Thống kê | Giá trị |
|----------|---------|
| Tổng trang | 21 HTML |
| Public pages | 11 (bao gồm 404) |
| Admin pages | 10 |
| Data JSON | 10 file |
| Shared JS | common.js + render.js |
| Shared CSS | common.css + admin-common.css |
| Layout chính | 3-col (240px sidebar + 1fr + 300px) |
| Layout phụ | 2-col (240px sidebar + 1fr), standalone |
| Theme | Dark — accent `#c8f04d` (lime) |

---

## Sơ đồ điều hướng

```
PUBLIC SITE
═══════════════════════════════════════════════════════════

  [Logo / bất kỳ trang] ──────────────────────→ home.html
  [Sidebar league click] ──────────────────────→ home.html?league=X
                                                     ↓ applyLeagueParam()
                                                  scroll + highlight

  home.html
    ├─ Tab "Dự đoán AI"  ──────→ predictions.html
    ├─ Tab "Tin tức"     ──────→ news.html
    ├─ Search (Enter)    ──────→ search-results.html?q=
    ├─ Match row click   ──────→ match-detail.html
    ├─ Team name click   ──────→ team-profile.html
    ├─ Post card (right) ──────→ post-detail.html
    ├─ lg-name click     ──────→ league-page.html?league=X
    ├─ User row          ──────→ admin-dashboard.html
    └─ Logout btn        ──────→ admin-login.html

  predictions.html
    ├─ lg-name click     ──────→ league-page.html?league=X
    └─ "Phân tích →"     ──────→ post-detail.html

  news.html
    └─ Post card / link  ──────→ post-detail.html

  league-page.html
    └─ Match row click   ──────→ match-detail.html

  match-detail.html
    └─ League tag        ──────→ league-page.html

  team-profile.html
    ├─ Player row        ──────→ player-profile.html
    └─ Match row         ──────→ match-detail.html

  player-profile.html
    └─ Club link         ──────→ team-profile.html

  post-detail.html
    └─ Breadcrumb        ──────→ home.html

  search-results.html
    └─ (kết quả click)   ──────→ match-detail.html / post-detail.html

  category-tag.html
    └─ Post card         ──────→ post-detail.html

  404.html
    └─ Quick links       ──────→ home / predictions / news / league / team / search / category


ADMIN SITE
═══════════════════════════════════════════════════════════

  admin-login.html
    └─ Submit form       ──────→ admin-dashboard.html

  admin-dashboard.html
    ├─ "Xem trang web"   ──────→ home.html
    ├─ "Tạo bài viết"    ──────→ admin-post-editor.html
    └─ Sidebar menu      ──────→ (tất cả admin pages)

  admin-posts.html
    └─ "Tạo / Sửa"       ──────→ admin-post-editor.html

  admin-post-editor.html
    └─ "Đăng" / Back     ──────→ admin-posts.html

  admin-matches.html
    ├─ "Xem trận"        ──────→ match-detail.html
    └─ "Xem bài"         ──────→ post-detail.html

  [Mọi admin page]
    ├─ Logo / top nav    ──────→ home.html
    └─ Logout            ──────→ admin-login.html
```

---

## Bảng trang Public

| File | Tiêu đề | Layout | Data JSON | Initializer |
|------|---------|--------|-----------|-------------|
| `home.html` | Trang chủ | 3-col | leagues + matches + posts | `initHomePage()` |
| `news.html` | Tin tức | 3-col | leagues + posts | inline fetch ⚠️ |
| `league-page.html` | Chi tiết giải | 3-col | leagues + league-detail | `initLeaguePage()` |
| `match-detail.html` | Chi tiết trận | 3-col | leagues + match-detail | `initMatchDetailPage()` |
| `post-detail.html` | Bài viết | 3-col | leagues | `initPostDetailPage()` |
| `predictions.html` | Dự đoán AI | 3-col | leagues + predictions | `initPredictionsPage()` |
| `player-profile.html` | Cầu thủ | 3-col | leagues + player | `initPlayerPage()` |
| `team-profile.html` | Đội bóng | 3-col | leagues + team | `initTeamPage()` |
| `search-results.html` | Kết quả tìm kiếm | 2-col | leagues + search | `initSearchPage()` |
| `category-tag.html` | Category / Tag | 2-col | leagues + categories | `initCategoryPage()` |
| `404.html` | Not Found | standalone | — | — |

---

## Bảng trang Admin

| File | Tiêu đề | Mô tả ngắn |
|------|---------|------------|
| `admin-login.html` | Đăng nhập | Form auth, redirect dashboard |
| `admin-dashboard.html` | Dashboard | Stats cards, recent activity, bảng bài viết |
| `admin-posts.html` | Bài viết | Danh sách + filter + action table |
| `admin-post-editor.html` | Tạo/Sửa bài | Rich text editor + metadata |
| `admin-predictions.html` | Dự đoán AI | Bảng predictions + accuracy stats |
| `admin-categories.html` | Categories & Tags | Tree + detail panel |
| `admin-matches.html` | Trận đấu | Danh sách trận + link post/detail |
| `admin-users.html` | Users | CRUD user table |
| `admin-job-monitor.html` | Job Monitor | API quota, Hangfire jobs, Telegram status |
| `admin-settings.html` | Cài đặt | System config form |

---

## Data JSON

| File | Entity | Dùng bởi |
|------|--------|----------|
| `leagues.json` | Sidebar league tree (country groups) | **Mọi trang** |
| `matches.json` | Match list theo giải + liveCount | home |
| `match-detail.json` | Chi tiết 1 trận (lineup, events, stats) | match-detail |
| `league-detail.json` | Standings, top scorers, fixtures | league-page |
| `posts.json` | Danh sách blog posts + featured | home (right), news |
| `predictions.json` | AI predictions nhóm theo league | predictions |
| `player.json` | Profile cầu thủ (stats, career) | player-profile |
| `team.json` | Profile đội (squad, results) | team-profile |
| `categories.json` | Category/tag + posts | category-tag |
| `search.json` | Kết quả mix (matches + posts + teams) | search-results |

---

## Data Flow Pattern

```
DOMContentLoaded
    ↓
init{Page}Page()                          ← gọi từ inline <script> cuối trang
    ↓
Promise.all([fetchData('leagues'), ...])  ← song song, load từ data/*.json
    ↓
renderLeagueTree()   → .league-tree       ← left sidebar
render{Content}()    → .matches-list      ← center main
renderPosts()        → .right-scroll      ← right sidebar (chỉ home)
    ↓
applyLeagueParam()                        ← home only: đọc ?league= URL param
    ↓
highlight .league-item[data-league=X]
+ scrollIntoView #m-X
```

---

## Shared JS — common.js

| Hàm | Mô tả |
|-----|-------|
| `toggleCountry(id)` | Collapse/expand country group ở sidebar |
| `selectLeague(el, leagueId)` | Click league: home → scroll; others → `home.html?league=X` |
| `applyLeagueParam()` | Đọc `?league=` URL, highlight + scroll sau render |
| `filterLeagues(q)` | Live search sidebar theo tên giải/quốc gia |
| `toggleLg(hdr)` | Collapse/expand league group trong match list |
| `setTab(el)` | Switch tab trong `.tabs` parent |
| `setTabBar(el)` | Switch tab trong `.tab-bar` parent |
| `setDate(el)` | Chọn ngày trong date bar |
| `setRightTab(el)` | Switch tab right sidebar |
| `setDetailTab(el, groupClass)` | Switch tab + panel ở match-detail |
| `updateLivePill(count)` | Cập nhật số live match trên pill + tab badge |

---

## Shared JS — render.js

**Page initializers** (9):
`initHomePage` · `initLeaguePage` · `initMatchDetailPage` · `initTeamPage` · `initPlayerPage` · `initPredictionsPage` · `initCategoryPage` · `initPostDetailPage` · `initSearchPage`

**Render functions** (10):
`renderLeagueTree` · `renderMatches` · `renderLeagueGroup` · `renderMatchRow` · `renderPosts` · `renderFeaturedPost` · `renderPostItem` · `updateLivePill`  
*(+ renderLeagueDetail, renderMatchDetail, renderTeam, renderPlayer, renderPredictions, renderCategory, renderSearch — inline trong từng trang)*

---

## Shared CSS — common.css

| Class | Mô tả |
|-------|-------|
| `.app` | Grid 3-col: `240px 1fr 300px`, height 100vh |
| `.app-2col` | Grid 2-col: `240px 1fr`, height 100vh |
| `.left-sidebar` | Nav sidebar trái, 240px, scroll độc lập |
| `.main-col` | Center column, flex column, overflow hidden |
| `.main-scroll` | Scroll container bên trong main-col |
| `.matches-list` | Danh sách trận, flex:1, scroll độc lập |
| `.right-sidebar` | Panel phải 300px (AI blog), scroll độc lập |
| `.match-row` | Grid 5-col: `50px 1fr 72px 1fr 52px` |
| `.lg` / `.lg-header` | League group + header trong match list |
| `.league-item` | Link giải trong sidebar (active state: lime bg + border-left) |
| `.tab-btn` | Tab button (`.active` → lime underline) |
| `.badge-live` / `.badge-ft` / `.badge-sch` | Status badge trận đấu |
| `.live-dot` | Animated green dot |

---

## Vấn đề còn thiếu / cần review

### ❌ Trang chưa tạo
| File cần tạo | Mô tả |
|-------------|-------|
| `admin-team.html` | Admin CRUD đội bóng |
| `admin-players.html` | Admin CRUD cầu thủ |

### ⚠️ Logic chưa đúng / chưa hoàn thiện

| Vị trí | Vấn đề |
|--------|--------|
| `news.html` | Dùng inline `fetchData()` thay vì `initNewsPage()` — không có trong render.js, không đồng bộ pattern |
| `predictions.html` | `toggleLg(this)` gọi `.closest('.lg')` nhưng section dùng class `pred-section` → collapse không hoạt động |
| `search-results.html` | Search bar không đọc `?q=` param từ URL khi load — kết quả hardcoded mock |
| `home.html` date bar | Chọn ngày không filter match list theo ngày thực tế |
| `render.js` | Thiếu `initNewsPage()` — nếu news.html refactor sẽ phải thêm |

---

*Cập nhật lần cuối: 2026-04-09*
