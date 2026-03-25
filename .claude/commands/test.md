# Chạy Tests

Khi được gọi (`/test` hoặc `/test unit` hoặc `/test integration`):

1. Xác định loại test cần chạy:
   - `/test` → chạy tất cả
   - `/test unit` → chỉ unit tests
   - `/test integration` → chỉ integration tests (cần Docker)

2. Chạy lệnh tương ứng:
```bash
# Tất cả
dotnet test FootballBlog.Tests/

# Chỉ unit
dotnet test FootballBlog.Tests/ --filter "FullyQualifiedName~Unit"

# Chỉ integration
dotnet test FootballBlog.Tests/ --filter "FullyQualifiedName~Integration"
```

3. Phân tích kết quả:
   - Nếu pass hết → thông báo số lượng tests passed
   - Nếu có fail → chỉ rõ test nào fail, lý do, gợi ý fix

4. Nếu integration test fail do Docker chưa chạy → nhắc chạy `docker compose up -d` trước
