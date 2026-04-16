# Review & Next Phase Plan

## Context

Review source code so với TODO.md / Bugs.md — tất cả đều **chính xác**, không có discrepancy.

### Kết quả review

| Phase | Status trong TODO | Status thực tế | Kết luận |
|-------|------------------|----------------|----------|
| Phase 1 | ✅ Done | ✅ Done | Khớp |
| Phase 2 | ✅ Done (trừ S3) | ✅ Done (trừ S3) | Khớp |
| Phase 3 | 🔄 In Progress | 0% implemented (chỉ có Identity model + HTML prototypes) | Khớp |
| Phase 4 | 🔄 In Progress | ~20% (Jobs + interface, thiếu service + SignalR + UI) | Khớp |
| Phase 5 | ⬜ | 0% (chỉ có models/repos) | Khớp |
| Bugs.md EventType | ✅ DONE | ✅ Đã có EventType.cs + MatchEvent.cs | Khớp |

**Không có bug trong TODO.md / Bugs.md** — tài liệu phản ánh đúng trạng thái code.

---

## Kế hoạch tiếp theo — Phase 3: Admin Panel Foundation

Theo TODO.md logic, Phase 3 là ưu tiên cao nhất (blocking Phase 4 LiveScore auth). Thực hiện theo thứ tự dependency:

### Step 1 — Cookie Auth + JWT Wire-up (API)

**File:** `FootballBlog.API/Program.cs`

Thêm sau `AddIdentity` (line ~83):
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    })
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
```

Thêm secret vào `dotnet user-secrets`: `Jwt:Key` (32+ chars).

### Step 2 — Install MudBlazor (Web)

**File:** `FootballBlog.Web/FootballBlog.Web.csproj`
```xml
<PackageReference Include="MudBlazor" Version="7.*" />
```

**File:** `FootballBlog.Web/Program.cs`
```csharp
builder.Services.AddMudServices();
```

**File:** `FootballBlog.Web/Components/App.razor` — chỉ thêm MudBlazor CSS/JS trong `<head>` cho admin routes (dùng conditional).

### Step 3 — Admin Layout + Login Page

**Files mới cần tạo:**

| File | Vai trò |
|------|---------|
| `Components/Layout/AdminLayout.razor` | MudBlazor shell (NavMenu, AppBar, Drawer) |
| `Components/Pages/Admin/Login.razor` | Login form — POST tới `/api/auth/login` |
| `Controllers/AuthController.cs` (API) | `POST /api/auth/login` → issue Cookie + JWT |

### Step 4 — Admin Pages (từ HTML prototypes đã có)

Tạo Blazor components từ prototypes trong `wwwroot/prototype/`:

| Blazor File | Prototype tham chiếu |
|-------------|---------------------|
| `Components/Pages/Admin/Dashboard.razor` | `prototype/admin-dashboard.html` |
| `Components/Pages/Admin/Posts/Index.razor` | `prototype/admin-posts.html` |
| `Components/Pages/Admin/Posts/Edit.razor` | `prototype/admin-post-editor.html` |
| `Components/Pages/Admin/Categories/Index.razor` | `prototype/admin-categories.html` |
| `Components/Pages/Admin/Tags/Index.razor` | (tạo mới tương tự categories) |

### Step 5 — ILiveScoreService Implementation (Phase 4 Quick Win)

**File mới:** `FootballBlog.Core/Services/LiveScoreService.cs`

Interface đã có tại `FootballBlog.Core/Interfaces/Services/ILiveScoreService.cs`.
Implementation cần dùng `IUnitOfWork` để query matches từ DB.

**DI register trong** `FootballBlog.API/Program.cs`:
```csharp
builder.Services.AddScoped<ILiveScoreService, LiveScoreService>();
```

---

## Thứ tự thực hiện (recommended)

```
Step 1: Cookie Auth + JWT  →  Step 2: MudBlazor install
    ↓                              ↓
Step 3: AdminLayout + Login  ←────┘
    ↓
Step 4: Admin Pages (Dashboard → Posts → Categories → Tags)
    ↓
Step 5: LiveScoreService (quick, independent)
```

---

## Critical Files

- `FootballBlog.API/Program.cs` — auth wire-up
- `FootballBlog.Web/FootballBlog.Web.csproj` — MudBlazor package
- `FootballBlog.Web/Program.cs` — AddMudServices
- `FootballBlog.Web/Components/App.razor` — MudBlazor CSS/JS conditional load
- `FootballBlog.Core/Interfaces/Services/ILiveScoreService.cs` — interface đã có, cần impl

## Verification

1. `dotnet build` — toàn bộ solution build clean
2. Truy cập `/admin/login` → login form hiện ra
3. Login thành công → redirect sang `/admin/dashboard`
4. MudBlazor DataGrid hiện data từ API (posts list)
5. Trang blog public `/` vẫn hoạt động bình thường (không bị ảnh hưởng bởi MudBlazor)
