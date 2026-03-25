# Đọc và Phân Tích Log

Khi được gọi (`/debug-log` hoặc `/debug-log error`):

1. Xác định loại log cần xem:
   - `/debug-log` → đọc log chung hôm nay
   - `/debug-log error` → chỉ đọc error log
   - `/debug-log jobs` → log Hangfire jobs
   - `/debug-log api` → log HTTP requests

2. Đọc file log tương ứng (log hôm nay):
```
logs/app/app-{ngày hôm nay}.log
logs/error/error-{ngày hôm nay}.log
logs/api/api-{ngày hôm nay}.log
logs/jobs/jobs-{ngày hôm nay}.log
```

3. Phân tích và tóm tắt:
   - Có Error hoặc Fatal nào không?
   - Pattern lỗi lặp lại?
   - Timestamp để xác định thời điểm xảy ra
   - Gợi ý nguyên nhân và hướng fix

4. Nếu log chưa có (app chưa chạy) → nhắc chạy app trước
