# FOLLOW.md — Bản đồ Prototype FootballBlog

> Dùng file này để hiểu toàn bộ flow prototype trước khi code.  
> Cập nhật khi thêm trang mới hoặc thay đổi logic điều hướng.

---

## Tổng quan

| Thống kê | Giá trị |
|----------|---------|
| Tổng trang | 24 HTML |
| Public pages | 11 (bao gồm 404) |
| Admin pages | 13 |
| Data JSON | 12 file (`ref/` × 2 + `pages/` × 10) |
| Shared JS | common.js + render.js |
| Shared CSS | common.css + admin-common.css |
| Layout chính | 3-col (240px sidebar + 1fr + 300px) |
| Layout phụ | 2-col (240px sidebar + 1fr), standalone |
| Theme | Dark — accent `#c8f04d` (lime) |

---

## TODO / Known Issues

### [Bug] predictions.html — user-row và logout chưa có onclick

- `<div class="user-row">` thiếu `onclick="location.href='admin-dashboard.html'"`
- `<button class="logout-btn">` thiếu `onclick="event.stopPropagation(); location.href='admin-login.html'"`

### [Bug] post-detail.html — sidebar league-item thiếu `?league=X`

Các `<a class="league-item" href="league-page.html">` trong sidebar của post-detail.html thiếu query param — cần thêm `?league={id}` để đúng với navigation spec.

---

## Mock Data — Cấu trúc JSON cho Prototype

> **Quy tắc:** Mọi trang HTML đều được render từ JSON thông qua `render.js`.  
> Không có dữ liệu hardcode trong HTML (trừ admin pages — UI-only mock).

### Cấu trúc thư mục

```
prototype/data/
├── ref/                        ← "dictionary" — load 1 lần, dùng cho mọi trang
│   ├── leagues.json            ← sidebar tree: country → leagues (liveCount badge)
│   └── taxonomy.json           ← categories[] + tags[] (không có recentPosts)
│
└── pages/                      ← 1 file = 1 API endpoint = 1 page contract
    ├── home.json               ← matchDay (byLeague[]) + posts (featured + items[])
    ├── news.json               ← paginated PostRef[] với slug/excerpt/tags/readTimeMinutes
    ├── post-detail.json        ← post content + relatedMatch + prediction + relatedPosts[3]
    ├── category-detail.json    ← category info + paginated PostRef[]
    ├── league-detail.json      ← standings[] + topScorers[] + recentMatches[]
    ├── match-detail.json       ← lineups + stats + events (có playerId) + commentary
    ├── team-detail.json        ← squad[] + fixtures[] (MatchRef chuẩn)
    ├── player-detail.json      ← seasonStats + careerHistory + upcomingMatches[]
    ├── predictions.json        ← accuracy block + filters + items[] (TeamRef chuẩn)
    └── search.json             ← matches[] + posts[] + teams[] + players[] + suggestions[]
```

### Canonical Reference DTOs

Mọi entity khi nhúng vào file khác phải dùng đúng shape này — không tự định nghĩa sub-object:

| DTO | Fields |
|-----|--------|
| `TeamRef` | `{ id, name, shortName, logo, profileUrl }` |
| `PlayerRef` | `{ id, name, number, position, nationality, teamId, teamName, teamLogo, profileUrl }` |
| `MatchRef` | `{ id, kickoff, status, elapsed, homeTeam: TeamRef, awayTeam: TeamRef, score, leagueId, round, detailUrl }` |
| `PostRef` | `{ id, title, slug, thumbGradient, thumbEmoji, tag, excerpt, publishedAt, updatedAt, url }` |
| `PredictionSnippet` | `{ postUrl, aiScore, confidence, confidenceLevel, summary }` |

### Cách render.js load data

```
Mọi page init:   fetchData('ref/leagues')       → renderLeagueTree()  [sidebar]
                 fetchData('pages/<page>')       → render<Page>()      [main content]

Home riêng:      fetchData('pages/home')         → home.matchDay       [matches center]
                                                 → home.posts          [right panel]
```

**Khi chuyển sang API thật:** chỉ cần thay `fetchData()` trỏ về `/api/` — mọi `render*()` function giữ nguyên.

---

## PostType — 3 Loại Bài Viết

