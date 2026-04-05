# Development Guide

## Yêu cầu môi trường

| Tool | Version | Kiểm tra |
|------|---------|---------|
| .NET SDK | 8.x | `dotnet --version` |
| Docker Desktop | 4.x+ (WSL2 trên Windows) | `docker --version` |
| Node.js | 18+ | `node --version` |
| EF Core CLI | latest | `dotnet ef --version` |

**Cài EF Core CLI (một lần):**
```bash
dotnet tool install --global dotnet-ef
```

---

## Cài đặt & Chạy lần đầu

### 1. Clone

```bash
git clone <repo-url>
cd football
```

### 2. Khởi động database local

```bash
docker compose up -d
```

Kiểm tra:
```bash
docker ps
# football-postgres-1   Up   0.0.0.0:5432->5432/tcp
# football-redis-1      Up   0.0.0.0:6379->6379/tcp
```

Kết nối DBeaver:
```
Host: localhost | Port: 5432 | Database: footballblog | User: admin | Password: localpass
```

### 3. Apply migrations

```bash
dotnet ef database update \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### 4. Build Tailwind CSS (Phase 2+)

```bash
cd FootballBlog.Web
npm install
npm run build:css
```

### 5. Chạy

```bash
# Terminal 1
dotnet run --project FootballBlog.API
# API:    https://localhost:7007
# Swagger: https://localhost:7007/swagger
# Health: https://localhost:7007/health

# Terminal 2
dotnet run --project FootballBlog.Web
# Web: https://localhost:7241
```

---

## Migrations

### Tạo migration mới

```bash
dotnet ef migrations add <TenMigration> \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### Apply

```bash
dotnet ef database update \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### Rollback

```bash
# Rollback về migration trước
dotnet ef database update <TenMigrationTruoc> \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API

# Xoá migration cuối (chưa apply)
dotnet ef migrations remove \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API
```

### Xem SQL trước khi apply (khuyến nghị cho prod)

```bash
dotnet ef migrations script \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API \
  --output migration.sql
```

### Schema hiện tại

| Table | Mô tả |
|-------|-------|
| `Posts` | Bài viết — `PublishedAt` null = draft, không lộ ra public |
| `Categories` | Danh mục — slug unique |
| `Tags` | Tag — slug unique |
| `PostTags` | Many-to-many Posts ↔ Tags |
| `AspNetUsers` | Identity users (Admin/Author) |
| `LiveMatches` | Trận đang live — FK → `Matches` (nullable) |
| `MatchEvents` | Sự kiện trận (goal/card/sub) — FK → `LiveMatches` |
| `Matches` | Lịch thi đấu từ Football API — FK → `MatchPredictions` |
| `MatchPredictions` | Kết quả AI dự đoán — FK → `Posts` (khi publish) |

---

## Lệnh hay dùng

### Build

```bash
dotnet build                      # Build toàn solution
dotnet build --no-restore -v q   # Build nhanh, ít output
```

### Hot reload

```bash
dotnet watch --project FootballBlog.Web
dotnet watch --project FootballBlog.API
```

### Docker

```bash
docker compose up -d              # Khởi động
docker compose down               # Dừng
docker compose down -v            # Dừng + xoá data (reset DB)
docker compose logs -f postgres   # Xem log postgres
```

### Tailwind

```bash
cd FootballBlog.Web
npm run build:css    # Build 1 lần
npm run watch:css    # Watch mode (dev)
```

### Logs

Logs được Serilog ghi vào `logs/` tại solution root:

```
logs/
├── app/app-YYYYMMDD.log      # Information+
├── error/error-YYYYMMDD.log  # Error + Fatal
├── api/api-YYYYMMDD.log      # HTTP requests
└── jobs/jobs-YYYYMMDD.log    # Hangfire jobs (Phase 4+)
```

```bash
# Xem log realtime (bash/WSL)
tail -f logs/app/app-$(date +%Y%m%d).log

# Tìm lỗi
grep "ERR\|FTL" logs/app/app-$(date +%Y%m%d).log
```

---

## Coding Conventions

### Async

```csharp
// ĐÚNG
public async Task<Post?> GetBySlugAsync(string slug)
    => await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);

// SAI — gây deadlock
var post = repository.GetBySlugAsync(slug).Result;
```

### Logging (structured)

```csharp
// ĐÚNG — searchable, filterable
_logger.LogInformation("Post created {@Post}", new { post.Id, post.Slug });
_logger.LogWarning("Post not found: {Slug}", slug);

// SAI — không query được
_logger.LogInformation("Post: " + post.Id + " " + post.Slug);
```

### Repository pattern

```csharp
// Read-only: bắt buộc AsNoTracking
return await _dbSet
    .AsNoTracking()
    .Include(p => p.Category)
    .Where(p => p.PublishedAt != null)
    .ToListAsync();

// Write: chỉ modify ChangeTracker, KHÔNG SaveChanges trong repository
await _dbSet.AddAsync(entity);
// Caller gọi: await uow.CommitAsync();
```

### Controller pattern

```csharp
var result = await _service.GetBySlugAsync(slug);
if (result == null)
    return NotFound(ApiResponse<PostDetailDto>.Fail($"Post '{slug}' not found"));
return Ok(ApiResponse<PostDetailDto>.Ok(result));
```
