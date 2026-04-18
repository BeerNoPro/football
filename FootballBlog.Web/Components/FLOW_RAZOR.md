# FLOW_RAZOR.md — Bản đồ Razor Components FootballBlog

> Đối chiếu prototype HTML (`wwwroot/prototype/`) với Razor components thực tế.
> Cập nhật khi thêm trang mới hoặc convert thêm prototype.
> Tham chiếu thêm: `wwwroot/FLOW_HTML.md` (prototype map) và `docs/FLOW.md` (system flow).

---

## Tổng quan

| Thống kê | Prototype HTML | Razor Hiện có | Còn thiếu |
|----------|---------------|--------------|-----------|
| Public pages | 11 | 12 | 0 |
| Admin pages | 13 | 14 (+Tags, +Users, +Teams, +Players) | 0 |
| Shared components | — | 9 | — |
| Layout | 3-col / 2-col / standalone | 3-col / 2-col / Empty / Admin | — |

---

## Public Pages

| HTML Prototype | Route Blazor | Razor File | Render Mode | Status |
|----------------|-------------|-----------|-------------|--------|
| `home.html` | `/` | `Pages/Blog/Home.razor` | Static SSR | ✅ Có |
| `news.html` | `/news` | `Pages/Blog/News.razor` | Static SSR | ✅ Có |
| `post-detail.html` | `/posts/{Slug}` | `Pages/Blog/PostDetail.razor` | Static SSR | ✅ Có |
| `category-tag.html` | `/category/{Slug}` | `Pages/Blog/CategoryDetail.razor` | Static SSR | ✅ Có |
| `category-tag.html` | `/tag/{Slug}` | `Pages/Blog/TagDetail.razor` | Static SSR | ✅ Có |
| `search-results.html` | `/search` | `Pages/Blog/SearchResults.razor` | Static SSR | ✅ Có |
| `404.html` | (NotFound route) | `Pages/Blog/NotFound.razor` | Static SSR | ✅ Có |
| `predictions.html` | `/predictions` | `Pages/Blog/Predictions.razor` | Static SSR | ✅ Có |
| `league-page.html` | `/league/{Slug}` | `Pages/Blog/LeaguePage.razor` | Static SSR | ✅ Có |
| `match-detail.html` | `/match/{Id}` | `Pages/Blog/MatchDetail.razor` | Static SSR | ✅ Có |
| `team-profile.html` | `/team/{Slug}` | `Pages/Blog/TeamProfile.razor` | Static SSR | ✅ Có |
| `player-profile.html` | `/player/{Slug}` | `Pages/Blog/PlayerProfile.razor` | Static SSR | ✅ Có |

> Ngoài prototype: `Pages/LiveScore/Index.razor` (`/livescore`, InteractiveServer) — không có prototype HTML tương ứng.

---

## Admin Pages

| HTML Prototype | Route Blazor | Razor File | Status |
|----------------|-------------|-----------|--------|
| `admin-login.html` | `/admin/login` | `Pages/Admin/Login.razor` | ✅ Có |
| `admin-dashboard.html` | `/admin` | `Pages/Admin/Dashboard.razor` | ✅ Có |
| `admin-posts.html` | `/admin/posts` | `Pages/Admin/Posts/Index.razor` | ✅ Có |
| `admin-post-editor.html` | `/admin/posts/create` | `Pages/Admin/Posts/Create.razor` | ✅ Có |
| `admin-post-editor.html` | `/admin/posts/edit/{Id:int}` | `Pages/Admin/Posts/Edit.razor` | ✅ Có |
| `admin-predictions.html` | `/admin/predictions` | `Pages/Admin/Predictions/Index.razor` | ✅ Có |
| `admin-categories.html` | `/admin/categories` | `Pages/Admin/Categories/Index.razor` | ✅ Có |
| `admin-categories.html` (tab Tags) | `/admin/tags` | `Pages/Admin/Tags/Index.razor` | ✅ Có (tách riêng) |
| `admin-matches.html` | `/admin/matches` | `Pages/Admin/Matches/Index.razor` | ✅ Có |
| `admin-job-monitor.html` | `/admin/jobs` | `Pages/Admin/Jobs/Index.razor` | ✅ Có |
| `admin-settings.html` | `/admin/settings` | `Pages/Admin/Settings/Index.razor` | ✅ Có |
| `admin-prompts.html` | `/admin/prompts` | `Pages/Admin/Prompts/Index.razor` | ✅ Có |
| `admin-users.html` | `/admin/users` | `Pages/Admin/Users/Index.razor` | ✅ Có |
| `admin-team.html` | `/admin/teams` | `Pages/Admin/Teams/Index.razor` | ✅ Có |
| `admin-players.html` | `/admin/players` | `Pages/Admin/Players/Index.razor` | ✅ Có |

> Ngoài prototype: `Pages/Admin/ApiKeys/Index.razor` (`/admin/api-keys`) — quản lý API keys, không có prototype HTML.

---

## Shared Components