Mỗi bài viết (`Post`) thuộc 1 trong 3 loại, gắn với vòng đời của trận đấu.  
Right sidebar tabs ở home.html (`Nhận định / Dự đoán / Phân tích`) filter theo `PostType`.

| PostType | Thời điểm sinh | Match.Status | Prompt Template | Badge hiển thị |
|----------|----------------|--------------|-----------------|----------------|
| `DuDoan` (Dự đoán) | ~24h trước kickoff | `SCH` (scheduled) | "Dự đoán tỷ số trận đấu" | `🤖 AI · Dự đoán` |
| `NhanDinh` (Nhận định) | Trong lúc thi đấu | `LIVE` | "Nhận định trực tiếp" | `🤖 AI · Nhận định LIVE` |
| `PhanTich` (Phân tích) | Sau khi kết thúc | `FT` | "Phân tích sau trận" | `🤖 AI · Phân tích` |

**Quy tắc sinh bài tự động:**
- `DuDoan` → `GeneratePredictionJob` chạy cron 1h, query Match chưa có bài DuDoan
- `NhanDinh` → `LiveScorePollingJob` trigger khi match chuyển sang LIVE (nếu chưa có)
- `PhanTich` → `GeneratePredictionJob` trigger khi match chuyển sang FT (nếu chưa có)

**Mỗi PostType dùng 1 Prompt Template riêng** — lưu trong DB, quản lý qua `admin-prompts.html`.  
Admin có thể update prompt bất kỳ lúc nào mà không cần deploy lại code.

---

## Core Business Flow: Content Pipeline

Đây là flow nghiệp vụ chính của hệ thống — từ dữ liệu trận đấu tới bài viết public:

```
[Football API] ─── FetchUpcomingMatchesJob (cron 6h)
       ↓
 admin-matches.html  ◄── xem danh sách trận sắp diễn ra
       ↓
 GeneratePredictionJob (cron 1h)
   ├── Match SCH + chưa có bài DuDoan  → AI tạo bài "Dự đoán" (PostType.DuDoan)
   ├── Match LIVE + chưa có bài NhanDinh → AI tạo bài "Nhận định" (PostType.NhanDinh)
   └── Match FT + chưa có bài PhanTich  → AI tạo bài "Phân tích" (PostType.PhanTich)
       ↓
 admin-predictions.html  ◄── xem kết quả AI, approve / reject / chỉnh sửa
       ↓
 PublishPredictionJob ─── tạo bài viết tự động + gửi Telegram
       ↓
 admin-posts.html  ◄── bài mới xuất hiện, có thể edit thêm
       ↓
 post-detail.html  ◄── public đọc bài
       │
       ├── home.html right panel tab "Dự đoán" ← PostType.DuDoan
       ├── home.html right panel tab "Nhận định" ← PostType.NhanDinh (LIVE)
       └── home.html right panel tab "Phân tích" ← PostType.PhanTich
```

**Live Score Real-time:**
```
LiveScorePollingJob (30s, chỉ khi có live match)
       ↓
 SignalR Hub ─── push event to clients
       ↓
 home.html: cập nhật score + live badge tự động (không reload)
```

---

## Screen Flow Map — Public Site

> **Quy tắc chung (áp dụng MỌI trang):**
> - **LEFT SIDEBAR** luôn có: Logo → home | Search → search-results?q= | League click → home?league=X (scroll) | User row → admin-dashboard | Logout → admin-login
> - **RIGHT SIDEBAR** (chỉ trang 3-col) luôn có: Post card click → post-detail?slug=X

