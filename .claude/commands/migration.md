# Tạo EF Core Migration

Khi được gọi với tên migration (e.g., `/migration AddPostTable`):

## Bước 1 — Xem thay đổi gần nhất

```bash
rtk git diff HEAD -- FootballBlog.Infrastructure/Data/ FootballBlog.Core/Models/
```

Nếu chưa stage:
```bash
rtk git diff -- FootballBlog.Infrastructure/Data/ FootballBlog.Core/Models/
```

Giải thích ngắn: migration sẽ tạo ra thay đổi gì trong DB (bảng mới, cột mới, index, ...).

## Bước 2 — Kiểm tra migration cuối cùng

```bash
rtk ls FootballBlog.Infrastructure/Migrations/
```

Đảm bảo migration mới sẽ kế thừa đúng snapshot hiện tại.

## Bước 3 — Tạo migration

```bash
rtk dotnet ef migrations add {TênMigration} --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

## Bước 4 — Đọc và giải thích migration vừa tạo

```bash
rtk ls FootballBlog.Infrastructure/Migrations/
```

Đọc file migration mới nhất (dùng `rtk read` hoặc `rtk smart`), giải thích `Up()` / `Down()` làm gì.

## Bước 5 — Xác nhận trước khi apply

Hỏi user trước, sau khi xác nhận mới chạy:

```bash
rtk dotnet ef database update --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

**Không tự động apply migration — luôn hỏi người dùng trước.**
