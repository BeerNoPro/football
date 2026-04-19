---
paths:
  - "FootballBlog.Web/**/*.razor"
  - "FootballBlog.Web/**/*.razor.cs"
---

# Blazor Rules

## Render Mode (CRITICAL)

### Kiến trúc hiện tại: Interactive Island per-page
- `<Routes />` trong App.razor là **SSR** — không có global rendermode
- Admin pages dùng `@rendermode InteractiveServer` trên từng page → "Interactive Island"
- Public pages (Home, PostDetail, etc.) và Login/Logout dùng SSR (không có `@rendermode`)
- Login.razor **BẮT BUỘC là SSR** vì dùng `HttpContext`, `<form method="post">`, `HttpContext.Response.Redirect` — không hoạt động trong InteractiveServer

### Anti-pattern vòng lặp — KHÔNG làm lại
❌ **KHÔNG đặt `@rendermode InteractiveServer` lên `<Routes />`** → Login.razor (dùng HttpContext + SSR form) sẽ bị phá vỡ  
❌ **KHÔNG đặt `@rendermode InteractiveServer` lên AdminLayout** → lỗi `Cannot pass parameter 'Body'` (RenderFragment không serialize được sang island)  
❌ **KHÔNG bỏ `@rendermode` khỏi admin pages** → drawer toggle và MudBlazor components mất interactivity

### MudBlazor providers — Interactive Island pattern

**AdminLayout là SSR → KHÔNG đặt MudPopoverProvider/MudDialogProvider/MudSnackbarProvider ở AdminLayout.**  
Lý do: AdminLayout (SSR) và island page (InteractiveServer) là 2 render context tách biệt. Nếu cả 2 khai báo cùng provider → section ID bị đăng ký 2 lần → crash `InvalidOperationException: There is already a subscriber`.

**Rule**: Mỗi admin page tự khai báo đúng providers mình cần, ngay sau các directive:

| Nếu page dùng | Cần khai báo |
|---|---|
| MudSelect / MudMenu / MudTooltip / MudAutocomplete | `<MudPopoverProvider />` |
| IDialogService | `<MudDialogProvider />` |
| ISnackbar | `<MudSnackbarProvider />` |

```razor
@page "/admin/example"
@rendermode InteractiveServer
@inherits AdminPageBase
@inject ISnackbar Snackbar
@inject IDialogService DialogService

<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

> **Checklist khi tạo admin page mới:**  
> - [ ] AdminLayout không có providers → đừng thêm vào đó  
> - [ ] Page dùng MudSelect/MudMenu → thêm `<MudPopoverProvider />`  
> - [ ] Page inject IDialogService → thêm `<MudDialogProvider />`  
> - [ ] Page inject ISnackbar → thêm `<MudSnackbarProvider />`

## Gọi API từ Blazor
- Dùng typed HttpClient được inject, KHÔNG gọi DbContext trực tiếp
- **Public pages (SSR):** `IPostApiClient`, `ICategoryApiClient`, `ITagApiClient`
- **Admin pages:** `IAdminApiClient` — auto-inject Bearer JWT qua `JwtAuthHandler`
- Thêm client mới: interface trong `Web/ApiClients/`, đăng ký `builder.Services.AddHttpClient<...>`

```razor
@inject IPostApiClient PostClient

@code {
    private PagedResult<PostSummaryDto>? _posts;

    protected override async Task OnInitializedAsync()
    {
        _posts = await PostClient.GetPublishedAsync(page: 1, pageSize: 10);
    }
}
```

## SEO (Static SSR pages)
- Luôn set `<PageTitle>` và meta description
- Dùng `<HeadContent>` cho Open Graph tags
- Slug: tiếng Việt không dấu, lowercase, dấu gạch ngang (dùng `SlugService.Generate()`)

## Component
- Tách `.razor.cs` khi logic > 50 dòng | inject qua `@inject` | `EventCallback` cho parent communication
- SignalR: dispose `HubConnection` trong `IAsyncDisposable` | check connection state trước khi gọi

## Styling
- Tailwind CSS utility classes | không inline style (trừ dynamic value) | CSS isolation (`.razor.css`)
- Admin pages: dùng MudBlazor components
