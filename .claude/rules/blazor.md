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

## SEO (Static SSR pages)
- Luôn set `<PageTitle>` và meta description
- Dùng `<HeadContent>` cho Open Graph tags
- Slug: tiếng Việt không dấu, lowercase, dùng dấu gạch ngang

## Component
- Tách `.razor.cs` khi logic > 50 dòng | inject qua `@inject` | dùng `EventCallback` cho parent communication
- SignalR: dispose `HubConnection` trong `IAsyncDisposable` | check connection state trước khi gọi

## Styling
- Tailwind CSS utility classes | không inline style (trừ dynamic value) | CSS isolation cho style riêng (`.razor.css`)
