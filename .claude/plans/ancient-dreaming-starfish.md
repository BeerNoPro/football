# Plan: Fix UI — Razor Components match HTML Prototypes

## Context
Các Razor component hiện dùng CSS class names từ `css/common.css` nhưng nhiều class bị thiếu
(`.hero`, `.news-card`, `.pagination`, `.news-grid`, ...) vì prototype dùng inline `<style>` block
cho page-specific CSS, còn `common.css` chỉ có shared components (sidebar, match-row, tabs...).
Ngoài ra logic `GetEmoji()`/`FormatTimeAgo()` bị copy ở 3 nơi.

Mục tiêu: Blazor app nhìn giống hệt HTML prototype — Phase 4 dùng mock data tĩnh.

---

## Phase A — Thêm CSS thiếu vào `wwwroot/css/common.css`

Append các section sau vào cuối file (sau line 697):

### A1. Fix CSS variable alias (tokens.css)
Thêm vào `wwwroot/css/tokens.css`:
```css
--text-main: var(--text);   /* alias cho Razor components dùng --text-main */
--border-main: var(--border); /* alias */
```

### A2. Hero section
Class: `.hero`, `.hero-bg`, `.hero-number`, `.hero-content`,
`.hero-tag`, `.hero-title`, `.hero-sub`, `.hero-dots`, `.hero-dot`

Spec từ prototype: height 200px, gradient `#1a1a2e → #0f3460 → #16213e`,
hero-number = 180px font ghost text, hero-content absolute bottom-left.

### A3. Section header (dùng lại ở Home + News)
Class: `.section-hd`, `.section-hd-title`, `.section-hd-more`
Spec: display flex, justify-content space-between, padding 14px 16px,
border-bottom 1px solid var(--border).

### A4. News grid layout (dùng lại ở Home + News + Category + Tag + Search)
Class: `.news-grid`
Spec: `display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; padding: 12px 0;`

### A5. PostCard — news-card
Classes: `.news-card`, `.news-card-thumb`, `.news-card-cat`, `.news-card-body`,
`.news-card-league`, `.news-card-title`, `.news-card-meta`, `.news-card-time`, `.news-card-read`

Spec từ prototype:
- `.news-card`: background var(--bg-card), border-radius 10px, overflow hidden, hover translateY(-2px)
- `.news-card-thumb`: height 120px, display flex, align-items center, justify-content center, position relative, font-size 36px
- `.news-card-cat`: position absolute top-left badge, background var(--accent-dim), color var(--accent), font-size 9px
- `.news-card-body`: padding 10px 12px
- `.news-card-title`: font-size 13px, font-weight 700, line-clamp 2
- `.news-card-meta`: display flex, justify-content space-between, font-size 10px
- `.news-card-read`: color var(--accent)

### A6. Breaking news strip (News page)
Classes: `.breaking-strip`, `.breaking-label`, `.breaking-text`
Spec: display flex, align-items center, background var(--bg-dark), padding 8px 16px,
label = accent badge "TIN MỚI", text = muted overflow ellipsis.

### A7. Filter pills (News page)
Classes: `.news-filter-bar`, `.filter-pill`, `.filter-pill.active`
Spec: news-filter-bar = flex, gap 6px, padding 10px 16px, overflow-x auto, flex-shrink 0;
filter-pill = inline-flex, border 1px solid var(--border2), border-radius 20px, padding 4px 12px, font-size 11px, cursor pointer;
filter-pill.active = border-color var(--accent), color var(--accent), background var(--accent-dim).

### A8. News hero (featured post — News page)
Classes: `.news-hero`, `.news-hero-inner`, `.news-hero-content`, `.news-hero-image`,
`.hero-category-pill`, `.hero-title`, `.hero-excerpt`, `.hero-meta`, `.hero-meta-sep`
Spec: news-hero-inner = grid, grid-template-columns 1fr 280px, gap 0, height 220px;
content side = flex column, justify-content flex-end, padding 20px, gradient overlay;
image side = background gradient placeholder.

