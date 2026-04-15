# Đọc và Phân Tích Log

Khi được gọi (`/debug-log [loại]`):

## Chọn log cần đọc

| Lệnh | File |
|------|------|
| `/debug-log` | `logs/app/app-{hôm nay}.log` |
| `/debug-log error` | `logs/error/error-{hôm nay}.log` |
| `/debug-log jobs` | `logs/jobs/jobs-{hôm nay}.log` |
| `/debug-log api` | `logs/api/api-{hôm nay}.log` |

Nếu file chưa có → nhắc chạy app trước.

## Phân tích log

1. Lọc các dòng `ERR` / `FTL` — bỏ qua `INF` / `DBG` trừ khi liên quan trực tiếp
2. Với mỗi lỗi tìm được, output:
   - **Timestamp** — khi nào xảy ra
   - **Exception** — type + message
   - **Location** — file:line từ stacktrace
   - **Pattern** — lỗi lặp lại mấy lần?

3. Nếu tìm thấy lỗi cần fix → gợi ý dùng `/fix-bug` với stacktrace đó:
   > "Tìm thấy lỗi tại `XxxService.cs:42`. Dùng `/fix-bug` để phân tích và fix an toàn."

**KHÔNG tự ý sửa code** khi chỉ đọc log — chuyển sang `/fix-bug` nếu muốn fix.
