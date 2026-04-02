---
paths:
  - "FootballBlog.API/Controllers/**"
  - "FootballBlog.API/Program.cs"
  - "FootballBlog.Web/Components/Pages/Admin/**"
  - "FootballBlog.Core/Services/**"
---

# Security

- Auth: ASP.NET Core Identity + JWT (API) + Cookie (Blazor SSR) | roles: Admin/Author/Reader (dùng constants)
- Secrets: KHÔNG commit connection string/API key/JWT secret | local: `dotnet user-secrets` | prod: AWS Secrets Manager
- Input: FluentValidation tại API boundary | sanitize HTML bài viết (HtmlSanitizer) | slug regex `^[a-z0-9-]+$`
- SQL: EF Core parameterized queries — raw SQL dùng `FromSqlRaw` với params, KHÔNG string interpolation
- XSS: Blazor tự escape | nếu render raw HTML phải sanitize trước
- Rate limit: login 5 req/phút/IP | public API 100 req/phút/IP (ASP.NET Core Rate Limiting middleware)
