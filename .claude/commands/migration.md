# Tạo EF Core Migration

Khi được gọi với tên migration (e.g., `/migration AddPostTable`):

1. Đọc các thay đổi gần nhất trong `FootballBlog.Infrastructure/Data/` và `FootballBlog.Core/Models/`
2. Giải thích ngắn gọn migration sẽ tạo ra thay đổi gì trong DB
3. Chạy lệnh:
   ```
   dotnet ef migrations add {TênMigration} --project FootballBlog.Infrastructure --startup-project FootballBlog.Web
   ```
4. Đọc file migration vừa tạo và giải thích Up() / Down() làm gì
5. Hỏi xác nhận trước khi apply: `dotnet ef database update --project FootballBlog.Infrastructure --startup-project FootballBlog.Web`

Lưu ý: Không tự động apply migration, luôn hỏi người dùng trước.
