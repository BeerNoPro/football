---
paths:
  - "FootballBlog.API/**"
---

# ASP.NET Core API

## REST + Response
- Routes: `GET /api/posts`, `GET /api/posts/{slug}`, `POST /api/posts`, `PUT /api/posts/{id}`, `DELETE /api/posts/{id}`
- Luôn wrap: `record ApiResponse<T>(bool Success, T? Data, string? Error = null)`

## Validation
- FluentValidation tại controller boundary | trả 400 với danh sách lỗi

## SignalR Hub
- Kế thừa `Hub<IClientInterface>` (strongly typed) | group theo match: `$"match-{matchId}"`

## Hangfire Jobs
- Idempotent | log đầu + cuối job | retry tối đa 3 lần, backoff 5 phút

## Security
- Admin endpoint: `[Authorize(Roles = "Admin")]` | rate limiting public endpoints | CORS chỉ allow domain cụ thể
