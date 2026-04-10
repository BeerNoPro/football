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

## Files cần sửa (theo thứ tự ưu tiên)

### Pass 1 — Admin sidebar fix (6 files)
1. `admin-dashboard.html` — thêm: Trận đấu, Đội bóng, Cầu thủ, Users; sửa Cài đặt → `admin-settings.html`
2. `admin-posts.html` — thêm: Trận đấu, Đội bóng, Cầu thủ, Users; sửa Cài đặt
3. `admin-predictions.html` — thêm: Trận đấu, Đội bóng, Cầu thủ, Users; sửa Cài đặt
4. `admin-categories.html` — thêm: Trận đấu, Đội bóng, Cầu thủ, Users; sửa Cài đặt
5. `admin-job-monitor.html` — thêm: Trận đấu, Đội bóng, Cầu thủ, Users; sửa Cài đặt
6. `admin-settings.html` — thêm: Đội bóng, Cầu thủ, Users

Reference sidebar (từ `admin-team.html`):
```html
<a href="admin-dashboard.html" class="nav-item">Dashboard</a>
<a href="admin-posts.html" class="nav-item">Bài viết</a>
<a href="admin-predictions.html" class="nav-item">Dự đoán AI</a>
<a href="admin-categories.html" class="nav-item">Categories & Tags</a>
<a href="admin-matches.html" class="nav-item">Trận đấu</a>
<a href="admin-team.html" class="nav-item">Đội bóng</a>
<a href="admin-players.html" class="nav-item">Cầu thủ</a>
<a href="admin-users.html" class="nav-item">Users</a>
<a href="admin-job-monitor.html" class="nav-item">Job Monitor</a>
<a href="admin-settings.html" class="nav-item">Cài đặt</a>
<a href="home.html" class="nav-item">Xem trang web</a>
<a href="admin-login.html" class="nav-item">Logout</a>
```

### Pass 2 — Layout & Public pages (5 files)
7. `search-results.html` — đổi sang 2-col, xóa right sidebar
8. `category-tag.html` — đổi sang 2-col, xóa right sidebar, fix related tag query params
9. `news.html` — thêm user-row + logout vào left sidebar
10. `post-detail.html` — fix related-card clickable, fix sidebar league onclick
11. `team-profile.html` — fix sidebar league items từ `<div>` sang `<a>`

### Pass 3 — Minor onclick fixes (2 files)
12. `predictions.html` — thêm onclick logout button
13. `search-results.html` — thêm onclick user-row (nếu chưa xong ở pass 2)

---

## Verification
Sau khi sửa:
1. Mở từng file bằng Live Server, click qua tất cả nav links kiểm tra redirect đúng
2. Admin pages: kiểm tra sidebar có đủ 12 items, active state đúng trang
3. search-results.html & category-tag.html: kiểm tra layout 2-col (không có right sidebar)
4. post-detail.html: click related card → chuyển trang post-detail
5. news.html: click user-row → admin-dashboard.html; click logout → admin-login.html
