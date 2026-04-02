---
paths:
  - "**/*.cs"
---

# Logging Rules

- Dùng **Serilog** | inject `ILogger<T>` qua DI | KHÔNG dùng `Log.Logger` static
- Sinks: Console + File (`logs/app/`, `logs/error/`, `logs/api/`, `logs/jobs/`) | rolling daily
- Structured logging: `_logger.LogInformation("Post created {@Post}", new { post.Id, post.Slug })` — KHÔNG string concat

## Bắt buộc log
- Service: `LogDebug` khi bắt đầu | `LogInformation` khi thành công | `LogWarning` khi not found
- Error: `_logger.LogError(ex, "...", args)` — luôn truyền exception object, sau đó `throw`
- Hangfire job: log đầu và cuối job kèm số lượng record xử lý

## Levels
Debug=flow dev | Information=user-facing action | Warning=bất thường không crash | Error=có thể recover | Fatal=app chết

## appsettings
Override: `Microsoft.*` → Warning | `EF Core` → Warning | `Hangfire` → Information (tránh spam)
