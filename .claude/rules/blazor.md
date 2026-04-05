---
paths:
  - "FootballBlog.Web/**/*.razor"
  - "FootballBlog.Web/**/*.razor.cs"
---

# Blazor Rules

## Render Mode (CRITICAL)
- Blog/Home/Post/Category/Tag → KHÔNG đặt @rendermode (Static SSR, SEO)
- Live score widget + admin → `@rendermode InteractiveServer`
- KHÔNG dùng `@rendermode InteractiveWebAssembly`

## Gọi API từ Blazor
- Dùng typed HttpClient được inject, KHÔNG gọi DbContext trực tiếp
- Clients đã có: `IPostApiClient`, `ICategoryApiClient`
- Thêm client mới: interface trong Core, implementation trong Web, đăng ký `builder.Services.AddHttpClient<...>`

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
