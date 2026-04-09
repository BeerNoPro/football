# Plan: Fix Navigation Links Across All Prototype Pages

## Context
Kiểm tra toàn bộ 15 file HTML prototype (8 public + 7 admin) để đảm bảo người dùng có thể click qua lại giữa các trang. Phát hiện 6 nhóm lỗi navigation.

---

## Vấn đề phát hiện

### PUBLIC PAGES

| # | Vấn đề | File bị ảnh hưởng |
|---|--------|-------------------|
| 1 | **Logo không phải link** — `<div class="logo-wrap">` không thể click để về home | home.html, predictions.html, category-tag.html, team-profile.html, search-results.html, league-page.html, match-detail.html |
| 2 | **Không có đường vào predictions.html** — home.html không có tab "Dự đoán AI" nào link đến predictions.html | home.html |
| 3 | **Search toolbar không dẫn đến search-results.html** — toolbar search center column thiếu `onkeydown` submit | home.html, league-page.html |
| 4 | **Team names không link đến team-profile.html** — click tên đội ở match-detail.html không đi đâu | match-detail.html |
| 5 | **Tags/category không link đến category-tag.html** — related tags trong post-detail.html dùng `href="#"` | post-detail.html |

### ADMIN PAGES

| # | Vấn đề | File bị ảnh hưởng |
|---|--------|-------------------|
| 6 | **admin-job-monitor.html thiếu trong sidebar menu** — 4 admin pages kia không có menu item dẫn đến Job Monitor | admin-dashboard.html, admin-posts.html, admin-predictions.html, admin-categories.html |

---

## Những thứ đang hoạt động tốt (không sửa)
- Admin sidebar: dashboard ↔ posts ↔ predictions ↔ categories ↔ login ✅
- League sidebar items (non-home pages) → league-page.html qua `selectLeague()` trong common.js ✅
- league-page.html → match-detail.html ✅
- match-detail.html → post-detail.html (right sidebar) ✅
- search-results.html → match-detail.html, post-detail.html ✅
- Mock data: `posts.url = 'post-detail.html'`, `matches.detailUrl = 'match-detail.html'` ✅
- admin-login → admin-dashboard, admin-post-editor → admin-posts ✅

---

## Fixes

### Fix 1 — Logo → home.html (7 public files)
Trong mỗi file, thay:
```html
<div class="logo-wrap">
  <div class="logo">...
```
Thành:
```html
<a href="home.html" class="logo-wrap" style="text-decoration:none;">
  <div class="logo">...
```
(Đóng `</div>` → `</a>`)

Files: `predictions.html`, `category-tag.html`, `team-profile.html`, `search-results.html`, `league-page.html`, `match-detail.html`
> home.html cũng sửa thành `<a href="home.html">` vì click logo ở home vẫn hợp lý (reload trang chủ)

### Fix 2 — home.html: thêm tab "Dự đoán AI" vào toolbar
Trong `.match-toolbar`, thêm button/link:
```html
<a class="toolbar-pred-link" href="predictions.html">🤖 Dự đoán AI</a>
```
Hoặc thêm tab thứ 4 vào `.tabs`:
```html
<button class="tab-btn" onclick="location.href='predictions.html'">Dự đoán AI</button>
```

### Fix 3 — Search submit → search-results.html
Trong toolbar search input của `home.html` và `league-page.html`:
```html
<input type="text" placeholder="Tìm trận, đội bóng..."
  onkeydown="if(event.key==='Enter'&&this.value.trim()) location.href='search-results.html?q='+encodeURIComponent(this.value)" />
```

### Fix 4 — Team names → team-profile.html (match-detail.html)
Tìm tên đội trong match header của match-detail.html, wrap với:
```html
<span class="team-name" onclick="location.href='team-profile.html'" style="cursor:pointer;">Man City</span>
```

### Fix 5 — Related tags → category-tag.html (post-detail.html)
Thay `href="#"` trên các related-tag links:
```html
<a class="related-tag" href="category-tag.html">Man City</a>
```

### Fix 6 — Thêm "Job Monitor" vào admin sidebar (4 files)
Trong sidebar của `admin-dashboard.html`, `admin-posts.html`, `admin-predictions.html`, `admin-categories.html`, thêm menu item sau "Danh mục & Tags":
```html
<a class="admin-menu-item" href="admin-job-monitor.html">
  <svg ...></svg> Job Monitor
</a>
```
(Copy icon từ admin-job-monitor.html, class `active` chỉ thêm vào đúng trang hiện tại)

---

## Files cần sửa
1. `prototype/home.html` — logo link, tab/link predictions, search submit
2. `prototype/predictions.html` — logo link
3. `prototype/category-tag.html` — logo link
4. `prototype/team-profile.html` — logo link
5. `prototype/search-results.html` — logo link
6. `prototype/league-page.html` — logo link, search submit
7. `prototype/match-detail.html` — logo link, team name links
8. `prototype/post-detail.html` — related tags links
9. `prototype/admin-dashboard.html` — thêm Job Monitor menu item
10. `prototype/admin-posts.html` — thêm Job Monitor menu item
11. `prototype/admin-predictions.html` — thêm Job Monitor menu item
12. `prototype/admin-categories.html` — thêm Job Monitor menu item

---

## Verification
Sau khi sửa, click test theo flow sau:
1. `home.html` → click logo → vẫn ở home ✓
2. `home.html` → click tab "Dự đoán AI" → `predictions.html` ✓
3. `home.html` → gõ search + Enter → `search-results.html` ✓
4. `home.html` → click trận → `match-detail.html` → click logo → `home.html` ✓
5. `match-detail.html` → click tên đội → `team-profile.html` → click logo → `home.html` ✓
6. `home.html` → click bài viết → `post-detail.html` → click tag → `category-tag.html` → click logo → `home.html` ✓
7. `admin-dashboard.html` → click Job Monitor → `admin-job-monitor.html` → click menu items → các admin pages ✓
