# Plan: Viết FOLLOW.md cho Prototype

## Context
Prototype hiện có 21 HTML file, 10 JSON data, 3 shared JS/CSS — rất khó diễn giải lại từ đầu mỗi session mới. Mục tiêu: viết một file `FOLLOW.md` làm "bản đồ" toàn bộ prototype: trang nào link tới đâu, logic click nào trigger gì, data nào đến render nào, phần nào còn thiếu.

---

## Output File
`FootballBlog.Web/wwwroot/FOLLOW.md`

---

## Cấu trúc FOLLOW.md

### 1. Tổng quan nhanh
- Số trang, layout patterns, dark theme tokens

### 2. Sơ đồ điều hướng (Navigation Map)
ASCII flow chart phân 2 section:
- **Public Site**: home → news, predictions, league, match, post, player, team, search, category, 404
- **Admin Site**: login → dashboard → sidebar menu items

### 3. Bảng trang Public (10 trang)

| File | Tiêu đề | Layout | Data JSON | Initializer | Trạng thái |
|------|---------|--------|-----------|-------------|------------|
| home.html | Trang chủ | 3-col | leagues + matches + posts | initHomePage() | ✅ Done |
| news.html | Tin tức | 3-col | leagues + posts | inline fetch | ✅ Done |
| league-page.html | Chi tiết giải | 3-col | leagues + league-detail | initLeaguePage() | ✅ Done |
| match-detail.html | Chi tiết trận | 3-col | leagues + match-detail | initMatchDetailPage() | ✅ Done |
| post-detail.html | Bài viết | 3-col | leagues | initPostDetailPage() | ✅ Done |
| predictions.html | Dự đoán AI | 3-col | leagues + predictions | initPredictionsPage() | ✅ Done |
| player-profile.html | Cầu thủ | 3-col | leagues + player | initPlayerPage() | ✅ Done |
| team-profile.html | Đội bóng | 3-col | leagues + team | initTeamPage() | ✅ Done |
| search-results.html | Tìm kiếm | 2-col | leagues + search | initSearchPage() | ✅ Done |
| category-tag.html | Category | 2-col | leagues + categories | initCategoryPage() | ✅ Done |
| 404.html | Not Found | standalone | — | — | ✅ Done |

### 4. Bảng trang Admin (11 trang)

| File | Tiêu đề | Trạng thái |
|------|---------|------------|
| admin-login.html | Đăng nhập | ✅ |
| admin-dashboard.html | Dashboard | ✅ |
| admin-posts.html | Danh sách bài viết | ✅ |
| admin-post-editor.html | Tạo/Sửa bài | ✅ |
| admin-predictions.html | Quản lý dự đoán AI | ✅ |
| admin-categories.html | Categories & Tags | ✅ |
| admin-matches.html | Quản lý trận đấu | ✅ |
| admin-users.html | Quản lý user | ✅ |
| admin-job-monitor.html | Job Monitor / API Quota | ✅ |
| admin-settings.html | Cài đặt hệ thống | ✅ |

**Thiếu** (❌): `admin-team.html`, `admin-players.html` — chưa có trang quản lý team/player phía admin.

### 5. Navigation Click Logic
Chi tiết từng link/click quan trọng:
- Sidebar league click → `selectLeague()` → nếu không phải home thì `home.html?league=X`
- Home load → `applyLeagueParam()` → highlight + scroll
- Logo → `home.html`
- User row → `admin-dashboard.html`
- Logout btn → `admin-login.html`
- Search enter → `search-results.html?q=`
- Match row click → `match-detail.html`
- Team name click → `team-profile.html`
- Post card → `post-detail.html`
- `lg-name` (match list header) → `league-page.html?league=X`
- `lg-name` (predictions) → `league-page.html?league=X`

### 6. Data Flow Pattern
```
fetchData(name) → data/{name}.json
  ↓
init{Page}Page()  — parallel Promise.all
  ↓
render{Component}() — populate DOM containers
  ↓
applyLeagueParam() (home only)
```

### 7. Shared Assets
- `common.js` — 11 functions (toggleCountry, selectLeague, applyLeagueParam, filterLeagues, toggleLg, setTab, setTabBar, setDate, setRightTab, setDetailTab, updateLivePill)
- `render.js` — 9 init + 10 render functions
- `common.css` — design tokens, 3-col layout, match row, sidebar, posts
- `admin-common.css` — admin layout only

### 8. Phần còn thiếu / cần review
- ❌ `admin-team.html` — quản lý đội bóng (CRUD)
- ❌ `admin-players.html` — quản lý cầu thủ (CRUD)
- ⚠️ `news.html` — dùng inline fetch thay vì `initNewsPage()` trong render.js → cần đồng bộ
- ⚠️ `predictions.html` — `toggleLg` gọi `.closest('.lg')` nhưng section dùng class `pred-section` → toggle không hoạt động
- ⚠️ `league-page.html` — sidebar league click vẫn ở lại league-page thay vì về home (cần xem lại flow)
- ⚠️ Search bar chưa đọc `?q=` param thực tế trong search-results.html (hardcoded mock)
- ⚠️ Date bar ở home.html chưa filter match theo ngày thực tế

---

## Cách viết file

- Dùng Markdown với headings, tables, code blocks, ASCII art
- Ngắn gọn — mỗi section fit trong 1 màn hình
- Ghi rõ ✅ / ❌ / ⚠️ để dễ scan review
- Không dùng prose dài — ưu tiên table và list

---

## Files cần đọc khi implement
- `prototype/*.html` — đã khám phá đủ
- `assets/common.js` — đã đọc
- `assets/render.js` — đã đọc
- `FOLLOW.md` — file output (tạo mới)

## Verification
- Mở `FOLLOW.md` trong IDE, đọc lướt 60 giây là hiểu toàn bộ flow
- Mỗi trang trong bảng có đủ: file, layout, data, initializer, trạng thái
- Section "Thiếu" liệt kê ít nhất 2 admin page chưa có + 3 bug/gap logic