```
PUBLIC NAVIGATION MAP
══════════════════════════════════════════════════════════════

[Mọi trang — LEFT SIDEBAR]
  ├─ Logo                     ──→ home.html
  ├─ Search input (Enter)     ──→ search-results.html?q=
  ├─ League item click        ──→ home.html?league=X + applyLeagueParam() scroll
  └─ User row / Logout        ──→ admin-dashboard.html / admin-login.html

[Mọi trang 3-col — RIGHT SIDEBAR]
  └─ Post card click          ──→ post-detail.html?slug=X

──────────────────────────────────────────────────────────────

[home.html]
  ├─ Tab "Dự đoán AI"         ──→ predictions.html
  ├─ Tab "Tin tức"            ──→ news.html
  ├─ Match row click          ──→ match-detail.html?match=X
  ├─ Team name (match row)    ──→ team-profile.html?team=X
  └─ League name (.lg-name)   ──→ league-page.html?league=X

[news.html]
  └─ Post card click          ──→ post-detail.html?slug=X

[predictions.html]
  ├─ League name link         ──→ league-page.html?league=X
  └─ "Phân tích →"            ──→ post-detail.html?slug=X

[league-page.html]
  ├─ Match row click          ──→ match-detail.html?match=X
  ├─ Team name                ──→ team-profile.html?team=X
  ├─ Top scorer (player link) ──→ player-profile.html?player=X
  └─ Featured / sidebar post  ──→ post-detail.html?slug=X

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
  ├─ Breadcrumb               ──→ home.html
  ├─ Tag pill                 ──→ category-tag.html?tag=X
  └─ Inline body link         ──→ team-profile.html?team=X

[search-results.html]
  ├─ Match result             ──→ match-detail.html?match=X
  ├─ Post result              ──→ post-detail.html?slug=X
  └─ Team result              ──→ team-profile.html?team=X

[category-tag.html]
  ├─ Post card                ──→ post-detail.html?slug=X
  └─ Related tag click        ──→ category-tag.html?tag=Y  (self)

[404.html]
  └─ Quick links              ──→ home / predictions / news / league / team / search / category
```

---

## Screen Flow Map — Admin Site

```
ADMIN NAVIGATION MAP
══════════════════════════════════════════════════════════════

[Mọi admin page — SIDEBAR]
  ├─ Dashboard                ──→ admin-dashboard.html
  ├─ Bài viết                 ──→ admin-posts.html
  ├─ Dự đoán AI               ──→ admin-predictions.html
  ├─ Categories & Tags        ──→ admin-categories.html
  ├─ Trận đấu                 ──→ admin-matches.html
  ├─ Đội bóng                 ──→ admin-team.html
  ├─ Cầu thủ                  ──→ admin-players.html
  ├─ Users                    ──→ admin-users.html
  ├─ Job Monitor              ──→ admin-job-monitor.html
  ├─ Cài đặt                  ──→ admin-settings.html
  ├─ "Xem trang web"          ──→ home.html
  └─ Logout                   ──→ admin-login.html

──────────────────────────────────────────────────────────────

[admin-login.html]
  └─ Form submit (success)    ──→ admin-dashboard.html

[admin-dashboard.html]
  ├─ "Tạo bài viết"           ──→ admin-post-editor.html (mode: create)
  └─ Recent post row click    ──→ admin-post-editor.html?id=X (mode: edit)

[admin-posts.html]
  ├─ "Tạo bài"                ──→ admin-post-editor.html (mode: create)
  ├─ Edit action (✏️)         ──→ admin-post-editor.html?id=X (mode: edit)
  └─ View action (👁)         ──→ post-detail.html?slug=X  (public page, new tab)

[admin-post-editor.html]
  ├─ "Đăng" / "Lưu nháp"     ──→ admin-posts.html
  ├─ Back / Breadcrumb        ──→ admin-posts.html
  └─ "Preview"                ──→ post-detail.html?slug=X&preview=true  ⚠️ cần thêm button

[admin-predictions.html]
  ├─ "Tạo bài từ prediction"  ──→ admin-post-editor.html?predId=X (mode: create)
  └─ "Xem bài"                ──→ post-detail.html?slug=X

[admin-matches.html]
  ├─ "Xem trận" (👁)          ──→ match-detail.html?match=X
  └─ "Xem bài" (📄)           ──→ post-detail.html?slug=X

[admin-team.html]
  └─ Xem squad                ──→ admin-players.html?team=X

[admin-players.html]
  └─ Lọc theo đội             ──→ admin-players.html?team=X (self filter)
```

---

## Interaction Catalog — Hành vi chi tiết từng click

> **Ký hiệu:**
> - `→ navigate` — chuyển trang (location.href)
> - `→ navigate (tab)` — mở tab mới (window.open)
> - `→ toggle UI` — thay đổi trạng thái DOM, không chuyển trang
> - `→ mock` — chỉ có trong prototype (alert/console), Blazor sẽ gọi API thật
> - `JS: fn()` — hàm JavaScript thực thi

