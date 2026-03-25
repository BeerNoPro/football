# Tạo Feature Mới

Khi được gọi với tên feature (e.g., `/new-feature LiveScore`):

1. Hỏi xác nhận scope: feature này thuộc tầng nào? (Blog public / Admin / Realtime)
2. Tạo các files theo đúng project structure:

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

3. Đăng ký DI trong Program.cs
4. Tạo EF migration nếu có entity mới (gợi ý dùng /migration)
5. Ghi lại task vào TODO.md phase tương ứng
