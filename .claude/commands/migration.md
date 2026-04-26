# Tạo EF Core Migration

Khi được gọi với tên migration (e.g., `/migration AddPostTable`):

## Bước 1 — Xem thay đổi gần nhất

```bash
git diff HEAD -- FootballBlog.Infrastructure/Data/ FootballBlog.Core/Models/
```

Nếu chưa stage, xem cả unstaged:
```bash
git diff -- FootballBlog.Infrastructure/Data/ FootballBlog.Core/Models/
```

Giải thích ngắn: migration sẽ tạo ra thay đổi gì trong DB (bảng mới, cột mới, index, ...).

## Bước 2 — Kiểm tra migration cuối cùng

```bash
ls -t FootballBlog.Infrastructure/Migrations/*.cs | head -3
```

Đảm bảo migration mới sẽ kế thừa đúng snapshot hiện tại.

## Bước 3 — Tạo migration

```bash
dotnet ef migrations add {TênMigration} --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

## Bước 4 — Đọc và giải thích migration vừa tạo

```bash
ls -t FootballBlog.Infrastructure/Migrations/*.cs | head -1
```

Đọc file migration mới nhất, giải thích `Up()` / `Down()` làm gì.

## Bước 5 — Xác nhận trước khi apply

Hỏi user trước, sau khi xác nhận mới chạy:

```bash
dotnet ef database update --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

**Không tự động apply migration — luôn hỏi người dùng trước.**