---

### UNIVERSAL — Left Sidebar (mọi trang)

| Element | Action | Kết quả |
|---------|--------|---------|
| Logo click | `→ navigate` | `home.html` |
| Search input (typing) | `→ toggle UI` · `JS: filterLeagues(q)` | Ẩn/hiện `.league-item` và `.country-group` theo từ khóa. Tự expand group đang collapse |
| Search input (Enter) | `→ navigate` | `search-results.html?q={value}` |
| Country group header click | `→ toggle UI` · `JS: toggleCountry(id)` | Toggle class `.collapsed` trên `.country-group` → ẩn/hiện `.sub-leagues` |
| League item click (đang ở home.html) | `→ toggle UI` · `JS: selectLeague(el, id)` | Thêm `.active` vào `.league-item` được click. `scrollIntoView` tới `#m-{leagueId}` trong match list |
| League item click (trang khác) | `→ navigate` · `JS: selectLeague(el, id)` | `home.html?league={id}` → sau khi load `applyLeagueParam()` tự scroll |
| User avatar/row click | `→ navigate` | `admin-dashboard.html` |
| Logout button click | `→ navigate` | `admin-login.html` |

---

### UNIVERSAL — Right Sidebar (chỉ trang 3-col)

Tab right sidebar filter bài viết theo `PostType` — dispatch `rightTabChange` event → `initHomePage` re-render:

| Tab | PostType filter | Match.Status | Kết quả hiển thị |
|-----|----------------|--------------|-----------------|
| **Nhận định** (default) | `NhanDinh` | `LIVE` | Featured post + bài đang diễn ra |
| **Dự đoán** | `DuDoan` | `SCH` | Bài dự đoán trận chưa đấu |
| **Phân tích** | `PhanTich` | `FT` | Bài phân tích trận đã kết thúc |

| Element | Action | Kết quả |
|---------|--------|---------|
| Tab "Nhận định" / "Dự đoán" / "Phân tích" | `→ toggle UI` · `JS: setRightTab(el)` | Active tab + re-render `.right-scroll` filter theo PostType |
| Featured post click | `→ navigate` | `post-detail.html?slug={slug}` |
| Post item click | `→ navigate` | `post-detail.html?slug={slug}` |

---

### home.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Tab "Lịch thi đấu" | `→ toggle UI` · `JS: setTab(el)` | Active tab, ở nguyên trang (default tab) |
| Tab "Dự đoán AI" | `→ navigate` | `predictions.html` |
| Tab "Tin tức" | `→ navigate` | `news.html` |
| Date bar button click | `→ toggle UI` · `JS: setDate(el)` | Active date button — **prototype không filter trận theo ngày**, chỉ visual |
| Date nav `‹` / `›` | `→ toggle UI` | Scroll `.date-bar` trái/phải |
| Live pill badge | `→ toggle UI` · `JS: setTab(el "Live")` | Switch tab về Live (nếu có tab riêng) |
| League group header (`.lg-header`) click | `→ toggle UI` · `JS: toggleLg(this)` | Toggle `.collapsed` trên `.lg` → ẩn/hiện `.lg-matches` |
| League name link (`.lg-name-link`) click | `→ navigate` | `league-page.html?league={id}` · event.stopPropagation() để không trigger toggleLg |
| Match row click | `→ navigate` | `match-detail.html?match={matchId}` |
| Team name click (trong match row) | `→ navigate` | `team-profile.html?team={teamSlug}` |
| Filter button | `→ toggle UI` · mock | Dropdown filter — **prototype chỉ visual**, không filter thật |
| Match search bar | `→ toggle UI` · mock | Prototype không filter trận — Blazor sẽ filter |

---

### news.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Filter pill click (Tất cả / Bóng đá / AI...) | `→ toggle UI` · `JS: filterNews(el, cat)` | Active pill — **prototype chỉ visual**, không filter bài |
| Post card click (featured / grid / list) | `→ navigate` | `post-detail.html?slug={slug}` |
| Pagination số click | `→ toggle UI` · `JS: changePage(el)` | Active page button — **prototype không load trang mới**, Blazor sẽ phân trang thật |

