---
paths:
  - "FootballBlog.API/**"
---

# ASP.NET Core API Rules

## REST Conventions
- GET /api/posts — danh sách (có pagination)
- GET /api/posts/{slug} — chi tiết
- POST /api/posts — tạo mới
- PUT /api/posts/{id} — cập nhật toàn bộ
- PATCH /api/posts/{id} — cập nhật một phần
- DELETE /api/posts/{id} — xóa

## Response Format chuẩn
```csharp
// Luôn wrap response trong ApiResponse<T>
public record ApiResponse<T>(bool Success, T? Data, string? Error = null);

// Success
return Ok(new ApiResponse<PostDto>(true, postDto));

// Error
return NotFound(new ApiResponse<PostDto>(false, null, "Post not found"));
```

## Validation
- Dùng FluentValidation, không validate thủ công trong controller
- Trả về 400 với danh sách lỗi rõ ràng khi validation fail

## SignalR Hub
- Hub class kế thừa Hub<IClientInterface> — strongly typed
- Method name: PascalCase (e.g., `SendLiveScore`, `UpdateMatchEvent`)
- Luôn handle connection/disconnection events
- Group theo match ID: `await Groups.AddToGroupAsync(connectionId, $"match-{matchId}")`

## Hangfire Jobs
- Job class implement interface, đăng ký qua DI
- Idempotent: chạy lại nhiều lần không gây lỗi
- Log đầu và cuối job (xem logging.md)
- Retry policy: tối đa 3 lần, backoff 5 phút

## Security
- Endpoint admin: require role "Admin" với [Authorize(Roles = "Admin")]
- Rate limiting cho public endpoints
- CORS chỉ allow domain cụ thể trên production
