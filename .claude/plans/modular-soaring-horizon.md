# BA/SA Review — FOLLOW.md & Prototype Navigation Analysis

## Context

Yêu cầu: Review toàn bộ 21 trang HTML prototype, đối chiếu với FOLLOW.md, tìm điểm sai/thiếu/thừa về nghiệp vụ và flow màn hình. Đứng vai trò BA + SA để phân tích sâu logic liên kết màn hình.

---

## 1. Những điểm FOLLOW.md ghi SAI hoặc THIẾU

### 1.1 Navigation Diagram — Thiếu nhiều link quan trọng

| Trang | FOLLOW.md ghi | Thực tế còn thiếu |
|-------|-------------|-------------------|
| `home.html` | ✓ đầy đủ | — |
| `news.html` | Chỉ ghi: "Post card → post-detail" | Thiếu: sidebar league → home?league=X; logo → home |
| `league-page.html` | Chỉ ghi: "Match row → match-detail" | Thiếu: team name → team-profile; top scorer → player-profile; sidebar post → post-detail |
| `match-detail.html` | Chỉ ghi: "League tag → league-page" | Thiếu: team name → team-profile; sidebar post → post-detail |
| `post-detail.html` | Chỉ ghi: "Breadcrumb → home" | Thiếu: tag pill → category-tag; inline body link → team-profile; sidebar post → post-detail (self) |
| `team-profile.html` | Ghi player + match | Thiếu: sidebar post → post-detail; sidebar league → home?league=X |
| `player-profile.html` | Chỉ ghi: "Club link → team-profile" | Thiếu: upcoming match → match-detail; sidebar post → post-detail |
| `search-results.html` | Ghi match + post | Thiếu: team result → team-profile? (cần confirm search.json có team không) |
| `category-tag.html` | Ghi: "Post card → post-detail" | Thiếu: related tag click → category-tag (self-referential) |
| `admin-posts.html` | Ghi: "Tạo/Sửa → editor" | Thiếu: "Xem" button → post-detail.html (public page) |
| `admin-dashboard.html` | Ghi sidebar → all admin | Thiếu: "Recent posts" table row → admin-post-editor |

### 1.2 Right Sidebar — Link ẩn trên MỌI trang 3-col

FOLLOW.md **không document** rằng: mọi trang 3-col đều có right sidebar AI Blog, và click post ở đó → `post-detail.html`. Đây là link phổ biến nhất nhưng bị bỏ qua hoàn toàn.

Các trang bị ảnh hưởng: home, news, league-page, match-detail, post-detail, predictions, team-profile, player-profile

### 1.3 URL Parameters — Không được document

FOLLOW.md không có bảng URL params. Thực tế prototype dùng:

| Trang | Param | Ý nghĩa |
|-------|-------|---------|
| `home.html` | `?league=X` | Scroll + highlight league X |
| `league-page.html` | `?league=X` | Hiện detail giải X |
| `predictions.html` | `?league=X` | Filter predictions theo giải |
| `search-results.html` | `?q=` | Query tìm kiếm |
| `category-tag.html` | `?tag=X` / `?cat=X` | Lọc theo tag/category |
| `team-profile.html` | `?team=X` | Profile đội X |
| `match-detail.html` | `?match=X` | Chi tiết trận X |
| `player-profile.html` | `?player=X` | Profile cầu thủ X |
| `post-detail.html` | `?slug=X` | Bài viết theo slug |

### 1.4 Trang còn thiếu (đã note trong FOLLOW.md nhưng chưa tạo)

- `admin-team.html` — CRUD đội bóng
- `admin-players.html` — CRUD cầu thủ

---

## 2. Phân tích Gap nghiệp vụ — Góc nhìn BA

### 2.1 Core Flow: AI Prediction → Blog Post (CHƯA DOCUMENT)

Đây là flow nghiệp vụ quan trọng nhất của hệ thống nhưng FOLLOW.md không vẽ flow end-to-end:

```
[Football API] → FetchUpcomingMatchesJob
      ↓
admin-matches.html (trigger manual hoặc cron)
      ↓
GeneratePredictionJob → AI (Claude/Gemini)
      ↓
admin-predictions.html (xem kết quả, approve/reject)
      ↓
PublishPredictionJob → tạo bài viết
      ↓
admin-posts.html (bài mới xuất hiện)
      ↓
post-detail.html (public đọc)
      ↓
predictions.html (hiện prediction card)
```

→ FOLLOW.md cần thêm section "Core Business Flow: Prediction Pipeline"

### 2.2 Admin → Public Preview Flow (THIẾU)

Từ admin-post-editor.html, sau khi tạo draft, admin cần preview bài trên public site. Flow này chưa có:
- `admin-post-editor.html` → "Preview" button → `post-detail.html?slug=X&preview=true`
- Chưa có "Preview" button trong admin-post-editor.html

### 2.3 Auth / Session Flow (THIẾU HOÀN TOÀN)

FOLLOW.md không document:
- Session expired → redirect `admin-login.html` (mọi admin page)
- Role check: chỉ admin mới thấy "User row → admin-dashboard" ở public site
- Ghi nhớ "Quên mật khẩu" flow (có thể chưa cần nhưng cần note)