### A9. Pagination
Classes: `.pagination`, `.page-btn`, `.page-prev`, `.page-next`,
`.page-num`, `.page-num.active`, `.page-ellipsis`
Spec: pagination = display flex, justify-content center, gap 4px, padding 20px 16px;
page-num = 32px × 32px, border-radius 6px, border 1px solid var(--border2), font-size 12px;
page-num.active = background var(--accent), color #111, border-color var(--accent).

### A10. Post detail article layout
Classes: `.article-col`, `.article-scroll`, `.article-wrap`, `.breadcrumb`,
`.article-title`, `.article-lead`, `.article-body`, `.article-tags`
Spec: article-wrap = max-width 760px, margin 0 auto, padding 24px 24px 40px;
article-title = font-size 26px, font-weight 900, line-height 1.2, margin 12px 0;
article-lead = font-size 14px, color var(--muted2), line-height 1.6, margin-bottom 20px;
article-body = font-size 14px, line-height 1.8, color var(--text).

Post body typography (`.article-body`):
- h2: font-size 18px, font-weight 800, margin 24px 0 10px, color var(--text)
- h3: font-size 16px, font-weight 700, margin 20px 0 8px
- p: margin 0 0 14px
- blockquote: border-left 3px solid var(--accent), padding 12px 16px, background var(--accent-dim), color var(--muted2), margin 20px 0
- code: background var(--bg-card), padding 2px 6px, border-radius 4px, font-size 12px
- pre: background var(--bg-card), padding 16px, border-radius 8px, overflow-x auto
- img: max-width 100%, border-radius 8px, margin 16px 0
- ul, ol: padding-left 20px, margin 0 0 14px

### A11. Tag/Category header (Category + Tag pages)
Classes: `.tag-header`, `.tag-header-top`, `.tag-icon`, `.tag-info`,
`.tag-name`, `.tag-sub`, `.tag-filter-row`, `.tag-chips`, `.tag-chip`, `.tag-chip.active`
Spec: tag-header = flex-shrink 0, border-bottom 1px solid var(--border), background var(--bg-sidebar);
tag-header-top = flex, align-items center, gap 12px, padding 16px 20px;
tag-icon = 44px × 44px, border-radius 10px, background var(--accent-dim), display flex center, font-size 22px;
tag-name = font-size 18px, font-weight 900;
tag-sub = font-size 11px, color var(--muted);
tag-filter-row = flex, gap 6px, padding 10px 20px, border-top 1px solid var(--border).

### A12. Tag chip (reusable pill)
Class: `.tag-chip`
Spec: display inline-flex, align-items center, gap 5px, padding 4px 10px, border-radius 20px,
border 1px solid var(--border2), font-size 11px, color var(--muted2), cursor pointer;
`:hover` → border-color var(--accent), color var(--accent).

### A13. Posts grid (Category + Tag pages)
Classes: `.posts-scroll`, `.posts-grid`, `.post-card`, `.post-card-img`,
`.post-card-body`, `.post-card-tag`, `.post-card-title`, `.post-card-excerpt`,
`.post-card-meta`, `.post-card-time`
Spec: posts-scroll = flex-1, overflow-y auto;
posts-grid = display grid, grid-template-columns repeat(2, 1fr), gap 12px, padding 16px;
post-card = background var(--bg-card), border-radius 10px, overflow hidden, border 1px solid var(--border);
post-card-img = height 140px, display flex center, font-size 40px;
post-card-body = padding 10px 12px;
post-card-tag = font-size 9px, font-weight 700, color var(--accent), text-transform uppercase, margin-bottom 4px;
post-card-title = font-size 13px, font-weight 700, line-clamp 2, margin-bottom 6px;
post-card-excerpt = font-size 11px, color var(--muted2), line-clamp 2, margin-bottom 8px;
post-card-meta = flex, justify-content space-between, font-size 10px, color var(--muted).

