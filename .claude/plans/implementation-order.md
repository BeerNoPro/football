# Implementation Order Plan

## Context
Phase 3 còn 3 items, Phase 4 còn 2 items, Phase 5-7 chưa bắt đầu.
Thứ tự dưới đây theo dependency: admin editor trước → upload ảnh → realtime → AI → Telegram → Deploy.

---

## TASK 1 — Posts CRUD đầy đủ (Phase 3) ✅ DONE

### 1a. Thêm API endpoint GET by ID (admin)
**File:** `FootballBlog.API/Controllers/PostsController.cs`
- Thêm `GET /api/posts/{id:int}` — `[Authorize(Roles="Admin")]`
- Trả về `PostDetailDto` kể cả draft (khác với `GetBySlug` chỉ trả published)
- Dùng `IPostService.GetByIdAsync(int id)` (cần thêm vào service + interface)

**Files cần sửa:**
- `FootballBlog.Core/Interfaces/Services/IPostService.cs` — thêm `GetByIdAsync`
- `FootballBlog.Core/Services/PostService.cs` — implement `GetByIdAsync`
- `FootballBlog.Core/Interfaces/IPostRepository.cs` — thêm `GetByIdAsync`
- `FootballBlog.Infrastructure/Repositories/PostRepository.cs` — implement (AsNoTracking + Include)

### 1b. Mở rộng IAdminApiClient
**File:** `FootballBlog.Web/ApiClients/IAdminApiClient.cs` + `AdminApiClient.cs`
- Thêm `GetPostByIdAsync(int id)` → `GET /api/posts/{id:int}`
- Thêm `CreatePostAsync(CreatePostDto dto)` → `POST /api/posts`
- Thêm `UpdatePostAsync(int id, CreatePostDto dto)` → `PUT /api/posts/{id}`

### 1c. Tạo Posts/Create.razor
**File mới:** `FootballBlog.Web/Components/Pages/Admin/Posts/Create.razor`
- `@page "/admin/posts/create"` + `@rendermode InteractiveServer`
- Kế thừa `AdminPageBase`
- Form: Title → tự generate Slug, Category dropdown, Tags chips, Excerpt, PublishNow toggle
- Quill.js editor cho Content (xem Task 2 dưới)
- Thumbnail input URL (upload ảnh: Task 3)
- Save → `IAdminApiClient.CreatePostAsync()` → redirect về `/admin/posts`
- Tham chiếu: `wwwroot/prototype/admin-post-editor.html`

### 1d. Tạo Posts/Edit.razor
**File mới:** `FootballBlog.Web/Components/Pages/Admin/Posts/Edit.razor`
- `@page "/admin/posts/edit/{Id:int}"` + `@rendermode InteractiveServer`
- Load post bằng `IAdminApiClient.GetPostByIdAsync(Id)` trong `OnAdminInitializedAsync`
- Cùng form như Create, pre-filled với data hiện có
- Save → `IAdminApiClient.UpdatePostAsync(Id, dto)`

### 1e. Cập nhật Posts/Index.razor
**File:** `FootballBlog.Web/Components/Pages/Admin/Posts/Index.razor`
- Thêm nút Delete + confirmation (dùng `DialogService.ShowMessageBox` — như Categories/Index.razor)
- Sửa `IPostApiClient.GetPublishedAsync()` → `IAdminApiClient.GetAllPostsAsync()` (trả cả draft)

---

## TASK 2 — Rich Text Editor (Quill.js) (Phase 3) ✅ DONE

### 2a. Tạo JS interop file
**File mới:** `FootballBlog.Web/wwwroot/js/quill-interop.js`
```js
window.QuillInterop = {
    create: (elementId, dotnetRef) => { ... },
    getContent: (elementId) => { ... },
    setContent: (elementId, content) => { ... },
    destroy: (elementId) => { ... }
}
```

### 2b. Tạo Blazor component QuillEditor
**File mới:** `FootballBlog.Web/Components/Admin/QuillEditor.razor`
- Parameters: `[Parameter] string Value`, `[Parameter] EventCallback<string> ValueChanged`
- Inject `IJSRuntime`
- `OnAfterRenderAsync` → gọi `JS.InvokeVoidAsync("QuillInterop.create", ...)`
- `IAsyncDisposable` → `QuillInterop.destroy`
- Load Quill CDN (Snow theme) chỉ trong admin layout

