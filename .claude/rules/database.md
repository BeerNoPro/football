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

## Query Tagging (bắt buộc)

Mọi custom LINQ query trong repository **phải có `.TagWithCaller()` trước terminal method** để db.log ghi rõ caller khi điều tra bug:

```csharp
// Pattern chuẩn — TagWithCaller ngay trước ToListAsync / FirstOrDefaultAsync / AnyAsync / CountAsync
return await _dbSet
    .AsNoTracking()
    .Where(...)
    .OrderBy(...)
    .TagWithCaller()        // ← bắt buộc, compile-time only, zero runtime overhead
    .ToListAsync();

// CountAsync với predicate — TagWithCaller trước CountAsync
await _dbSet.TagWithCaller().CountAsync(p => p.PublishedAt != null);

// KHÔNG áp dụng cho FindAsync (không phải IQueryable)
await _dbSet.FindAsync(id);  // ← giữ nguyên
```

Extension method tại: `FootballBlog.Infrastructure/Data/QueryableExtensions.cs`
