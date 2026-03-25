---
paths:
  - "FootballBlog.Infrastructure/**"
  - "FootballBlog.Core/Models/**"
---

# Database & EF Core Rules

## Entity Framework Core
- Dùng Code-First migrations, không sửa DB trực tiếp
- Migration name phải mô tả rõ thay đổi (e.g., `AddPostSlugIndex`, `CreateLiveMatchTable`)
- Luôn review migration file trước khi apply
- Không gọi DbContext trực tiếp từ tầng Web — phải qua Repository

## Repository Pattern
- Mỗi entity có interface IXxxRepository và implementation XxxRepository
- Generic repository cho CRUD cơ bản, specific repository cho query phức tạp
- Trả về IEnumerable hoặc List, không expose IQueryable ra ngoài tầng Infrastructure

## Query Performance
- Dùng `.AsNoTracking()` cho read-only queries
- Dùng `.Select()` để chỉ lấy fields cần thiết, tránh SELECT *
- Pagination bắt buộc cho list queries (Skip/Take)
- Index các cột: slug, published_at, category_id

## Naming Convention (DB)
- Tên bảng: snake_case, số nhiều (posts, categories, live_matches)
- Tên cột: snake_case (created_at, published_at, home_team)
- Primary key: id (int hoặc Guid tùy entity)
- Foreign key: {table_singular}_id (e.g., post_id, category_id)

## Connection & Transactions
- Dùng connection string từ IConfiguration, không hardcode
- Unit of Work pattern cho operations cần transaction
