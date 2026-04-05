# Docker Dev Environment

Khi được gọi (`/docker`, `/docker up`, `/docker status`, `/docker reset`):

## `/docker` hoặc `/docker status`
Kiểm tra trạng thái services:
```bash
docker compose ps
docker compose logs --tail=20 postgres
docker compose logs --tail=20 redis
```
Báo cáo: postgres và redis có healthy không, port có đúng không (5432, 6379).

## `/docker up`
Khởi động services:
```bash
docker compose up -d
```
Sau đó chờ 3 giây và kiểm tra health:
```bash
docker compose ps
```
Nếu postgres chưa healthy → hiển thị logs để debug.

## `/docker reset`
**Cảnh báo trước:** lệnh này xóa toàn bộ dữ liệu DB local.
Chỉ thực hiện khi user xác nhận:
```bash
docker compose down -v
docker compose up -d
```
Sau reset → nhắc chạy migration:
```
dotnet ef database update --project FootballBlog.Infrastructure --startup-project FootballBlog.API
```

## Ghi chú
- Postgres: `localhost:5432` | Redis: `localhost:6379`
- Connection string lấy từ `dotnet user-secrets` — KHÔNG hardcode
- Nếu port conflict → kiểm tra process đang dùng port: `netstat -ano | findstr :5432`
