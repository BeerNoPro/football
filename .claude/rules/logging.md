# Logging Rules & Structure

## Thư viện
- Dùng **Serilog** (không dùng NLog hay log4net)
- Sink: Console (dev) + File (tất cả môi trường) + Seq (optional, local debug)
- Inject ILogger<T> qua DI, không dùng Log.Logger static trực tiếp

## Cấu trúc thư mục Log
```
logs/
├── app/
│   ├── app-20260325.log        ← log chung toàn app (Information+)
│   └── app-20260324.log
├── error/
│   ├── error-20260325.log      ← chỉ Error + Fatal
│   └── error-20260324.log
├── api/
│   ├── api-20260325.log        ← HTTP request/response log
│   └── api-20260324.log
└── jobs/
    ├── jobs-20260325.log       ← Hangfire background jobs
    └── jobs-20260324.log
```
Thêm `logs/` vào `.gitignore`

## Log Levels — dùng đúng level
| Level | Dùng khi nào |
|-------|-------------|
| Verbose | Trace chi tiết, chỉ bật khi debug sâu |
| Debug | Thông tin dev cần khi debug (giá trị biến, flow) |
| Information | Sự kiện bình thường (user login, post created) |
| Warning | Tình huống bất thường nhưng không crash (retry, fallback) |
| Error | Lỗi có thể recover được (DB timeout, API fail) |
| Fatal | Lỗi làm app không thể tiếp tục |

## Output Template chuẩn
```
[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}
```

## Structured Logging — dùng property thay string concat
```csharp
// ĐÚNG — searchable, filterable
_logger.LogInformation("Post created {@Post}", new { Id = post.Id, Slug = post.Slug });

// SAI — không filter được
_logger.LogInformation("Post created: " + post.Id + " - " + post.Slug);
```

## Các điểm BẮT BUỘC phải log

### API Layer
```csharp
// Request/Response — dùng middleware, không log thủ công từng endpoint
// Serilog RequestLogging middleware tự xử lý
```

### Service Layer
```csharp
// Log khi bắt đầu operation quan trọng
_logger.LogDebug("Getting post by slug {Slug}", slug);

// Log khi thành công (Information cho user-facing actions)
_logger.LogInformation("Post {PostId} published by {UserId}", postId, userId);

// Log warning khi không tìm thấy
_logger.LogWarning("Post not found for slug {Slug}", slug);
```

### Error Handling
```csharp
try { ... }
catch (Exception ex)
{
    // Luôn include exception object — Serilog tự format stack trace
    _logger.LogError(ex, "Failed to create post for user {UserId}", userId);
    throw; // re-throw hoặc return error response
}
```

### Hangfire Jobs
```csharp
// Log đầu và cuối mỗi job
_logger.LogInformation("LiveScore polling job started at {Time}", DateTime.UtcNow);
_logger.LogInformation("LiveScore polling job completed. Matches updated: {Count}", count);
```

## Cấu hình Serilog trong appsettings.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Hangfire": "Information",
        "System": "Warning"
      }
    }
  }
}
```
- Override EF Core xuống Warning để tránh spam SQL queries log
- Override Microsoft.* xuống Warning để log sạch hơn

## appsettings theo môi trường
- `appsettings.Development.json` → MinimumLevel: Debug, log ra Console + File
- `appsettings.Production.json` → MinimumLevel: Information, log ra File + Error sink riêng
