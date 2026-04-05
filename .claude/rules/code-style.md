---
paths:
  - "**/*.cs"
  - "**/*.razor"
  - "**/*.razor.cs"
---

# C# Code Style

- Naming: Class/Method/Property → PascalCase | variable/param → camelCase | interface → `IFoo` | private field → `_foo`
- Async: tất cả DB+HTTP dùng `async/await` với hậu tố `Async` — KHÔNG `.Result`/`.Wait()`
- DI: inject qua constructor | không service locator
- Error: try/catch ở controller boundary | log bằng `ILogger<T>`, không `Console.WriteLine`
- Comments: tiếng Việt cho logic phức tạp | XML `///` cho public API methods
- Misc: không `var` khi kiểu mơ hồ | expression body cho 1 dòng | xóa unused using/dead code

## Project-Specific Patterns

**Service method chuẩn:**
```csharp
public async Task<PostDetailDto?> GetBySlugAsync(string slug)
{
    _logger.LogDebug("Getting post by slug {Slug}", slug);
    var post = await _uow.Posts.GetBySlugAsync(slug);
    if (post == null) { _logger.LogWarning("Post not found: {Slug}", slug); return null; }
    _logger.LogInformation("Post retrieved: {Slug}", slug);
    return post.ToDetailDto();
}
```

**Repository read-only (bắt buộc AsNoTracking):**
```csharp
return await _context.Posts
    .AsNoTracking()
    .Include(p => p.Category)
    .Where(p => p.PublishedAt != null)
    .ToListAsync();
```

**Controller action chuẩn:**
```csharp
var result = await _service.GetBySlugAsync(slug);
if (result == null) return NotFound(new ApiResponse<PostDetailDto>(false, null, "Not found"));
return Ok(new ApiResponse<PostDetailDto>(true, result));
```