### 2.4 Live Score Real-time Flow (THIẾU)

home.html có live pill và live badges, nhưng không document:
- Cơ chế update: SignalR push hay polling?
- `LiveScorePollingJob` (30s) push → SignalR hub → client
- home.html cần subscribe hub để auto-update scores

### 2.5 Pagination / Load more (THIẾU)

3 trang có listing nhưng không document pagination:
- `news.html` — scroll infinite hay "Xem thêm"?
- `search-results.html` — bao nhiêu kết quả? phân trang?
- `category-tag.html` — tương tự

---

## 3. Phân tích Gap — Góc nhìn SA (Kỹ thuật)

### 3.1 Pattern Không Đồng Nhất

| Vấn đề | Trang | Ảnh hưởng |
|--------|-------|-----------|
| `news.html` dùng inline fetch thay `initNewsPage()` | news.html | Khi refactor sang Blazor sẽ không có reference function |
| `admin-*` pages không có data JSON — hardcoded mock | Tất cả admin | Blazor component sẽ khó estimate field list cần bind |
| `predictions.html` dùng `pred-section` thay `.lg` | predictions.html | `toggleLg()` không hoạt động → collapse broken |

### 3.2 Sidebar League — selectLeague() Ambiguity

Khi ở trang không phải home (ví dụ: match-detail.html), click league ở sidebar → navigate `home.html?league=X` → load lại trang chủ và highlight league.

Nhưng đây có đúng UX không? Hay nên navigate thẳng `league-page.html?league=X`?

**Gap trong FOLLOW.md**: Không phân biệt hai behavior này. Cần quyết định:
- Option A (hiện tại): Sidebar league → home + scroll (bị lẫn context)  
- Option B (đề xuất): Sidebar league → league-page.html?league=X

### 3.3 Render Functions Inline vs render.js

FOLLOW.md section render.js ghi: "renderLeagueDetail, renderMatchDetail, renderTeam, renderPlayer, renderPredictions, renderCategory, renderSearch — inline trong từng trang"

Điều này không được document rõ → khi tách Blazor component, developer sẽ không biết hàm nào cần tạo mới.

---

## 4. Screen Flow Map — Đầy Đủ (Bản Đề Xuất Cho FOLLOW.md)

```
PUBLIC NAVIGATION MAP
══════════════════════════════════════════════════════

[Mọi trang — LEFT SIDEBAR]
  ├─ Logo                     ──→ home.html
  ├─ Search input (Enter)     ──→ search-results.html?q=
  ├─ League item click        ──→ home.html?league=X  (nếu không ở home)
  │                           ──→ scroll + highlight  (nếu đang ở home)
  └─ User row / Logout        ──→ admin-dashboard / admin-login

[Mọi trang 3-col — RIGHT SIDEBAR]
  └─ Post card click          ──→ post-detail.html?slug=X

[home.html]
  ├─ Tab "Dự đoán AI"         ──→ predictions.html
  ├─ Tab "Tin tức"            ──→ news.html
  ├─ Match row                ──→ match-detail.html?match=X
  ├─ Team name (match row)    ──→ team-profile.html?team=X
  └─ League name (.lg-name)   ──→ league-page.html?league=X

[news.html]
  └─ Post card                ──→ post-detail.html?slug=X

[predictions.html]
  ├─ League name link         ──→ league-page.html?league=X
  └─ "Phân tích →"            ──→ post-detail.html?slug=X

[league-page.html]
  ├─ Match row                ──→ match-detail.html?match=X
  ├─ Team name                ──→ team-profile.html?team=X
  ├─ Top scorer (player link) ──→ player-profile.html?player=X
  └─ Featured post            ──→ post-detail.html?slug=X

[match-detail.html]
  ├─ Breadcrumb league        ──→ league-page.html?league=X
  └─ Team name                ──→ team-profile.html?team=X

[team-profile.html]
  ├─ Player row               ──→ player-profile.html?player=X
  └─ Match row                ──→ match-detail.html?match=X

[player-profile.html]
  ├─ Club link                ──→ team-profile.html?team=X
  └─ Upcoming match           ──→ match-detail.html?match=X

[post-detail.html]
  ├─ Breadcrumb               ──→ home.html  (hoặc league-page nếu có context)
  ├─ Tag pill                 ──→ category-tag.html?tag=X
  └─ Inline body link         ──→ team-profile.html?team=X

[search-results.html]
  ├─ Match result             ──→ match-detail.html?match=X
  ├─ Post result              ──→ post-detail.html?slug=X
  └─ Team result (?)          ──→ team-profile.html?team=X  (cần confirm)

[category-tag.html]
  ├─ Post card                ──→ post-detail.html?slug=X
  └─ Related tag              ──→ category-tag.html?tag=Y  (self)

[404.html]
  └─ Quick links              ──→ home / predictions / news / league / team / search / category


ADMIN NAVIGATION MAP
══════════════════════════════════════════════════════

[Mọi admin page — SIDEBAR]
  ├─ Dashboard                ──→ admin-dashboard.html
  ├─ Bài viết                 ──→ admin-posts.html
  ├─ Dự đoán AI               ──→ admin-predictions.html
  ├─ Categories               ──→ admin-categories.html
  ├─ Trận đấu                 ──→ admin-matches.html
  ├─ Users                    ──→ admin-users.html
  ├─ Job Monitor              ──→ admin-job-monitor.html
  ├─ Cài đặt                  ──→ admin-settings.html
  ├─ "Xem trang web"          ──→ home.html
  └─ Logout                   ──→ admin-login.html

[admin-login.html]
  └─ Form submit (success)    ──→ admin-dashboard.html

[admin-dashboard.html]
  ├─ "Tạo bài viết"           ──→ admin-post-editor.html (mode: create)
  └─ Recent post row          ──→ admin-post-editor.html (mode: edit, ?id=X)

[admin-posts.html]
  ├─ "Tạo bài"                ──→ admin-post-editor.html (mode: create)
  ├─ Edit action              ──→ admin-post-editor.html (mode: edit, ?id=X)
  └─ View action              ──→ post-detail.html?slug=X  (public page)

[admin-post-editor.html]
  ├─ "Đăng" / "Lưu"          ──→ admin-posts.html
  ├─ Back / Breadcrumb        ──→ admin-posts.html
  └─ "Preview" (CẦN THÊM)    ──→ post-detail.html?slug=X&preview=true

[admin-predictions.html]
  ├─ "Tạo bài từ prediction"  ──→ admin-post-editor.html (mode: create, ?predId=X)
  └─ "Xem bài"                ──→ post-detail.html?slug=X

[admin-matches.html]
  ├─ "Xem trận"               ──→ match-detail.html?match=X
  └─ "Xem bài"                ──→ post-detail.html?slug=X
```