### A14. Search hero (Search page)
Classes: `.search-hero`, `.search-hero-bar`, `.search-meta`, `.search-count`,
`.search-filters`, `.search-filter-btn`, `.search-filter-btn.active`,
`.results-scroll`, `.result-section-header`, `.result-section-title`,
`.post-results-grid`, `.post-result-card`, `.post-result-img`, `.post-result-body`
Spec: search-hero = flex-shrink 0, padding 16px, border-bottom 1px solid var(--border);
search-hero-bar = search input full width với search icon, giống .search-input-wrap nhưng lớn hơn;
search-meta = flex, justify-content space-between, align-items center, margin-top 10px;
search-count = font-size 12px, color var(--muted);
search-filter-btn = giống filter-pill;
result-section-header = padding 10px 16px, background var(--bg-dark), font-size 11px, font-weight 700;
post-results-grid = display grid, grid-template-columns repeat(2, 1fr), gap 12px, padding 16px.

### A15. Match toolbar + Date bar (Home page mock)
Classes đã có trong common.css (`.match-toolbar`, `.live-pill`, `.tab-date-row`, `.date-bar`, `.date-btn2`)
→ Chỉ cần render HTML đúng structure, không cần thêm CSS.

---

## Phase B — Shared Helper

**File mới:** `FootballBlog.Web/Helpers/PostHelpers.cs`
```csharp
namespace FootballBlog.Web.Helpers;

public static class PostHelpers
{
    public static string GetEmoji(string? category) => ...;
    public static string GetThumbBg(string? thumbnail) => ...;
    public static string FormatTimeAgo(DateTime dt) => ...;
}
```

**Cập nhật** `Components/_Imports.razor`:
```razor
@using FootballBlog.Web.Helpers
```

---

## Phase C — Cập nhật từng Razor Component

### C1. `PostCard.razor`
- Dùng `PostHelpers.GetEmoji()`, `PostHelpers.FormatTimeAgo()`, `PostHelpers.GetThumbBg()`
- Xóa `@code` private methods
- Class `news-card-thumb` giữ, CSS sẽ được thêm ở Phase A

### C2. `PostCardCompact.razor`
- Dùng PostHelpers thay private methods

### C3. `RightSidebar.razor`
- Dùng PostHelpers thay private methods
- Đổi `.post-featured-thumb` → `.post-featured-img` (đúng với common.css đã có)

### C4. `Pagination.razor`
- Đổi `.page-btn` → `.page-num` cho numbered buttons (đang dùng sai class)
- Class `.page-prev`, `.page-next` cho prev/next buttons — OK
- Class `.page-num.active` cho active — check

### C5. `Home.razor`
Thay phần "Match list placeholder" bằng match panel với mock data:
```
<div class="match-panel">
  <div class="match-toolbar">
    <div class="live-pill"><span class="live-dot"></span> LIVE · 2</div>
    <div class="match-search-bar">...</div>
    <button class="filter-btn">Tất cả giải ▾</button>
  </div>
  <div class="tab-date-row">
    <div class="tabs">
      <a class="tab-btn active">📅 Lịch thi đấu</a>
      <a class="tab-btn">🤖 Dự đoán AI</a>
      <a class="tab-btn">📰 Tin tức</a>
    </div>
    <div class="date-bar">
      <!-- 7 date buttons, today active -->
    </div>
  </div>
  <div class="matches-list">
    <!-- 2-3 league groups với 3-4 match rows mỗi nhóm, dùng mock data C# list -->
  </div>
</div>
```
Mock data: 2 league groups (V.League, Premier League) × 3 matches mỗi nhóm.
Match row dùng class đã có: `.lg`, `.lg-header`, `.match-row`, `.mr-time`, `.mr-home`,
`.mr-away`, `.score-c`, `.mr-status`, `.badge-live`, `.badge-ft`, `.badge-sch`.

### C6. `News.razor`
Thêm:
1. `.breaking-strip` phía trên
2. `.news-hero` cho bài đầu tiên trong `_posts` (thay vì render qua PostCard)
3. `.news-filter-bar` với filter pills — giữ logic active state hiện có

