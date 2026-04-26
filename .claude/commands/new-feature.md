# Tạo Feature Mới

Khi được gọi với tên feature (e.g., `/new-feature LiveScore`):

## Bước 1 — Xác nhận scope

Hỏi:
- Feature thuộc tầng nào? (Blog public / Admin / Realtime)
- Có entity DB mới không? → cần migration sau
- Có API endpoint mới không? → cần api-client sau

## Bước 2 — Kiểm tra files đã tồn tại

```bash
find . -name "*{Feature}*" -not -path "*/bin/*" -not -path "*/obj/*" | head -20
```

Tránh tạo trùng — xem những gì đã có trước.

## Bước 3 — Kiểm tra DI registrations hiện tại

```bash
grep -n "AddScoped\|AddSingleton\|AddTransient\|AddHttpClient" FootballBlog.API/Program.cs | tail -30
grep -n "AddScoped\|AddSingleton\|AddTransient\|AddHttpClient" FootballBlog.Web/Program.cs | tail -30
```

## Bước 4 — Tạo files theo project structure

**Nếu là Blog/Admin feature:**
- `FootballBlog.Core/Models/{Feature}.cs` — Entity model
- `FootballBlog.Core/Interfaces/I{Feature}Repository.cs` — Repository interface
- `FootballBlog.Core/Services/{Feature}Service.cs` — Business logic
- `FootballBlog.Infrastructure/Repositories/{Feature}Repository.cs` — EF Core implementation
- `FootballBlog.Web/Components/Pages/{Feature}/{Feature}Page.razor` — Blazor page

**Nếu là Realtime feature:**
- Model + Repository + Service như trên
- `FootballBlog.API/Hubs/{Feature}Hub.cs` — SignalR Hub
- `FootballBlog.API/Jobs/{Feature}Job.cs` — Hangfire background job
- Blazor component với `@rendermode InteractiveServer`

## Bước 5 — Đăng ký DI trong Program.cs

Thêm vào đúng Program.cs của project liên quan.

## Bước 6 — Ghi task còn lại

```bash
grep -n "Phase" TODO.md | tail -10
```

Thêm subtask vào TODO.md phase tương ứng.

## Bước 7 — Gợi ý bước tiếp theo

- Có entity mới → `/migration Add{Feature}Table`
- Có API endpoint mới → `/api-client {Feature}`
- Cần Blazor page → `/blazor-page {Feature}`
