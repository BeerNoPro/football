---
paths:
  - "FootballBlog.Infrastructure/**"
  - "FootballBlog.Core/Models/**"
  - "FootballBlog.Core/Interfaces/**"
---

# Database & EF Core

- Code-First migrations | migration name mô tả thay đổi (`AddPostSlugIndex`) | review trước khi apply
- Repository: interface `IXxxRepository` + implementation | trả `IEnumerable`/`List`, KHÔNG expose `IQueryable`
- Read-only queries: `.AsNoTracking()` + `.Select()` chỉ lấy fields cần | pagination bắt buộc (Skip/Take)
- DB naming: bảng snake_case số nhiều (`live_matches`) | cột snake_case (`published_at`) | FK `{table}_id`
- Transactions: dùng `IUnitOfWork.CommitAsync()` — KHÔNG gọi `SaveChangesAsync()` từ repository
- KHÔNG gọi DbContext trực tiếp từ tầng Web — chỉ qua Repository/Service
