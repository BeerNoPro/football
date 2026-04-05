# Chạy Tests

Khi được gọi (`/test`, `/test unit`, `/test integration`):

1. Kiểm tra test project có tồn tại chưa:
```bash
ls FootballBlog.Tests/ 2>/dev/null || echo "NOT_FOUND"
```

**Nếu chưa có test project** → thông báo:
> Test project chưa được tạo. Phase 2+ mới implement tests.
> Để tạo: `dotnet new xunit -n FootballBlog.Tests` rồi thêm packages: xUnit, FluentAssertions, NSubstitute, Testcontainers.PostgreSql

**Nếu đã có** → chạy theo loại:

2. Chạy lệnh tương ứng:
```bash
# Tất cả
dotnet test FootballBlog.Tests/ --no-build -v minimal

# Chỉ unit (không cần Docker)
dotnet test FootballBlog.Tests/ --filter "Category=Unit" -v minimal

# Chỉ integration (cần Docker)
dotnet test FootballBlog.Tests/ --filter "Category=Integration" -v minimal
```

3. Phân tích kết quả:
   - Pass hết → báo số lượng passed
   - Có fail → chỉ rõ test nào fail, lý do, gợi ý fix

4. Nếu integration test fail do Docker chưa chạy → nhắc `/docker up` trước

## Naming convention (khi tạo test mới)
`MethodName_Scenario_ExpectedResult`
Ví dụ: `GetBySlugAsync_WhenSlugNotFound_ReturnsNull`

## Packages cần có trong test project
```xml
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="NSubstitute" Version="5.*" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.*" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.*" /> <!-- integration only -->
```