### C7. `PostDetail.razor`
Đổi cấu trúc wrap:
```razor
<div class="article-col">
  <div class="article-scroll">
    <div class="article-wrap">
      <!-- breadcrumb -->
      <!-- article-meta (tags + time) -->
      <!-- h1.article-title -->
      <!-- p.article-lead (excerpt nếu có) -->
      <!-- featured image -->
      <!-- div.article-body → @((MarkupString)_post.Content) -->
      <!-- article-tags -->
    </div>
  </div>
</div>
```
Layout: `PublicLayout3Col` → giữ nguyên.

### C8. `CategoryDetail.razor`
Đổi cấu trúc:
```razor
<div class="tag-header">
  <div class="tag-header-top">
    <div class="tag-icon">📂</div>
    <div class="tag-info">
      <div class="tag-name">@_category.Name</div>
      <div class="tag-sub">@_total bài viết</div>
    </div>
  </div>
</div>
<div class="posts-scroll">
  <div class="posts-grid">
    @foreach (var post in _posts) { <PostCard Post="post" /> }
  </div>
  <Pagination ... />
</div>
```

### C9. `TagDetail.razor`
Tương tự C8, icon `🏷️`, tag-name dùng `#tagName`.

### C10. `SearchResults.razor`
Đổi cấu trúc:
```razor
<div class="search-hero">
  <div class="search-hero-bar">
    <input ... />
    <button ...>Tìm</button>
  </div>
  <div class="search-meta">
    <span class="search-count">@_count kết quả cho "@_query"</span>
    <div class="search-filters">...</div>
  </div>
</div>
<div class="results-scroll">
  @if (_posts.Count > 0) {
    <div class="result-section-header">Bài viết</div>
    <div class="post-results-grid">
      @foreach ... { <PostCard ... /> }
    </div>
    <Pagination ... />
  }
</div>
```

---

## Files cần sửa (14 files)

| File | Thay đổi |
|------|----------|
| `wwwroot/css/tokens.css` | Thêm --text-main, --border-main alias |
| `wwwroot/css/common.css` | Thêm ~250 dòng CSS (A2-A15) |
| `Helpers/PostHelpers.cs` | NEW — GetEmoji, GetThumbBg, FormatTimeAgo |
| `Components/_Imports.razor` | Thêm @using FootballBlog.Web.Helpers |
| `Components/Shared/PostCard.razor` | Dùng PostHelpers |
| `Components/Shared/PostCardCompact.razor` | Dùng PostHelpers |
| `Components/Shared/RightSidebar.razor` | Dùng PostHelpers, sửa post-featured-img |
| `Components/Shared/Pagination.razor` | Sửa CSS class names |
| `Components/Pages/Blog/Home.razor` | Match panel mock data |
| `Components/Pages/Blog/News.razor` | Breaking strip + news-hero + filter bar |
| `Components/Pages/Blog/PostDetail.razor` | article-col/scroll/wrap structure |
| `Components/Pages/Blog/CategoryDetail.razor` | tag-header + posts-scroll/grid |
| `Components/Pages/Blog/TagDetail.razor` | tag-header + posts-scroll/grid |
| `Components/Pages/Blog/SearchResults.razor` | search-hero + results-scroll |

---

## Verification
1. Build: `dotnet build FootballBlog.Web` — không có lỗi
2. Mở browser tại `https://localhost:7241`:
   - `/` — Hero hiển thị, match panel có 2 league groups + match rows, news grid phía dưới
   - `/news` — Breaking strip, hero featured, filter pills, news grid
   - `/posts/{slug}` — Article layout đẹp, typography đúng
   - `/category/{slug}` — Tag header + posts grid 2 cột
   - `/tag/{slug}` — Tương tự category
   - `/search?q=test` — Search hero + result grid
3. So sánh với prototype HTML trong browser — visual phải xấp xỉ giống nhau
