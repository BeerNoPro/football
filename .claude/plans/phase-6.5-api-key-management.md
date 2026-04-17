# Plan: Phase 6.5 — API Key Management

**Trạng thái:** Chờ approve  
**Ưu tiên:** Trước Phase 7 (Deploy)

---

## Mục tiêu

- Lưu API keys (FootballApi / Claude / Gemini) vào DB thay vì appsettings
- Hỗ trợ nhiều key per provider — tự động rotate sang key tiếp theo khi key hiện tại hết quota hoặc trả 403/429
- Admin có thể thêm/xóa/enable/disable key từ UI mà không cần restart app

## Quyết định kiến trúc

| Vấn đề | Quyết định | Lý do |
|---|---|---|
| Telegram migrate vào DB không? | **Không** | Bot là identity (tên, avatar), `TelegramBotClient` khởi tạo 1 lần với fixed token, không phải vấn đề rate-limit |
| Usage tracking ở đâu? | **Redis** (không phải DB) | Reset daily, đọc ghi tần suất cao, đã có Redis sẵn |
| Seed data lần đầu? | `ApiKeySeeder` đọc appsettings → insert DB nếu chưa có | Không mất key cũ khi migrate |
| Encrypt KeyValue trong DB? | Plain text (dev) — note cho Phase 7 (prod) | Tránh phức tạp hóa sớm |

---

## Schema

### Entity: `ApiKeyConfig`

```csharp
// FootballBlog.Core/Models/ApiKeyConfig.cs
public class ApiKeyConfig
{
    public int Id { get; set; }
    public string Provider { get; set; }   // "FootballApi" | "Claude" | "Gemini"
    public string KeyValue { get; set; }   // plain text dev, cần encrypt prod
    public int Priority { get; set; }      // 1 = ưu tiên cao nhất
    public bool IsActive { get; set; } = true;
    public int DailyLimit { get; set; }    // 0 = unlimited
    public string? Note { get; set; }      // "account1@gmail.com" — ghi nhớ key này của ai
    public DateTime CreatedAt { get; set; }
}
```

### Redis key pattern (usage tracking)

```
apikey:usage:{provider}:{keyHash}:{yyyy-MM-dd}
```
TTL tự expire lúc midnight UTC. `keyHash` = SHA1(KeyValue)[..8] — tránh lộ key trong Redis.

---

## Interface

```csharp
// FootballBlog.Core/Interfaces/IApiKeyRotator.cs
public interface IApiKeyRotator
{
    Task<string?> GetAvailableKeyAsync(string provider);
    Task MarkExhaustedAsync(string provider, string key);
}
```

Logic `GetAvailableKeyAsync`:
1. Query DB lấy keys của provider, `IsActive=true`, order by `Priority` (cache Redis 5 phút)
2. Với mỗi key: kiểm tra Redis `usedToday < dailyLimit` (0 = skip check)
3. Return key đầu tiên còn quota — `null` nếu hết tất cả → log Warning

---

## Files thay đổi

### Files MỚI (6 files)

| File | Nội dung |
|---|---|
| `FootballBlog.Core/Models/ApiKeyConfig.cs` | Entity |
| `FootballBlog.Core/Interfaces/IApiKeyRotator.cs` | Interface |
| `FootballBlog.Infrastructure/Services/ApiKeyRotator.cs` | Implementation: DB query + Redis cache + usage tracking |
| `FootballBlog.Infrastructure/Services/ApiKeySeeder.cs` | Seed từ appsettings lần đầu startup |
| `FootballBlog.API/Controllers/ApiKeysController.cs` | CRUD endpoints (Admin only) |
| `FootballBlog.Web/Components/Pages/Admin/ApiKeys/Index.razor` | Admin UI — list + toggle + thêm/xóa key |

### Files SỬA (7 files)

| File | Thay đổi cụ thể |
|---|---|
| `FootballBlog.Core/Options/FootballApiOptions.cs` | Xóa `ApiKey` property |
| `FootballBlog.Infrastructure/Data/ApplicationDbContext.cs` | Thêm `DbSet<ApiKeyConfig> ApiKeyConfigs` |
| `FootballBlog.Infrastructure/Services/RedisFootballApiRateLimiter.cs` | Đổi Redis key từ global → per-key pattern (counter reset, chấp nhận được) |
| `FootballBlog.API/ApiClients/FootballApi/FootballApiClient.cs` | Inject `IApiKeyRotator`, set `x-apisports-key` header per-request |
| `FootballBlog.Infrastructure/Services/ClaudeAIPredictionProvider.cs` | Thay `IConfiguration["Claude:ApiKey"]` → `IApiKeyRotator.GetAvailableKeyAsync("Claude")` |
| `FootballBlog.Infrastructure/Services/GeminiAIPredictionProvider.cs` | Thay `IConfiguration["Gemini:ApiKey"]` → `IApiKeyRotator.GetAvailableKeyAsync("Gemini")` |
| `FootballBlog.API/Program.cs` | Bỏ hardcode header FootballApi, register `IApiKeyRotator`, gọi `ApiKeySeeder` |

### Migration

- Tên: `AddApiKeyConfig`
- Lệnh: `dotnet ef migrations add AddApiKeyConfig --project FootballBlog.Infrastructure --startup-project FootballBlog.API`

---

## Thứ tự implement

```
Step 1 — Core layer
  [x] ApiKeyConfig entity
  [x] IApiKeyRotator interface

Step 2 — Migration
  [x] Tạo + apply migration AddApiKeyConfig

Step 3 — Infrastructure
  [x] ApiKeyRotator implementation
  [x] ApiKeySeeder (đọc appsettings → seed DB lần đầu)
  [x] Cập nhật RedisFootballApiRateLimiter → per-key tracking

Step 4 — Cập nhật providers
  [x] FootballApiClient — inject IApiKeyRotator
  [x] ClaudeAIPredictionProvider — inject IApiKeyRotator
  [x] GeminiAIPredictionProvider — inject IApiKeyRotator

Step 5 — Program.cs
  [x] Bỏ hardcode header, register services, gọi seeder

Step 6 — API Controller
  [x] ApiKeysController (CRUD, [Authorize(Roles="Admin")])

Step 7 — Admin UI
  [x] /admin/api-keys — table + toggle IsActive + thêm/xóa

Step 8 — Cleanup config
  [x] Review appsettings.json + appsettings.Development.example.json
  [x] Xóa ApiKey khỏi user-secrets sau khi seeder chạy xong
```

---

## Checklist review appsettings khi implement xong

**Xóa khỏi appsettings / user-secrets:**
- `FootballApi:ApiKey`
- `Claude:ApiKey`
- `Gemini:ApiKey`

**Giữ nguyên (infra config):**
- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:Redis`
- `Jwt:Key`
- `FootballApi:BaseUrl`, `FootballApi:DailyRequestLimit`, `FootballApi:LeagueIds`
- `Telegram:BotToken`, `Telegram:ChannelId`
- `WebBaseUrl`, `Prediction:*`, `Serilog:*`

---

## Risk

| Risk | Mức độ | Xử lý |
|---|---|---|
| Redis key pattern đổi → counter reset | Thấp | Chấp nhận — reset về 0 không gây lỗi |
| Key lộ trong DB | Trung bình | Note cho Phase 7: dùng AWS Secrets Manager hoặc column encryption |
| Seeder chạy nhiều lần | Không có | Guard: chỉ seed nếu `ApiKeyConfigs` table đang rỗng |
| Tổng 13 files | Trung bình | Hầu hết thêm mới, sửa từng điểm inject — ít risk regression |