---

## 5. Danh sách việc cần làm với FOLLOW.md

### A. Cập nhật Navigation Diagram (bắt buộc)
- [ ] Thay sơ đồ điều hướng hiện tại bằng Screen Flow Map đầy đủ ở trên
- [ ] Thêm note "Right Sidebar present on all 3-col pages → post-detail"
- [ ] Thêm note "Left Sidebar present on all pages → home / search"

### B. Thêm section URL Parameters
- [ ] Bảng URL params cho mỗi trang

### C. Thêm section Core Business Flow
- [ ] Prediction Pipeline (Football API → AI → Blog Post → Public)
- [ ] Live Score Real-time flow (SignalR)

### D. Sửa các bug đã biết (section ⚠️)
- [ ] Xác nhận fix `predictions.html` collapse dùng `.pred-section` thay `.lg`
- [ ] Thêm `initNewsPage()` vào render.js để đồng bộ pattern
- [ ] `search-results.html` đọc `?q=` param khi load

### E. Tạo 2 trang admin còn thiếu
- [ ] `admin-team.html`
- [ ] `admin-players.html`

### F. Thêm feature UX còn thiếu trong prototype
- [ ] "Preview" button trong admin-post-editor.html
- [ ] Pagination hoặc "Xem thêm" trong news / search / category
- [ ] Quyết định sidebar league → home hay league-page (Option A vs B)

---

## Quyết định đã confirm với user

| # | Câu hỏi | Quyết định |
|---|---------|-----------|
| 1 | Sidebar league click từ trang khác | **Giữ nguyên**: về home.html?league=X + scroll |
| 2 | Pagination | **Phân trang số** (1 2 3...) — URL riêng mỗi trang, tốt cho SEO |
| 3 | admin-team + admin-players | **Tạo ngay** — cần hoàn chỉnh prototype trước khi tách Blazor |

---

## Kế hoạch thực hiện (theo thứ tự ưu tiên)

### Task 1 — Cập nhật FOLLOW.md
- Thêm Screen Flow Map đầy đủ (bản đề xuất ở section 4)
- Thêm bảng URL Parameters
- Thêm section Core Business Flow (Prediction Pipeline)
- Thêm note right sidebar present on all 3-col pages
- Sửa lại sơ đồ post-detail, news, league-page cho đúng

### Task 2 — Fix bugs prototype đã biết
- `predictions.html`: sửa `toggleLg()` dùng selector đúng `.pred-section` 
- `news.html`: thêm `initNewsPage()` vào render.js, refactor inline fetch
- `search-results.html`: đọc `?q=` param khi load page

### Task 3 — Tạo 2 trang admin còn thiếu
- `admin-team.html` — CRUD đội bóng (tên, logo, quốc gia, giải đấu, cầu thủ)
- `admin-players.html` — CRUD cầu thủ (tên, số áo, vị trí, đội, stats)
- Copy admin sidebar + header từ `admin-matches.html` làm base

### Task 4 — Thêm tính năng còn thiếu vào prototype
- Pagination component (shared) trong news / search / category (số trang 1 2 3...)
- "Preview" button trong `admin-post-editor.html` → mở `post-detail.html?preview=true`

### Task 5 — Tạo data JSON còn thiếu (nếu cần)
- Xác nhận `search.json` có team entries không
- Thêm mock data cho admin pages nếu cần test bind Blazor