---

### predictions.html

| Element | Action | Kết quả |
|---------|--------|---------|
| League section header (`.pred-section-header`) click | `→ toggle UI` · `JS: toggleLg(this)` | Toggle `.collapsed` trên `.pred-section` → ẩn/hiện `.lg-matches` + xoay `.lg-chevron` |
| League name link trong header | `→ navigate` · `event.stopPropagation()` | `league-page.html?league={id}` — không trigger collapse |
| "Phân tích →" link trên pred-card | `→ navigate` | `post-detail.html?slug={slug}` |
| Confidence bar / tỷ lệ dự đoán | `→ không có action` | Visual only |

---

### league-page.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Tab "Lịch thi đấu" / "BXH" / "Top Scorer" | `→ toggle UI` · `JS: setDetailTab(el, 'league')` | Switch panel content, active tab |
| Date bar | `→ toggle UI` · `JS: setDate(el)` | Visual only trong prototype |
| League group header click | `→ toggle UI` · `JS: toggleLg(this)` | Collapse/expand fixture group |
| League name link | `→ navigate` | Đây là trang league rồi — link này tự navigate chính trang (không có tác dụng) |
| Match row click | `→ navigate` | `match-detail.html?match={matchId}` |
| Team name click (BXH / match row) | `→ navigate` | `team-profile.html?team={teamSlug}` |
| Top scorer — player name click | `→ navigate` | `player-profile.html?player={playerSlug}` |

---

### match-detail.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Breadcrumb — giải đấu click | `→ navigate` | `league-page.html?league={id}` |
| Breadcrumb — home click | `→ navigate` | `home.html` |
| Team name click (header trận) | `→ navigate` | `team-profile.html?team={teamSlug}` |
| Tabs (Đội hình / Thống kê / Timeline / Bình luận) | `→ toggle UI` · `JS: setDetailTab(el, 'match')` | Switch panel + active tab |
| Cầu thủ trong lineup click | `→ navigate` · mock | `player-profile.html?player={slug}` — **prototype có thể chưa link, Blazor cần thêm** |

---

### team-profile.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Tab (Đội hình / Kết quả / Lịch thi đấu / Tin tức) | `→ toggle UI` · `JS: setTab(el)` | Switch tab content |
| Player row click | `→ navigate` | `player-profile.html?player={playerSlug}` |
| Match row click (lịch / kết quả) | `→ navigate` | `match-detail.html?match={matchId}` |
| League badge trên match row | `→ navigate` | `league-page.html?league={id}` |

---

### player-profile.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Club logo / tên đội click | `→ navigate` | `team-profile.html?team={teamSlug}` |
| Tab (Thống kê / Sự nghiệp / Tin tức) | `→ toggle UI` · `JS: setTab(el)` | Switch tab content |
| Upcoming match row click | `→ navigate` | `match-detail.html?match={matchId}` |

---

### post-detail.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Breadcrumb click | `→ navigate` | `home.html` |
| Tag pill click | `→ navigate` | `category-tag.html?tag={slug}` |
| Inline body link (tên đội) click | `→ navigate` | `team-profile.html?team={teamSlug}` |
| Share button | `→ mock` | Prototype: không có action — Blazor: copy URL / Web Share API |
| Related posts click | `→ navigate` | `post-detail.html?slug={slug}` |

---

### search-results.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Search bar (Enter / Submit) | `→ navigate` | Reload `search-results.html?q={newValue}` |
| Clear button (✕) | `→ toggle UI` | Xóa input value |
| Filter tab (Tất cả / Trận đấu / Bài viết / Đội bóng) | `→ toggle UI` · `JS: setTabBar(el)` | Switch result type |
| Match result card click | `→ navigate` | `match-detail.html?match={matchId}` |
| Post result card click | `→ navigate` | `post-detail.html?slug={slug}` |
| Team result card click | `→ navigate` | `team-profile.html?team={teamSlug}` |
| Pagination số click | `→ toggle UI` · `JS: changePage(el)` | Active page — **prototype visual only** |
| Suggestion chip click | `→ navigate` | Reload với query mới |

---

