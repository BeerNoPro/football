# Tạo Typed API Client

Khi được gọi với tên entity (e.g., `/api-client Tag`):

## Bước 1 — Xác nhận endpoints đã có

```bash
grep -n "HttpGet\|HttpPost\|HttpPut\|HttpDelete\|Route" FootballBlog.API/Controllers/{Entity}Controller.cs 2>/dev/null | head -20
```

Nếu controller chưa có → nhắc tạo controller trước, không tạo client cho endpoint không tồn tại.

## Bước 2 — Kiểm tra interface/client đã tồn tại chưa

```bash
find . -name "I{Entity}ApiClient.cs" -o -name "{Entity}ApiClient.cs" | grep -v "/bin/\|/obj/"
```

## Bước 3 — Tạo 3 files theo pattern

**`FootballBlog.Core/Interfaces/I{Entity}ApiClient.cs`:**
```csharp
public interface I{Entity}ApiClient
{
    Task<IEnumerable<{Entity}Dto>?> GetAllAsync();
    Task<{Entity}Dto?> GetBySlugAsync(string slug);
    // ... thêm method theo endpoint thực tế
}
```

**`FootballBlog.Web/ApiClients/{Entity}ApiClient.cs`:**
```csharp
public class {Entity}ApiClient(HttpClient http, ILogger<{Entity}ApiClient> logger)
    : I{Entity}ApiClient
{
    public async Task<IEnumerable<{Entity}Dto>?> GetAllAsync()
    {
        try
        {
            var response = await http.GetFromJsonAsync<ApiResponse<IEnumerable<{Entity}Dto>>>("/api/{entities}");
            return response?.Data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch {Entities}", "{entities}");
            return null;
        }
    }
}
```

**Đăng ký trong `FootballBlog.Web/Program.cs`:**
```csharp
builder.Services.AddHttpClient<I{Entity}ApiClient, {Entity}ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});
```

## Bước 4 — Xác nhận đăng ký đã đúng chỗ

```bash
grep -n "{Entity}ApiClient" FootballBlog.Web/Program.cs
```

## Bước 5 — Nhắc inject vào Blazor pages

```bash
grep -rn "I{Entity}ApiClient" FootballBlog.Web/Components/ --include="*.razor" | head -10
```

Liệt kê các page cần inject client mới.
