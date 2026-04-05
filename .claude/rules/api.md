---
paths:
  - "FootballBlog.API/**"
---

# ASP.NET Core API

## REST + Response
- Luôn wrap: `record ApiResponse<T>(bool Success, T? Data, string? Error = null)`
- Slug-based GET dùng `{slug}`, id-based PUT/DELETE dùng `{id}`
- Pagination: nhận `page` + `pageSize`, trả `PagedResult<T>`

## Validation
- FluentValidation tại controller boundary | trả 400 với danh sách lỗi

## SignalR Hub
- Kế thừa `Hub<IClientInterface>` (strongly typed) | group theo match: `$"match-{matchId}"`

## Hangfire Jobs
- Idempotent | log đầu + cuối job | retry tối đa 3 lần, backoff 5 phút
- Đăng ký trong Program.cs với `RecurringJob.AddOrUpdate`

## Security
- Admin endpoint: `[Authorize(Roles = "Admin")]`
- Rate limiting public endpoints
- CORS chỉ allow origin từ `WebBaseUrl` trong appsettings