### category-tag.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Tab (Tất cả / Bóng đá / Dự đoán AI...) | `→ toggle UI` · `JS: setTab(el)` | Filter post grid theo category |
| Post card click | `→ navigate` | `post-detail.html?slug={slug}` |
| Related tag pill click | `→ navigate` | `category-tag.html?tag={slug}` (self, khác tag) |
| Pagination số click | `→ toggle UI` · `JS: changePage(el)` | Visual only trong prototype |

---

### admin-login.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Form submit (username + password) | `→ navigate` · mock | `admin-dashboard.html` — prototype không validate |
| "Quên mật khẩu" link | `→ mock` | Chưa implement |

---

### admin-dashboard.html

| Element | Action | Kết quả |
|---------|--------|---------|
| "Tạo bài viết" button | `→ navigate` | `admin-post-editor.html` (mode: create, không có ?id) |
| Recent post row click | `→ navigate` | `admin-post-editor.html?id={postId}` (mode: edit) |
| Stats card click | `→ navigate` | Trang tương ứng (posts / predictions / matches) |
| "Xem trang web" | `→ navigate` | `home.html` |

---

### admin-posts.html

| Element | Action | Kết quả |
|---------|--------|---------|
| "Tạo bài" button | `→ navigate` | `admin-post-editor.html` (mode: create) |
| Search / filter bar | `→ toggle UI` · mock | Visual filter prototype |
| Edit action (✏️) | `→ navigate` | `admin-post-editor.html?id={postId}` |
| View action (👁) | `→ navigate (tab)` | `post-detail.html?slug={slug}` — mở tab mới |
| Delete action (🗑) | `→ mock` | Prototype: alert xác nhận — Blazor: DELETE API |
| Pagination | `→ toggle UI` · mock | Visual only |

---

### admin-post-editor.html

| Element | Action | Kết quả |
|---------|--------|---------|
| "← Quay lại" button | `→ navigate` | `admin-posts.html` |
| "Lưu nháp" button | `→ navigate` · mock | `admin-posts.html` — Blazor sẽ PATCH status=draft |
| "Preview" button | `→ navigate (tab)` | `post-detail.html?preview=true` — tab mới |
| "Xuất bản" button | `→ navigate` · mock | `admin-posts.html` — Blazor sẽ PATCH status=published |
| Tag input (Enter) | `→ toggle UI` | Thêm tag chip vào danh sách tags |
| Tag chip (✕) | `→ toggle UI` | Xóa tag chip |
| Category select | `→ toggle UI` | Chọn category cho bài |
| Image upload area click | `→ mock` | Prototype: không có file picker — Blazor: upload S3 |
| SEO title / meta textarea | `→ toggle UI` | Cập nhật SEO preview realtime |

---

### admin-predictions.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Row expand (▾) | `→ toggle UI` · `JS: toggleDetail(id)` | Mở rộng row xem chi tiết prediction |
| "Tạo bài" từ prediction | `→ navigate` | `admin-post-editor.html?predId={id}` |
| "Xem bài" link | `→ navigate (tab)` | `post-detail.html?slug={slug}` |
| "Chạy lại" button | `→ mock` | Prototype: alert — Blazor: trigger GeneratePredictionJob cho match này |
| "Approve" / "Reject" | `→ mock` | Prototype: toggle badge — Blazor: PATCH prediction status |

---

### admin-matches.html

| Element | Action | Kết quả |
|---------|--------|---------|
| "Fetch Matches" button | `→ mock` · `JS: triggerFetch()` | Prototype: alert thành công — Blazor: trigger FetchUpcomingMatchesJob |
| "Tạo Prediction" (per row) | `→ mock` · `JS: triggerPrediction(id)` | Prototype: alert — Blazor: trigger GeneratePredictionJob cho trận này |
| "Tạo tất cả Prediction" | `→ mock` · `JS: triggerAllPredictions()` | Prototype: alert — Blazor: bulk trigger |
| "Xem trận" (👁) | `→ navigate (tab)` | `match-detail.html?match={matchId}` |
| "Xem bài" (📄) | `→ navigate (tab)` | `post-detail.html?slug={slug}` |
| Filter bar (giải / status) | `→ toggle UI` · mock | Visual filter |

---

### admin-team.html