### 2c. Tích hợp vào App.razor / AdminLayout
**File:** `FootballBlog.Web/Components/Layout/AdminLayout.razor`
- Thêm Quill CDN links (CSS + JS) vào `<head>` — chỉ cho admin routes

---

## TASK 3 — Upload Ảnh (Phase 3) ✅ DONE

### 3a. Tạo MediaController
**File mới:** `FootballBlog.API/Controllers/MediaController.cs`
- `POST /api/media/upload` — `[Authorize(Roles="Admin")]`
- Dev: lưu vào `wwwroot/uploads/` → trả về URL tương đối
- Prod (TODO): upload lên S3
- Validate: chỉ nhận image/*, max 5MB
- Trả về `{ url: "/uploads/filename.jpg" }`

### 3b. Thêm vào IAdminApiClient
- `UploadImageAsync(Stream fileStream, string fileName)` → `POST /api/media/upload`

### 3c. Tích hợp vào Posts/Create + Edit
- Thêm `InputFile` component cho Thumbnail
- Quill toolbar → Image button → gọi `UploadImageAsync` → insert URL vào editor

---

## TASK 4 — SignalR Hub + Redis Backplane (Phase 4)

### 4a. Cài NuGet (đã có StackExchange.Redis)
**File:** `FootballBlog.API/FootballBlog.API.csproj`
- Kiểm tra `Microsoft.AspNetCore.SignalR.StackExchangeRedis` — thêm nếu chưa có

### 4b. Tạo LiveScoreHub
**File mới:** `FootballBlog.API/Hubs/LiveScoreHub.cs`
```csharp
public class LiveScoreHub : Hub
{
    public async Task JoinMatch(string matchId) => await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");
    public async Task LeaveMatch(string matchId) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");
}
```

### 4c. Register SignalR + Redis backplane
**File:** `FootballBlog.API/Program.cs`
```csharp
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["Redis"]!);
app.MapHub<LiveScoreHub>("/hubs/livescore");
```

### 4d. Cập nhật LiveScorePollingJob — thêm broadcast
**File:** `FootballBlog.API/Jobs/LiveScorePollingJob.cs`
- Inject `IHubContext<LiveScoreHub>`
- Sau `uow.CommitAsync()`: `hubContext.Clients.Group($"match-{matchId}").SendAsync("MatchUpdated", dto)`

---

## TASK 5 — Blazor LiveScore Pages (Phase 4)

### 5a. Tạo LiveMatchDto + mapping
**Đã có** `LiveMatchDto` trong Core/DTOs — kiểm tra đủ fields chưa (score, minute, events).

### 5b. Tạo LiveScoreWidget.razor
**File mới:** `FootballBlog.Web/Components/Shared/LiveScoreWidget.razor`
- `@rendermode InteractiveServer`
- Inject `NavigationManager`, `IJSRuntime`
- `OnInitializedAsync` → load initial data từ API
- `OnAfterRenderAsync` → connect SignalR, join group `match-{MatchId}`
- Handle `MatchUpdated` event → update UI
- `IAsyncDisposable` → leave group + dispose connection
- Tham chiếu: `wwwroot/prototype/home.html` (live score row style)

### 5c. Tạo trang LiveScore/Index.razor
**File mới:** `FootballBlog.Web/Components/Pages/LiveScore/Index.razor`
- `@page "/livescore"` + `@rendermode InteractiveServer`
- Load danh sách live matches từ API (`IPostApiClient` hoặc thêm live score client)
- Embed `LiveScoreWidget` cho mỗi match

### 5d. Thêm ILiveScoreApiClient (Web layer)
**File mới:** `FootballBlog.Web/ApiClients/ILiveScoreApiClient.cs`
- `GetLiveMatchesAsync()` → `GET /api/livescore`
- `GetMatchByIdAsync(int id)` → `GET /api/livescore/{id}`
Cần thêm `LiveScoreController` trong API (hoặc endpoint trong PostsController).

---

## TASK 6 — AI Prediction (Phase 5)

### 6a. IAIPredictionProvider interface
**File mới:** `FootballBlog.Core/Interfaces/Services/IAIPredictionProvider.cs`
```csharp
interface IAIPredictionProvider {
    string ProviderName { get; }
    Task<MatchPredictionDto> PredictAsync(MatchContext context, CancellationToken ct);
}
```

### 6b. ClaudeAIPredictionProvider
**File mới:** `FootballBlog.Infrastructure/Services/ClaudeAIPredictionProvider.cs`
- Dùng Anthropic SDK hoặc raw HttpClient → `claude-opus-4-6`
- Build prompt từ `MatchContext` (H2H, TeamForm, Lineup, Referee)
- Parse response → `MatchPredictionDto`

### 6c. GeneratePredictionJob
**File mới:** `FootballBlog.API/Jobs/GeneratePredictionJob.cs`
- Query: `Match WHERE Prediction IS NULL AND ContextData IS NOT NULL AND KickoffUtc > NOW()`
- Deserialize ContextJson → MatchContext POCO
- Gọi `IAIPredictionProvider.PredictAsync()`
- `uow.MatchPredictions.AddAsync()`
- Trigger `PublishPredictionJob` nếu thành công

### 6d. PublishPredictionJob
**File mới:** `FootballBlog.API/Jobs/PublishPredictionJob.cs`
- Tạo Post từ prediction content
- `IPostService.CreateAsync(CreatePostDto)` — tự động publish

### 6e. GeminiAIPredictionProvider (fallback)
**File mới:** `FootballBlog.Infrastructure/Services/GeminiAIPredictionProvider.cs`
- Fallback khi Claude fails
- Register cả 2 providers với named DI

---

## TASK 7 — Telegram (Phase 6)

### Files cần tạo:
- `FootballBlog.Core/Interfaces/Services/ITelegramService.cs`
- `FootballBlog.Infrastructure/Services/TelegramService.cs` — dùng `Telegram.Bot` NuGet
- `FootballBlog.API/Jobs/TelegramNotificationJob.cs` — gửi prediction + edit khi có kết quả
- Admin page: prediction history + manual retrigger

---

## TASK 8 — Deploy & DevOps (Phase 7)

- `Dockerfile` (multi-stage: dotnet publish → runtime image)
- `docker-compose.prod.yml`
- `.github/workflows/ci.yml` — build + test + deploy Railway
- AWS EC2 + RDS + S3 setup (manual hoặc Terraform)
- CloudWatch log sink trong Serilog

---

## Thứ tự thực hiện

```
TASK 1 (Posts CRUD API + pages)
    │
    ├── TASK 2 (Quill.js editor) ← phụ thuộc Task 1
    │       │
    │       └── TASK 3 (Image upload) ← phụ thuộc Task 2
    │
TASK 4 (SignalR Hub)
    │
    └── TASK 5 (LiveScore UI) ← phụ thuộc Task 4
    
TASK 6 (AI Prediction) ← chạy song song với Task 4-5
    │
    └── TASK 7 (Telegram) ← phụ thuộc Task 6
    
TASK 8 (Deploy) ← sau khi mọi feature xong
```

---

## Quick Reference — Critical Files

| Task | Files chính |
|------|------------|
| 1a | `PostsController.cs`, `IPostService.cs`, `PostService.cs` |
| 1b | `IAdminApiClient.cs`, `AdminApiClient.cs` |
| 1c-d | `Admin/Posts/Create.razor`, `Admin/Posts/Edit.razor` (MỚI) |
| 1e | `Admin/Posts/Index.razor` |
| 2 | `wwwroot/js/quill-interop.js` (MỚI), `Admin/QuillEditor.razor` (MỚI) |
| 3 | `Controllers/MediaController.cs` (MỚI) |
| 4 | `Hubs/LiveScoreHub.cs` (MỚI), `Program.cs` (API), `LiveScorePollingJob.cs` |
| 5 | `Components/Shared/LiveScoreWidget.razor` (MỚI), `Pages/LiveScore/Index.razor` (MỚI) |
| 6 | `IAIPredictionProvider.cs` (MỚI), `ClaudeAIPredictionProvider.cs` (MỚI), `GeneratePredictionJob.cs` (MỚI) |
| 7 | `ITelegramService.cs` (MỚI), `TelegramService.cs` (MỚI) |
| 8 | `Dockerfile` (MỚI), `.github/workflows/ci.yml` (MỚI) |
