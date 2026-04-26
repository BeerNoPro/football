# Đọc và Phân Tích Log — UAT Investigation

Khi được gọi (`/debug-log [loại]`):

---

## Chọn log cần đọc

| Lệnh | File log |
|------|----------|
| `/debug-log` | `logs/app/app-{hôm nay}.log` |
| `/debug-log error` | `logs/error/error-{hôm nay}.log` |
| `/debug-log jobs` | `logs/jobs/jobs-{hôm nay}.log` |
| `/debug-log api` | `logs/api/api-{hôm nay}.log` |
| `/debug-log build` | `logs/build/build-{hôm nay}.log` |
| `/debug-log database` | `logs/database/db-{hôm nay}.log` ⚠️ xem bên dưới |

Nếu file chưa có → nhắc chạy app trước.

---

## Bước 1 — Lấy ngày và xác định file

```bash
date +%Y-%m-%d
```

Dùng kết quả để điền tên file log chính xác (ví dụ `logs/app/app-2026-04-25.log`).

---

## Bước 2 — Lọc lỗi từ log

```bash
grep -n "ERR\|FTL\|Exception" logs/app/app-2026-04-25.log | tail -80
```

RTK hook tự rewrite thành `rtk grep ...` → output được filter + compress, tiết kiệm token.

Xem cuối log (lỗi mới nhất):
```bash
tail -n 150 logs/app/app-2026-04-25.log
```

Lọc theo khoảng giờ cụ thể (khi user báo "lỗi lúc 2h chiều"):
```bash
grep "2026-04-25 14:" logs/app/app-2026-04-25.log | grep "ERR\|FTL\|Exception"
```

---

## Bước 3 — Đọc stacktrace đầy đủ quanh lỗi

```bash
grep -n -A 10 "ExceptionType\|message lỗi cụ thể" logs/app/app-2026-04-25.log | head -60
```

`-A 10` lấy 10 dòng sau match để có stacktrace. RTK compress output thừa tự động.

---

## Bước 4 — Trace về source code

Từ `file:line` trong stacktrace, tìm method bị lỗi:
```bash
grep -rn "TênMethod\|TênClass" FootballBlog.API/ FootballBlog.Core/ --include="*.cs" | head -20
```

Tìm callers của method đó:
```bash
grep -rn "TênMethod(" FootballBlog.API/ FootballBlog.Web/ --include="*.cs" | head -20
```

Xác định layer bị ảnh hưởng: Controller / Service / Repository / Job / Blazor Component.

---

## Bước 5 — Đối chiếu kiến trúc

Trước khi kết luận, kiểm tra:
- `Bugs.md` — lỗi này có phải architectural decision đã biết không?
- `CLAUDE.md` → IUnitOfWork, DTO pattern, phân tầng Web→API→Core←Infrastructure
- Rule tương ứng với layer bị lỗi:
  - Repository / EF → `.claude/rules/database.md`
  - Controller / API → `.claude/rules/api.md`
  - Blazor → `.claude/rules/blazor.md`
  - Logging → `.claude/rules/logging.md`

---

## ⚠️ db.log — chỉ đọc khi thực sự cần

`db.log` có thể đạt **800KB–1MB+/ngày** do bulk INSERT và query spam từ seeding job.

**Chỉ đọc db.log khi lỗi chứa:** `SQL` · `EF` · `DbUpdate` · `query` · `timeout` · `deadlock` · `migration` · `Cannot write DateTime`

**Nếu cần đọc db.log** — dùng grep có filter ketat, không đọc toàn file:
```bash
grep -n "SLOW\|ERR\|Exception" logs/database/db-2026-04-25.log | head -40
```

---

## Bước 6 — Map sang skill để fix

| Loại lỗi | Skill gợi ý |
|----------|-------------|
| Bug trong code hiện có | `/fix-bug <stacktrace>` |
| Thiếu API endpoint | `/api-client` |
| Thiếu Blazor page / component | `/blazor-page` |
| Cần EF migration | `/migration` |
| Cần feature mới hoàn toàn | `/new-feature` |

---

## Output bắt buộc

```
## Log Analysis — {loại} — {ngày}

### Lỗi tìm thấy

**[1] {ExceptionType}: {message}**
- Timestamp: ...
- Location: `File.cs:line`
- Pattern: xuất hiện X lần (lần đầu HH:mm, lần cuối HH:mm)
- Trigger: (request nào? job nào? user action nào?)

### Root cause (sơ bộ)
<giải thích ngắn dựa trên trace>

### Layer bị ảnh hưởng
<Controller / Service / Repository / Job / Blazor>

### Bước tiếp theo
> Dùng `/fix-bug` với stacktrace trên để phân tích và fix an toàn.
```

---

## Nguyên tắc bất biến

- **KHÔNG tự ý sửa code** khi chỉ đọc log
- Luôn chuyển sang `/fix-bug` nếu muốn fix — không shortcut
- Nếu có nhiều lỗi → ưu tiên theo mức độ: `FTL` > `ERR` > lỗi lặp nhiều lần
- Tất cả lệnh `grep` / `tail` / `cat` đều tự qua RTK hook — không cần làm gì thêm