| File | Mô tả | Dùng bởi |
|------|-------|----------|
| `Shared/LeftSidebar.razor` | Sidebar trái: logo, search, league tree | `PublicLayout2Col`, `PublicLayout3Col` |
| `Shared/RightSidebar.razor` | Sidebar phải: AI Predictions tabs + posts | `PublicLayout3Col` |
| `Shared/PostCard.razor` | Card bài viết dạng đầy đủ | News, CategoryDetail, TagDetail |
| `Shared/PostCardCompact.razor` | Card bài viết dạng compact (sidebar) | RightSidebar |
| `Shared/TagPill.razor` | Pill tag link đến `/tag/{Slug}` | PostDetail, PostCard |
| `Shared/Pagination.razor` | Điều hướng phân trang | News, CategoryDetail, TagDetail, SearchResults |
| `Shared/SeoHead.razor` | Meta SEO + OG + JSON-LD | Mọi public page |
| `Shared/LiveScoreWidget.razor` | Widget live score realtime (SignalR) | LiveScore/Index |
| `Shared/RedirectToLogin.razor` | Redirect về `/admin/login` nếu chưa auth | Routes.razor |

---

## Layout Components

| File | Dùng cho | Prototype tương ứng |
|------|----------|-------------------|
| `Layout/PublicLayout3Col.razor` | Public pages 3-col | `home.html`, `news.html`, `post-detail.html`... |
| `Layout/PublicLayout2Col.razor` | Public pages 2-col | `search-results.html`, `category-tag.html` |
| `Layout/AdminLayout.razor` | Tất cả admin pages | Sidebar admin chung |
| `Layout/EmptyLayout.razor` | Login page | `admin-login.html` |
| `Layout/MainLayout.razor` | Error boundary wrapper | — |

---

## Admin Helper Components

| File | Mô tả |
|------|-------|
| `Admin/QuillEditor.razor` | Rich text editor (Quill.js interop), InteractiveServer |
| `Admin/AdminMudProviders.razor` | DEPRECATED — đã chuyển vào `AdminLayout.razor` |

---

## Trang Cần Kết Nối API Thực (mock data hiện tại)

Tất cả 8 trang mới đang dùng mock/static data. Khi có API endpoint, cần replace:

### Public — 5 trang

| Trang | API endpoint cần bổ sung |
|-------|--------------------------|
| `Pages/Blog/Predictions.razor` | `GET /api/predictions` — list predictions nhóm theo league |
| `Pages/Blog/LeaguePage.razor` | `GET /api/leagues/{slug}` — standings, top scorers, fixtures |
| `Pages/Blog/MatchDetail.razor` | `GET /api/matches/{id}` — lineup, events, stats |
| `Pages/Blog/TeamProfile.razor` | `GET /api/teams/{slug}` — squad, results, fixtures |
| `Pages/Blog/PlayerProfile.razor` | `GET /api/players/{slug}` — stats, career, upcoming matches |

### Admin — 3 trang

| Trang | API endpoint cần bổ sung |
|-------|--------------------------|
| `Pages/Admin/Users/Index.razor` | `GET/POST/PUT/DELETE /api/users` |
| `Pages/Admin/Teams/Index.razor` | `GET/POST/PUT/DELETE /api/teams` (đã có partial qua UoW) |
| `Pages/Admin/Players/Index.razor` | `GET/POST/PUT/DELETE /api/players` |

---

## Lưu ý Khi Implement Trang Còn Thiếu

### Public pages
- Tất cả dùng **Static SSR** (không `@rendermode`) — SEO-friendly
- Layout: 3-col dùng `PublicLayout3Col`, thêm `@layout PublicLayout3Col`
- Data hiện là mock/static — replace bằng typed HttpClient khi có API endpoint
- `MatchDetail` cần embed `LiveScoreWidget` (InteractiveServer) cho live matches

### Admin pages
- Kế thừa `AdminPageBase` để tự inject JWT
- Render mode: `@rendermode InteractiveServer`
- `Teams`, `Players`, `Users` dùng slide-in edit panel (toggle `_panelOpen`)
- Nút Save/Delete hiện là UI-only — wire vào `IAdminApiClient` khi có endpoint

---

## Navigation Links Cần Update Khi Thêm Trang

Khi tạo thêm trang thiếu, cần update links ở các nơi sau:

| Trang thiếu | Nơi cần update link |
|-------------|---------------------|
| `/league/{Slug}` | `LeftSidebar.razor` (league item click), `Home.razor` (league name link) |
| `/match/{Id}` | `Home.razor` (match row click), `LeaguePage.razor`, `TeamProfile.razor` |
| `/team/{Slug}` | `Home.razor` (team name click), `MatchDetail.razor`, `PlayerProfile.razor` |
| `/player/{Slug}` | `LeaguePage.razor` (top scorer), `TeamProfile.razor` (player row) |
| `/predictions` | `Home.razor` tab "Dự đoán AI", `AdminLayout.razor` sidebar |
| `/admin/users` | `AdminLayout.razor` sidebar nav |
| `/admin/teams` | `AdminLayout.razor` sidebar nav |
| `/admin/players` | `AdminLayout.razor` sidebar nav, `Admin/Teams/Index.razor` |

---

*Cập nhật lần cuối: 2026-04-18 — Tất cả 8 trang còn thiếu đã được tạo với mock data*
