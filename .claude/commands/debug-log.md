# Đọc và Phân Tích Log — UAT Investigation

Khi được gọi (`/debug-log [loại]`):

## Chọn log cần đọc

| Lệnh | File |
|------|------|
| `/debug-log` | `logs/app/app-{hôm nay}.log` |
| `/debug-log error` | `logs/error/web-error-{hôm nay}.log` |
| `/debug-log jobs` | `logs/jobs/jobs-{hôm nay}.log` |
| `/debug-log api` | `logs/api/api-{hôm nay}.log` |
| `/debug-log build` | `logs/build/build-{hôm nay}.log` |

Nếu file chưa có → nhắc chạy app trước.

---

## Quy trình phân tích (UAT mode)

### Bước 1 — Lọc và phân loại lỗi

1. Đọc toàn bộ log, lọc dòng `ERR` / `FTL` — bỏ qua `INF` / `DBG` trừ khi liên quan trực tiếp
2. Với mỗi lỗi tìm được, output đầy đủ:
   - **Timestamp** — khi nào xảy ra
   - **Exception** — type + message
   - **Location** — file:line từ stacktrace
   - **Pattern** — lỗi lặp lại mấy lần? có theo trigger nào không?

### Bước 2 — Trace qua hệ thống

Từ file:line trong stacktrace, truy ngược luồng dữ liệu:
```
Controller → Service → Repository → DB / External API
```
- Dùng `Grep` tìm method/class bị lỗi
- Tìm **callers** của method đó (ai gọi nó?)
- Xác định layer bị ảnh hưởng: Controller / Service / Repository / Job / Blazor Component

### Bước 3 — Đối chiếu với kiến trúc hệ thống

Trước khi kết luận, kiểm tra:
- `Bugs.md` — lỗi này có phải architectural decision đã biết không?
- `CLAUDE.md` — IUnitOfWork, DTO pattern, phân tầng Web→API→Core←Infrastructure
- Rule tương ứng với layer bị lỗi:
  - Repository / EF → `.claude/rules/database.md`
  - Controller / API → `.claude/rules/api.md`
  - Blazor → `.claude/rules/blazor.md`
  - Logging → `.claude/rules/logging.md`

### Bước 4 — Xác định skill phù hợp để fix

Sau khi hiểu rõ lỗi, map sang skill tương ứng:

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