| Element | Action | Kết quả |
|---------|--------|---------|
| "Thêm đội" button | `→ toggle UI` · `JS: openEditPanel(null)` | Mở slide-in panel bên phải (mode: create) |
| "Cầu thủ" button per row | `→ navigate` | `admin-players.html?team={slug}` |
| "Sửa" button per row | `→ toggle UI` · `JS: openEditPanel(slug)` | Mở slide-in panel (mode: edit) — prototype không pre-fill data |
| "Xóa" button per row | `→ mock` | Prototype: alert — Blazor: DELETE API |
| Edit panel — "Lưu" | `→ toggle UI` · `JS: saveTeam()` | Prototype: alert + đóng panel — Blazor: POST/PUT API |
| Edit panel — "Hủy" / click overlay | `→ toggle UI` · `JS: closeEditPanel()` | Đóng slide-in panel |
| Team name input (typing) | `→ toggle UI` | Auto-generate slug vào field slug (Vietnamese slug normalize) |
| Filter bar | `→ toggle UI` · mock | Visual filter |

---

### admin-players.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Team filter dropdown change | `→ navigate` · `JS: applyTeamFilter()` | `admin-players.html?team={slug}` — reload với filter |
| "Thêm cầu thủ" button | `→ toggle UI` · `JS: openEditPanel(null)` | Mở slide-in panel (mode: create) |
| "Sửa" button per row | `→ toggle UI` · `JS: openEditPanel(slug)` | Mở slide-in panel (mode: edit) |
| "Xóa" button per row | `→ mock` | Prototype: alert — Blazor: DELETE API |
| Edit panel — "Lưu" | `→ toggle UI` · `JS: savePlayer()` | Prototype: alert + đóng panel |
| Edit panel — "Hủy" / click overlay | `→ toggle UI` · `JS: closeEditPanel()` | Đóng panel |
| Player name input (typing) | `→ toggle UI` | Auto-generate slug |
| Edit panel — Vị trí select | `→ toggle UI` | Thay đổi vị trí — prototype không thay đổi badge trên table |

---

### admin-job-monitor.html

| Element | Action | Kết quả |
|---------|--------|---------|
| "Hangfire Dashboard" link | `→ navigate (tab)` | External URL — Hangfire UI |
| "Chạy ngay" per job | `→ mock` | Prototype: alert — Blazor: trigger job manually via Hangfire API |
| Refresh button | `→ mock` | Prototype: reload page — Blazor: fetch job status từ API |

---

### admin-categories.html

| Element | Action | Kết quả |
|---------|--------|---------|
| Category tab (Categories / Tags) | `→ toggle UI` · `JS: switchCatTab(el, tabId)` | Switch panel |
| Category item click | `→ toggle UI` · `JS: selectCat(el)` | Highlight category + hiện detail panel bên phải |
| "Thêm category" / "Thêm tag" | `→ toggle UI` · mock | Mở form thêm mới |
| Edit / Delete action | `→ mock` | Prototype: alert — Blazor: PUT/DELETE API |

---

## URL Parameters

| Trang | Param | Kiểu | Ý nghĩa |
|-------|-------|------|---------|
| `home.html` | `?league=X` | string slug | Scroll + highlight league X trong sidebar + match list |
| `league-page.html` | `?league=X` | string slug | Hiện detail giải X (standings, fixtures, scorers) |
| `predictions.html` | `?league=X` | string slug | Filter predictions theo giải |
| `search-results.html` | `?q=` | string | Query tìm kiếm (match + post + team) |
| `category-tag.html` | `?tag=X` | string slug | Lọc bài viết theo tag |
| `category-tag.html` | `?cat=X` | string slug | Lọc bài viết theo category |
| `team-profile.html` | `?team=X` | string slug | Profile đội bóng X |
| `match-detail.html` | `?match=X` | number/slug | Chi tiết trận X |
| `player-profile.html` | `?player=X` | number/slug | Profile cầu thủ X |
| `post-detail.html` | `?slug=X` | string slug | Bài viết theo slug |
| `post-detail.html` | `?preview=true` | boolean | Xem draft (chưa publish) — chỉ admin |
| `admin-post-editor.html` | `?id=X` | number | Edit bài viết có sẵn |
| `admin-post-editor.html` | `?predId=X` | number | Tạo bài từ prediction |
| `admin-players.html` | `?team=X` | string slug | Filter cầu thủ theo đội |

