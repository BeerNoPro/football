# Tạo Blazor SSR Page

Khi được gọi với tên page (e.g., `/blazor-page PostDetail`):

## Bước 1 — Xác nhận

Hỏi:
- Page thuộc nhóm nào? Blog public (SSR) / Admin (InteractiveServer) / Realtime widget (InteractiveServer)
- Route pattern? (e.g., `/bai-viet/{slug}`, `/danh-muc/{slug}`)
- Cần inject client nào? (IPostApiClient / ICategoryApiClient / ...)

## Bước 2 — Kiểm tra page đã tồn tại chưa

```bash
find FootballBlog.Web/Components/Pages -name "*{Page}*" 2>/dev/null
```

## Bước 3 — Kiểm tra route conflict

```bash
grep -rn "@page" FootballBlog.Web/Components/Pages/ --include="*.razor" | grep "route-pattern" | head -10
```

## Bước 4 — Tạo file Razor theo đúng nhóm

**Blog public (Static SSR — KHÔNG đặt @rendermode):**
```razor
@page "/route/{param}"
@inject IXxxApiClient XxxClient
@inject NavigationManager Nav

<PageTitle>Tiêu đề trang — Football Blog</PageTitle>
<HeadContent>
    <meta name="description" content="Mô tả trang" />
    <meta property="og:title" content="Tiêu đề" />
    <meta property="og:description" content="Mô tả" />
    <meta property="og:type" content="article" />
</HeadContent>

@* Nội dung trang *@

@code {
    [Parameter] public string Param { get; set; } = "";
    private XxxDto? _data;

    protected override async Task OnInitializedAsync()
    {
        _data = await XxxClient.GetByXxxAsync(Param);
        if (_data == null) Nav.NavigateTo("/404");
    }
}
```

**Admin / Realtime (InteractiveServer):**
```razor
@page "/admin/route"
@rendermode InteractiveServer
@inject IXxxApiClient XxxClient

<PageTitle>Tên trang — Admin</PageTitle>

@code {
    protected override async Task OnInitializedAsync() { ... }
}
```

## Bước 5 — Tạo code-behind nếu logic > 50 dòng

Tách thành `{Page}.razor.cs` partial class.

## Bước 6 — Kiểm tra nav layout

```bash
grep -n "href\|NavLink\|NavigateTo" FootballBlog.Web/Components/Layout/ -r --include="*.razor" | head -20
```

Nếu cần route mới trong nav → nhắc cập nhật layout component.

## Bước 7 — Nhắc build CSS nếu dùng Tailwind class mới

```bash
# Nếu thêm class Tailwind mới vào public page
npm run build:css
```
