# Tạo Blazor SSR Page

Khi được gọi với tên page (e.g., `/blazor-page PostDetail`):

1. Hỏi xác nhận:
   - Page này thuộc nhóm nào? Blog public (SSR) / Admin (InteractiveServer) / Realtime widget (InteractiveServer)
   - Route pattern? (e.g., `/bai-viet/{slug}`, `/danh-muc/{slug}`)
   - Cần inject client nào? (IPostApiClient / ICategoryApiClient / ...)

2. Tạo file Razor theo đúng nhóm:

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

3. Tạo file `.razor.cs` nếu logic > 50 dòng (tách code-behind)

4. Nếu cần route mới trong nav → nhắc cập nhật layout component

5. Nhắc chạy `npm run build:css` nếu thêm Tailwind class mới
