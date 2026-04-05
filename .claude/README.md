# Claude Code — Cấu hình & Hướng dẫn

## Cài đặt

```bash
npm install -g @anthropic-ai/claude-code
claude
```

---

## Files quan trọng

| File | Mục đích |
|------|---------|
| `CLAUDE.md` | Context tổng quan — Claude đọc mỗi session |
| `TODO.md` | Roadmap và task hiện tại theo phase |
| `Bugs.md` | Architectural decisions, known issues |
| `.claude/rules/` | Coding rules — Claude tuân theo khi generate code |
| `.claude/commands/` | Custom slash commands |
| `.claude/settings.json` | Hooks và permissions (commit vào git) |
| `.claude/settings.local.json` | Settings local — KHÔNG commit |
| `.claude/plans/` | Implementation plans (xoá sau khi implement xong) |

---

## Hooks đã cấu hình

Hooks chạy tự động sau mỗi tool call. Xem/sửa tại `.claude/settings.json`.

### Hook 1 — Auto Build Check

**Trigger:** Sau mỗi lần Claude edit/write file `.cs`  
**Tác dụng:** Chạy `dotnet build --no-restore -v q`, hiển thị output nếu có lỗi

```
[Hook] Build check after edit...
Build succeeded.        ← hoặc: error CS0246: Type 'Foo' not found
```

### Hook 2 — DbContext Migration Reminder

**Trigger:** Sau khi Claude sửa file có `DbContext` trong tên  
**Tác dụng:** Nhắc tạo EF migration

```
DbContext changed — remember to add EF migration:
  dotnet ef migrations add <Name> --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

### Hook 3 — Stop Notification

**Trigger:** Khi Claude dừng làm việc  
**Tác dụng:** Nhắc verify build trước khi tiếp tục

```
Claude stopped. Verify: dotnet build --no-restore -v q
```

### Vô hiệu hoá hook tạm thời

```json
// .claude/settings.json
"disableAllHooks": true
```

---

## Custom Slash Commands

| Command | Tác dụng |
|---------|---------|
| `/new-feature` | Scaffold feature mới theo đúng cấu trúc project |
| `/migration` | Hướng dẫn tạo EF Core migration |
| `/review` | Review code trước khi commit |
| `/test` | Chạy tests |
| `/debug-log` | Đọc và phân tích log files |
| `/blazor-page` | Tạo Blazor SSR page mới |
| `/api-client` | Tạo typed API client |
| `/cleanup` | Dọn dẹp context sau khi implement xong |

**Ví dụ:**
```
/new-feature Tạo trang danh sách bài viết theo tag
/review FootballBlog.Core/Services/PostService.cs
/migration Thêm cột ViewCount vào bảng Posts
/blazor-page LiveScore/MatchDetail
```

---

## Rules files

| File | Phạm vi áp dụng |
|------|----------------|
| `rules/code-style.md` | Naming, async, DI, error handling |
| `rules/database.md` | EF Core, repository, AsNoTracking, naming |
| `rules/api.md` | REST conventions, response format, SignalR, Hangfire |
| `rules/blazor.md` | Render mode rules, SSR vs InteractiveServer |
| `rules/logging.md` | Serilog structured logging, log levels |
| `rules/security.md` | Auth, secrets, input validation, rate limiting |
| `rules/testing.md` | Test conventions |

---

## Memory

Claude lưu thông tin xuyên session trong `~/.claude/projects/.../memory/`.  
Xem index tại `~/.claude/projects/.../memory/MEMORY.md`.

---

## Plans

Khi plan xong một feature, Claude tạo file trong `.claude/plans/`.  
**Sau khi implement xong → xoá file plan** (source code là source of truth).  
Nếu implement một phần → ghi phần còn lại vào `TODO.md` rồi xoá plan.

---

## Tips

- Dùng `/review` trước mỗi commit để Claude check lỗi logic + security
- Dùng `/debug-log` khi có lỗi runtime — Claude đọc log và trace nguyên nhân  
- Với task lớn (implement cả phase), dùng Plan mode: `Ctrl+Shift+P` → "Enter Plan Mode"
