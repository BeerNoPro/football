# Plan: Static HTML Prototypes — All Pages

## Context
Dự án Football Blog cần prototype HTML tĩnh cho toàn bộ luồng trang trước khi tách Blazor component. Hiện chỉ có `home.html` (P1). Cần build thêm 10 trang (P2–P6 public + A1–A6 admin). Để tiết kiệm token và dễ maintain, tách design tokens + shared components ra file chung thay vì inline mỗi file.

---

## File Structure Output

```
FootballBlog.Web/wwwroot/prototype/
  assets/
    common.css          ← design tokens + shared component styles
    common.js           ← shared JS: tab switch, sidebar toggle, search
    admin-common.css    ← admin layout styles (top nav + sidebar 2 cột)
  home.html    ← cập nhật: import từ assets/
  match-detail.html     ← P2
  post-detail.html      ← P3
  league-page.html      ← P4
  search-results.html   ← P5
  category-tag.html     ← P6
  admin-login.html      ← A1
  admin-dashboard.html  ← A2
  admin-posts.html      ← A3
  admin-post-editor.html← A4
  admin-predictions.html← A5
  admin-categories.html ← A6
```

---

## Step 1 — Tạo `assets/common.css`

Extract từ `home.html`:
- CSS custom properties (tất cả `--bg`, `--accent`, `--text`, v.v.)
- Base reset: `* { box-sizing: border-box; margin: 0; padding: 0 }`
- Scrollbar custom styles
- Shared components: `.badge-live`, `.badge-ft`, `.badge-sch`, `.tab-btn`, `.tab-badge`
- Left sidebar: `.logo-wrap`, `.left-search`, `.league-tree`, `.country-group`, `.league-item`, `.left-bottom`
- Right panel: `.right-header`, `.post-featured`, `.post-item`, `.pred-score`, `.ai-badge`
- Match row: `.match-row`, `.mr-time`, `.mr-home`, `.mr-away`, `.score-c`, `.tl`, `.tn`
- `@keyframes livepulse`

## Step 2 — Tạo `assets/common.js`

Extract từ `home.html`:
- `toggleCountry(id)` — expand/collapse sidebar country group
- `selectLeague(el, id)` — highlight league + scroll
- `filterLeagues(q)` — live search sidebar
- `toggleLg(hdr)` — expand/collapse league group in main
- `setTab(el)` — switch tabs (generic, by parent selector)
- `setDate(el)` — switch date bar
- `setRightTab(el)` — switch right panel tabs

## Step 3 — Tạo `assets/admin-common.css`

Layout riêng cho admin (2 cột: sidebar trái + main):
```
┌─────────────────────────────┐
│  Top nav (logo + user menu) │
├──────────┬──────────────────┤
│ Admin    │ Main content     │
│ Sidebar  │ (table/form)     │
│ 220px    │ 1fr              │
└──────────┴──────────────────┘
```
- `.admin-layout`, `.admin-nav`, `.admin-sidebar`, `.admin-main`
- `.admin-menu-item`, `.admin-menu-group`
- `.stat-card`, `.data-table`, `.form-field`, `.btn-primary`, `.btn-danger`

## Step 4 — Cập nhật `home.html`

Thay `<style>` block bằng:
```html
<link rel="stylesheet" href="assets/common.css">
```
Thay inline `<script>` bằng:
```html
<script src="assets/common.js"></script>
```
Giữ lại CSS đặc thù của home (hero banner, 3-col grid, match toolbar) trong file.

## Step 5 — Build Public Pages (P2–P6)

### P2: `match-detail.html`
- Layout: 3 cột (giống home, sidebar trái + phải giữ nguyên)
- Center: match header (logo đội + tỉ số lớn + thời gian), tab bar [Lineups | Events | Stats | Timeline | H2H]
- Tab Lineups: 2 đội song song, danh sách cầu thủ số áo + tên
- Tab Events: timeline dọc (goal, card, sub)
- Tab Stats: progress bar so sánh 2 đội (possession, shots, corners)
- Mock data: Man City vs Arsenal

