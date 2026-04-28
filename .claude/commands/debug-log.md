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

---

## Bước 2 — Lọc và đọc log

```bash
# Filter + deduplicate log, chỉ giữ dòng quan trọng
rtk log logs/app/app-2026-04-26.log

# Tìm lỗi theo pattern cụ thể, grouped by file
rtk grep "ERR|FTL|Exception" logs/app/app-2026-04-26.log

# Lọc theo giờ cụ thể (khi user báo "lỗi lúc 2h chiều")
rtk grep "2026-04-26 14:" logs/app/app-2026-04-26.log
```

---

## Bước 3 — Đọc stacktrace đầy đủ quanh lỗi

```bash
# Xem context ±5 dòng quanh exception
rtk grep "ExceptionType|message lỗi cụ thể" logs/app/app-2026-04-26.log
```

---

## Bước 4 — Trace về source code

Từ `file:line` trong stacktrace:

```bash
# Tìm class/method bị lỗi — grouped by file, compact output
rtk grep "TênClass|TênMethod" FootballBlog.API/

# Tìm callers
rtk grep "TênMethod(" FootballBlog.API/ FootballBlog.Web/ FootballBlog.Core/

# Xem cấu trúc file (chỉ signatures, không load body)
rtk read FootballBlog.API/Path/To/File.cs -l aggressive

# Tóm tắt nhanh 2 dòng một file
rtk smart FootballBlog.API/Path/To/File.cs
```

Xác định layer bị ảnh hưởng: Controller / Service / Repository / Job / Blazor Component.

---

## Bước 5 — Đối chiếu kiến trúc

```bash
rtk grep "TênMethod|TênClass" Bugs.md
```

Kiểm tra rule tương ứng với layer:
- Repository / EF → `.claude/rules/database.md`
- Controller / API → `.claude/rules/api.md`
- Blazor → `.claude/rules/blazor.md`
- Logging → `.claude/rules/logging.md`

---

## ⚠️ db.log — chỉ đọc khi thực sự cần

Đạt **800KB–1MB+/ngày** do bulk INSERT từ seeding job.

**Chỉ đọc khi lỗi chứa:** `SQL` · `EF` · `DbUpdate` · `query` · `timeout` · `deadlock` · `migration` · `Cannot write DateTime`

```bash
# Filter aggressively — không đọc toàn file
rtk grep "SLOW|ERR|Exception" logs/database/db-2026-04-26.log
```

---

## Bước 6 — Dọn Hangfire stale jobs (nếu lỗi là Hangfire signature mismatch)

Khi log chứa `does not contain a method with signature` → job cũ trong DB không khớp method hiện tại.
**Xóa trước khi trigger lại** — không chờ retry tự hết (mỗi retry chiếm worker slot):

```bash
# Tìm container postgres đang chạy
docker ps --format "{{.Names}}\t{{.Image}}"

# Xóa stale jobs theo id (lấy id từ log: "Failed to process the job '371'")
docker exec -i <container-name> psql -U <db-user> -d footballblog \
  -c "DELETE FROM hangfire.job WHERE id IN (371, 372) RETURNING id, statename;"
```

Sau khi xóa → rebuild app → trigger lại từ Admin UI.

---

## Bước 7 — Map sang skill để fix

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
