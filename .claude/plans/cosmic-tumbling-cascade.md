# Plan: Kiểm tra & Sửa toàn bộ HTML Prototype theo FOLLOW.md

## Context
FOLLOW.md định nghĩa 23 trang HTML prototype với navigation map, interaction catalog, layout rules, và shared JS function catalog. Sau khi so sánh spec vs thực tế, phát hiện nhiều lỗi: admin sidebar không đồng nhất, layout sai, link thiếu onclick, user row không click được.

---

## Issues phát hiện

### 🔴 Critical — Admin Sidebar không đồng nhất

Spec yêu cầu MỌI trang admin có đủ 12 nav items. Thực tế:

| File | Thiếu nav items | Cài đặt link |
|------|----------------|--------------|
| `admin-dashboard.html` | Trận đấu, Đội bóng, Cầu thủ, Users | `href="#"` BROKEN |
| `admin-posts.html` | Trận đấu, Đội bóng, Cầu thủ, Users | `href="#"` BROKEN |
| `admin-predictions.html` | Trận đấu, Đội bóng, Cầu thủ, Users | `href="#"` BROKEN |
| `admin-categories.html` | Trận đấu, Đội bóng, Cầu thủ, Users | `href="#"` BROKEN |
| `admin-job-monitor.html` | Trận đấu, Đội bóng, Cầu thủ, Users | `href="#"` BROKEN |
| `admin-settings.html` | Đội bóng, Cầu thủ, Users | OK |

Pages đúng chuẩn (dùng làm reference): `admin-team.html`, `admin-players.html`, `admin-matches.html`, `admin-users.html`

Fix: Copy sidebar block từ `admin-team.html` (sidebar đầy đủ) vào 6 trang trên, giữ nguyên active state của từng trang.

---

### 🔴 Critical — Layout sai (2-col vs 3-col)

| File | Spec | Thực tế |
|------|------|---------|
| `search-results.html` | 2-col (`.app-2col`) | 3-col (có right sidebar) |
| `category-tag.html` | 2-col (`.app-2col`) | 3-col (có right sidebar) |

Fix: Xóa `<aside class="right-sidebar">` block, đổi class `.app` → `.app-2col`.

---

### 🟠 High — User row / Logout thiếu hoặc không clickable

| File | Issue |
|------|-------|
| `news.html` | Thiếu hoàn toàn user-row và logout button trong left sidebar |
| `search-results.html` | User row có nhưng KHÔNG có `onclick` → admin-dashboard.html |
| `category-tag.html` | User row có nhưng KHÔNG có `onclick` → admin-dashboard.html |
| `predictions.html` | Logout button có nhưng KHÔNG có onclick handler |

Spec: User row click → `admin-dashboard.html`, Logout → `admin-login.html`

Fix: Thêm `onclick="location.href='admin-dashboard.html'"` vào user-row, `onclick="event.stopPropagation();location.href='admin-login.html'"` vào logout.

---

### 🟠 High — Related posts không clickable (post-detail.html)

`.related-card` elements là plain `<div>` không có onclick hay href. Spec yêu cầu click → `post-detail.html?slug=X`.

Fix: Wrap each `.related-card` bằng `<a href="post-detail.html">` hoặc thêm `onclick="location.href='post-detail.html'"`.

---

### 🟡 Medium — team-profile.html sidebar dùng `<div>` thay `<a>`

League items trong left sidebar của `team-profile.html` dùng `<div class="league-item" onclick="selectLeague()">` thay vì `<a href="league-page.html" onclick="selectLeague()">` như các trang khác. Không navigate được khi JS disabled hoặc khi ở trang khác.

Fix: Đổi thành `<a class="league-item" href="league-page.html?league={id}" onclick="...selectLeague()">`.

---

### 🟡 Medium — category-tag.html related tags thiếu query param

`<a class="related-tag" href="category-tag.html">` không có `?tag=slug`. Spec: `category-tag.html?tag=Y`.

Fix: Thêm `?tag={slug}` vào mỗi related tag link.

---

### 🟡 Medium — post-detail.html sidebar thiếu onclick trên league items

League items trong sidebar của `post-detail.html` thiếu `onclick="selectLeague(this, 'id')"`. Các trang khác đều có handler này.

Fix: Thêm onclick handlers cho league items trong sidebar.

---

## Trạng thái thực hiện

### ✅ Pass 1 — Admin sidebar (8 files DONE)
- `admin-dashboard.html`, `admin-posts.html`, `admin-predictions.html`, `admin-categories.html` — thêm Bóng đá group, Users, fix Cài đặt href
- `admin-matches.html`, `admin-settings.html` — thêm Đội bóng, Cầu thủ, Users
- `admin-job-monitor.html` — viết lại toàn bộ sidebar chuẩn structure
- `admin-users.html` — thêm Bóng đá group, fix Users label + SVG, bỏ Trận đấu khỏi Hệ thống

### ✅ Pass 2a — Layout (2 files DONE)
- `search-results.html` — đổi sang 2-col, xóa right sidebar, fix user-row + logout onclick
- `category-tag.html` — đổi sang 2-col, xóa right sidebar, fix related tag ?tag=slug, fix user-row onclick

### ✅ Pass 2b — Public pages (3 files DONE)
- `news.html` — thêm left-bottom block (settings-btn + user-row + logout)
- `post-detail.html` — thêm onclick vào tất cả related-card
- `team-profile.html` — đổi league-item từ `<div>` → `<a href="league-page.html?league=X">`, fix user-row onclick

### ⏳ Pass 3 — Còn lại (2 file)
1. `predictions.html` — thêm:
   - `onclick="location.href='admin-dashboard.html'"` vào `<div class="user-row">`
   - `onclick="event.stopPropagation(); location.href='admin-login.html'"` vào `<button class="logout-btn">`
2. `post-detail.html` — fix sidebar league-item hrefs: `href="league-page.html"` → `href="league-page.html?league=X"` (hiện thiếu query param)

---

## Verification
Sau khi sửa:
1. Mở từng file bằng Live Server, click qua tất cả nav links kiểm tra redirect đúng
2. Admin pages: kiểm tra sidebar có đủ 12 items, active state đúng trang
3. search-results.html & category-tag.html: kiểm tra layout 2-col (không có right sidebar)
4. post-detail.html: click related card → chuyển trang post-detail
5. news.html: click user-row → admin-dashboard.html; click logout → admin-login.html
