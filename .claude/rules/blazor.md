---
paths:
  - "FootballBlog.Web/**/*.razor"
  - "FootballBlog.Web/**/*.razor.cs"
---

# Blazor Component Rules

## Render Mode — QUAN TRỌNG
- Trang Blog, Home, bài viết, category, tag → KHÔNG đặt @rendermode → mặc định Static SSR
- Live score widget, tường thuật realtime → `@rendermode InteractiveServer`
- Admin panel toàn bộ → `@rendermode InteractiveServer`
- TUYỆT ĐỐI KHÔNG dùng `@rendermode InteractiveWebAssembly`

## SEO cho Static SSR Pages
- Luôn set PageTitle và meta description
- Dùng <HeadContent> để inject Open Graph tags
- Slug URL phải là tiếng Việt không dấu, lowercase, dùng dấu gạch ngang

## Component Structure
- Code-behind tách file .razor.cs nếu logic phức tạp hơn 50 dòng
- Inject service qua @inject, không dùng static
- Dùng EventCallback cho component giao tiếp với parent

## SignalR / Realtime
- Dispose HubConnection trong IAsyncDisposable
- Luôn kiểm tra connection state trước khi gọi
- Hiển thị loading state khi chờ kết nối

## Styling
- Dùng Tailwind CSS utility classes
- Không viết inline style trừ trường hợp dynamic value
- Component có style riêng thì dùng CSS isolation (file .razor.css)