### P3: `post-detail.html`
- Layout: 2 cột (sidebar trái giữ nguyên, không có right panel)
- Center: article full width — hero image, tiêu đề, meta (ngày/tag), AI prediction box nổi bật (tỉ số dự đoán + confidence %), nội dung bài viết dài, related posts cuối trang

### P4: `league-page.html`
- Layout: 3 cột (giống home)
- Center: league header (logo giải + tên + mùa giải), tab [Fixtures | Results | Table | Top Scorers]
- Tab Fixtures: list match rows grouped by date (dùng lại `.match-row`)
- Tab Table: standings table (rank, team, P/W/D/L/GD/Pts)

### P5: `search-results.html`
- Layout: 3 cột
- Center: search bar lớn ở top + kết quả 2 section: "Trận đấu" (match rows) và "Bài viết" (post cards grid)
- Hiển thị số kết quả, highlight từ khóa tìm kiếm

### P6: `category-tag.html`
- Layout: 3 cột
- Center: tag header (tên tag + số bài), grid bài viết (card: thumbnail + tag + tiêu đề + excerpt + meta)
- Filter bar: sort by (Mới nhất / Nhiều đọc) + pagination

## Step 6 — Build Admin Pages (A1–A6)

### A1: `admin-login.html`
- Full-screen centered form trên nền `--bg`
- Logo + form (email, password, remember me) + login button accent lime
- Link "Quên mật khẩu"

### A2: `admin-dashboard.html`
- Admin layout (top nav + sidebar)
- Stat cards: Tổng bài viết | Draft | Published | AI Predictions hôm nay
- Chart placeholder (bar chart mock bằng CSS)
- Recent posts table (5 dòng), Recent predictions list

### A3: `admin-posts.html`
- Admin layout
- Toolbar: search + filter (All/Draft/Published) + nút "Tạo bài mới"
- Table: Tiêu đề | Danh mục | Trạng thái | Ngày tạo | Actions (Edit/Delete)
- Pagination

### A4: `admin-post-editor.html`
- Admin layout, main area chiếm rộng hơn
- Form: Tiêu đề, Slug (auto-gen), Category (select), Tags (input chips)
- Rich text area mock (toolbar bold/italic/link/image + textarea)
- Sidebar form: Featured image upload, Publish status toggle, Publish date, SEO preview
- Nút Save Draft + Publish

### A5: `admin-predictions.html`
- Admin layout
- Filter: League, Team, Date range, Status (Pending/Published)
- Table: Trận đấu | Dự đoán AI | Confidence | Model | Trạng thái | Actions
- Detail expand row: nội dung AI analysis text

### A6: `admin-categories.html`
- Admin layout, 2 panel ngang
- Panel trái: Category tree (có thể nest), nút Add + Delete
- Panel phải: Form edit (tên, slug, description, parent, màu/icon)
- Tab [Categories | Tags] ở top

---

## Thứ Tự Build

1. `assets/common.css` + `assets/common.js` + `assets/admin-common.css`
2. Cập nhật `home.html` import assets
3. P2 `match-detail.html`
4. P3 `post-detail.html`
5. P4 `league-page.html`
6. P5 `search-results.html`
7. P6 `category-tag.html`
8. A1 `admin-login.html`
9. A2 `admin-dashboard.html`
10. A3 `admin-posts.html`
11. A4 `admin-post-editor.html`
12. A5 `admin-predictions.html`
13. A6 `admin-categories.html`

---

## Verification

- Mở từng file qua Live Server (VSCode) — kiểm tra layout không vỡ
- Verify `home.html` sau khi refactor trông giống hệt bản gốc
- Kiểm tra JS interactions: tab switch, sidebar toggle, search filter hoạt động
- Kiểm tra cross-file: màu sắc, typography, spacing nhất quán giữa các trang
- Click flow thử: Home → Match Detail → back → click bài → Post Detail

---

## Notes

- Mỗi page HTML chỉ cần import `common.css` + `common.js`, sau đó thêm page-specific CSS trong `<style>` block nhỏ
- Admin pages import thêm `admin-common.css`
- Mock data dùng tên thật (Man City, Arsenal, Premier League, v.v.) cho realistic
- Không cần Tailwind CDN nếu không dùng — loại bỏ khỏi các file mới để tránh conflict