---

## Bảng trang Public

| File | Tiêu đề | Layout | Data JSON | Initializer |
|------|---------|--------|-----------|-------------|
| `home.html` | Trang chủ | 3-col | leagues + matches + posts | `initHomePage()` |
| `news.html` | Tin tức | 3-col | leagues + posts | `initNewsPage()` |
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

| File | Tiêu đề | Mô tả ngắn | Status |
|------|---------|------------|--------|
| `admin-login.html` | Đăng nhập | Form auth, redirect dashboard | ✅ |
| `admin-dashboard.html` | Dashboard | Stats cards, recent activity, bảng bài viết | ✅ |
| `admin-posts.html` | Bài viết | Danh sách + filter + action table | ✅ |
| `admin-post-editor.html` | Tạo/Sửa bài | Rich text editor + metadata | ✅ |
| `admin-predictions.html` | Dự đoán AI | Bảng predictions + accuracy stats | ✅ |
| `admin-categories.html` | Categories & Tags | Tree + detail panel | ✅ |
| `admin-matches.html` | Trận đấu | Danh sách trận + link post/detail | ✅ |
| `admin-users.html` | Users | CRUD user table | ✅ |
| `admin-job-monitor.html` | Job Monitor | API quota, Hangfire jobs, Telegram status | ✅ |
| `admin-settings.html` | Cài đặt chung | System config: AI keys, Football API, Telegram | ✅ |
| `admin-prompts.html` | Prompt Templates | CRUD prompt theo PostType (DuDoan/NhanDinh/PhanTich) | ✅ |
| `admin-team.html` | Đội bóng | CRUD đội (tên, logo, quốc gia, giải đấu) | ✅ |
| `admin-players.html` | Cầu thủ | CRUD cầu thủ (tên, số áo, vị trí, đội, stats) | ✅ |

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
renderPosts()        → .right-scroll      ← right sidebar (chỉ 3-col pages)
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

**Page initializers** (10):
`initHomePage` · `initLeaguePage` · `initMatchDetailPage` · `initTeamPage` · `initPlayerPage` · `initPredictionsPage` · `initCategoryPage` · `initPostDetailPage` · `initSearchPage` · `initNewsPage`

**Render functions** (8 global + nhiều inline):
`renderLeagueTree` · `renderMatches` · `renderLeagueGroup` · `renderMatchRow` · `renderPosts` · `renderFeaturedPost` · `renderPostItem` · `updateLivePill`

*Inline (trong từng trang):* `renderLeagueDetail` · `renderMatchDetail` · `renderTeam` · `renderPlayer` · `renderPredictions` · `renderCategory` · `renderSearch`

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

## Vấn đề còn thiếu / cần fix

### ✅ Trang đã tạo đủ
Tất cả 23 trang đã có file HTML prototype.

### ⚠️ Logic còn mock trong prototype (sẽ implement ở Blazor)

| Vị trí | Mock gì | Blazor thật sẽ làm |
|--------|---------|-------------------|
| `home.html` date bar | Visual only, không filter trận | Query matches theo date param |
| `admin-matches.html` Fetch/Prediction buttons | alert() mock | Trigger Hangfire job qua API |
| `admin-post-editor.html` image upload | Không có file picker | Upload lên S3, lưu URL |
| `admin-predictions.html` Approve/Reject | Toggle badge visual | PATCH prediction.Status |
| Mọi trang Delete action | alert() mock | DELETE API + reload list |
| `news.html` filter pill | Visual only | Query posts theo category |
| Pagination tất cả trang | Visual only (changePage active) | Server-side pagination |

### ℹ️ Chưa implement (Phase 4-6, không phải prototype)
| Tính năng | Ghi chú |
|-----------|---------|
| Auth session expired → redirect login | Blazor middleware xử lý, không cần prototype |
| Role check user row (chỉ admin thấy link dashboard) | Blazor auth, không cần prototype |
| Live score SignalR subscribe | Chỉ cần implement khi tách Blazor |

---

*Cập nhật lần cuối: 2026-04-10 — thêm Interaction Catalog*
