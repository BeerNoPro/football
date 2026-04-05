# Tạo Typed API Client

Khi được gọi với tên entity (e.g., `/api-client Tag`):

1. Xác nhận các methods cần thiết (dựa trên controller tương ứng đã có trong API)

2. Tạo 3 files theo đúng pattern:

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

3. Kiểm tra endpoint tương ứng trong API Controller đã có chưa — nếu chưa thì nhắc tạo trước

4. Nhắc inject `I{Entity}ApiClient` vào Blazor pages cần dùng
