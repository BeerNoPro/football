# Plan: Audit & Document Razor Component Coverage

## Context
User đang test UI và nhận thấy nhiều trang prototype HTML chưa được convert sang Razor.
Mục tiêu: kiểm kê đầy đủ → ghi vào `FootballBlog.Web/Components/FLOW_RAZOR.md` để
có bức tranh rõ ràng về những gì đã có và còn thiếu.

---

## Kết quả Audit

### Public Pages — 11 trang HTML vs Razor

| HTML Prototype | Route | Razor File | Status |
|----------------|-------|-----------|--------|
| `home.html` | `/` | `Pages/Blog/Home.razor` | ✅ Có |
| `news.html` | `/news` | `Pages/Blog/News.razor` | ✅ Có |
| `post-detail.html` | `/posts/{Slug}` | `Pages/Blog/PostDetail.razor` | ✅ Có |
| `category-tag.html` | `/category/{Slug}` | `Pages/Blog/CategoryDetail.razor` | ✅ Có |
| `category-tag.html` | `/tag/{Slug}` | `Pages/Blog/TagDetail.razor` | ✅ Có |
| `search-results.html` | `/search` | `Pages/Blog/SearchResults.razor` | ✅ Có |
| `404.html` | (NotFound) | `Pages/Blog/NotFound.razor` | ✅ Có |
| `predictions.html` | `/predictions` | ❌ Chưa có | ❌ Thiếu |
| `league-page.html` | `/league/{Slug}` | ❌ Chưa có | ❌ Thiếu |
| `match-detail.html` | `/match/{Id}` | ❌ Chưa có | ❌ Thiếu |
| `team-profile.html` | `/team/{Slug}` | ❌ Chưa có | ❌ Thiếu |
| `player-profile.html` | `/player/{Slug}` | ❌ Chưa có | ❌ Thiếu |

**Public thiếu: 5 trang**

### Admin Pages — 13 trang HTML vs Razor

| HTML Prototype | Route | Razor File | Status |
|----------------|-------|-----------|--------|
| `admin-login.html` | `/admin/login` | `Pages/Admin/Login.razor` | ✅ Có |
| `admin-dashboard.html` | `/admin` | `Pages/Admin/Dashboard.razor` | ✅ Có |
| `admin-posts.html` | `/admin/posts` | `Pages/Admin/Posts/Index.razor` | ✅ Có |
| `admin-post-editor.html` | `/admin/posts/create` | `Pages/Admin/Posts/Create.razor` | ✅ Có |
| `admin-post-editor.html` | `/admin/posts/edit/{Id}` | `Pages/Admin/Posts/Edit.razor` | ✅ Có |
| `admin-predictions.html` | `/admin/predictions` | `Pages/Admin/Predictions/Index.razor` | ✅ Có |
| `admin-categories.html` | `/admin/categories` | `Pages/Admin/Categories/Index.razor` | ✅ Có (tách thêm Tags) |
| `admin-matches.html` | `/admin/matches` | `Pages/Admin/Matches/Index.razor` | ✅ Có |
| `admin-job-monitor.html` | `/admin/jobs` | `Pages/Admin/Jobs/Index.razor` | ✅ Có |
| `admin-settings.html` | `/admin/settings` | `Pages/Admin/Settings/Index.razor` | ✅ Có |
| `admin-prompts.html` | `/admin/prompts` | `Pages/Admin/Prompts/Index.razor` | ✅ Có |
| `admin-users.html` | `/admin/users` | ❌ Chưa có | ❌ Thiếu |
| `admin-team.html` | `/admin/teams` | ❌ Chưa có | ❌ Thiếu |
| `admin-players.html` | `/admin/players` | ❌ Chưa có | ❌ Thiếu |

**Admin thiếu: 3 trang**

---

## Task duy nhất: Ghi FLOW_RAZOR.md

Ghi toàn bộ audit trên vào `FootballBlog.Web/Components/FLOW_RAZOR.md` theo format:
- Bảng trang Public (có/thiếu)
- Bảng trang Admin (có/thiếu)  
- Bảng Shared Components
- Danh sách trang còn thiếu cần implement
- Lưu ý về API endpoints cần bổ sung cho các trang thiếu

**Không tạo Razor file mới** — chỉ ghi documentation.

---

## Verification
Đọc lại `FLOW_RAZOR.md` sau khi ghi để xác nhận nội dung chính xác.
