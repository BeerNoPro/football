---
paths:
  - "FootballBlog.API/**"
  - "FootballBlog.Web/**"
  - "FootballBlog.Core/**"
---

# Security Rules

## Authentication & Authorization
- Dùng ASP.NET Core Identity + JWT cho API
- Blazor SSR dùng Cookie authentication
- Role: Admin, Author, Reader
- Không hardcode role string — dùng constants

## Secrets Management
- KHÔNG commit connection string, API key, JWT secret lên git
- Local: dùng `dotnet user-secrets`
- Production: dùng AWS Secrets Manager hoặc environment variables
- File `.env` phải có trong `.gitignore`

## Input Validation
- Validate tất cả input từ user tại API boundary (FluentValidation)
- Sanitize HTML content của bài viết trước khi lưu (dùng HtmlSanitizer)
- Slug phải match regex `^[a-z0-9-]+$` — không nhận ký tự đặc biệt

## SQL Injection
- Luôn dùng EF Core parameterized queries
- Nếu phải dùng raw SQL: `FromSqlRaw` với parameters, KHÔNG string interpolation

## XSS
- Blazor tự escape output mặc định — không disable
- Nếu render raw HTML (bài viết): phải sanitize trước

## CSRF
- Blazor Server tự handle CSRF qua SignalR connection
- API endpoints dùng JWT — không cần CSRF token

## Rate Limiting
- Login endpoint: tối đa 5 lần/phút/IP
- Public API: 100 requests/phút/IP
- Dùng ASP.NET Core Rate Limiting middleware (.NET 7+)
