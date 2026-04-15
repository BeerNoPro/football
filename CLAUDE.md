# Football Blog — Claude Context

## Stack
ASP.NET Core 8 + Blazor (SSR + InteractiveServer) | PostgreSQL | Redis + SignalR | Hangfire | EF Core | Tailwind (public) | MudBlazor (admin)

## Architecture
- **4 projects**: Web → API → Core ← Infrastructure
- Web gọi API qua typed HttpClient (`IPostApiClient`, `ICategoryApiClient`)
- Repository chỉ modify ChangeTracker — commit duy nhất qua `IUnitOfWork.CommitAsync()`
- DTOs trong `Core/DTOs/` — KHÔNG expose entity ra ngoài service layer

## IUnitOfWork (quick ref)
```
uow.Posts | uow.Categories | uow.Tags | uow.LiveMatches
uow.Matches | uow.MatchPredictions
uow.Countries   — upsert by Code
uow.Leagues     — upsert by ExternalId
uow.Teams       — upsert by ExternalId
uow.MatchContexts — 1-to-1 với Match (JSONB)
```

## Dev Environment
- DB: `docker compose up` (postgres:5432, redis:6379)
- **Dev ports**: API `https://localhost:7007` | Web `https://localhost:7241`
- **EF migration**: `--project FootballBlog.Infrastructure --startup-project FootballBlog.API`
- Logs: `/logs/` — xem `.claude/rules/logging.md`
- Secrets: `dotnet user-secrets` (local) | AWS Parameter Store (prod)
- appsettings: xem `FootballBlog.API/appsettings.json` (đừng đọc cả file — grep key cần)

## Search & Context Optimization
- **Tìm file**: dùng `Glob` (pattern) — nhanh, ít token
- **Tìm nội dung**: dùng `Grep` (regex) — KHÔNG dùng Bash grep/rg
- **Đọc file**: `Grep` trước → `Read` với `offset`+`limit` chỉ đoạn cần — KHÔNG đọc cả file
- **HTML prototype**: chỉ đọc đến `<!-- STYLES -->` — bỏ qua `<style>` block
- **Context limit**: đóng conversation lúc 15-20 messages → `/cleanup` → tab mới

## Bug Fix Protocol
Khi fix bug: **LUÔN dùng `/fix-bug`** — KHÔNG sửa code trực tiếp từ mô tả lỗi.
Flow bắt buộc: Read log → Locate code → Propose (chờ approve) → Apply.

## Rules & Commands
- Code patterns: `.claude/rules/` (api, blazor, code-style, database, logging, security, testing, ui-design)
- Slash commands: `.claude/commands/` (api-client, blazor-page, cleanup, debug-log, docker, migration, new-feature, review, test)

## Cleanup Sau Khi Implement
Sau khi hoàn thành bất kỳ task/plan:
1. **Plan xong** → `rm .claude/plans/<file>.md` (không hỏi lại)
2. **Plan một phần** → ghi phần còn lại vào TODO.md rồi xóa plan
3. **Plan trùng** → giữ bản mới nhất, xóa bản cũ
4. Khi thêm config vào `.claude/` → kiểm tra trùng với CLAUDE.md/rules/ trước

## Current Phase
Xem **TODO.md** (phase + task). Xem **Bugs.md** (architectural decisions + known issues).
