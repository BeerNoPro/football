# Architectural Decisions & Technical Notes

> Tài liệu này ghi lại các quyết định kiến trúc quan trọng, rủi ro đã xác định, và hướng giải quyết.
> Cập nhật: 2026-04-01

---

## RỦI RO #1 + #2 — Data Flow & Service Layer (RESOLVED → Option B)

### Quyết định: Web → API via typed HttpClient

```
FootballBlog.Web (Blazor)
    └── HttpClient (typed)
            ↓
FootballBlog.API (ASP.NET Core)
    └── Controllers
            └── Services (IPostService, ILiveScoreService...)
                    └── Repositories
                            └── PostgreSQL
```

**Tại sao Option B:**
- Web project giữ nguyên, không thêm Infrastructure reference → separation of concerns rõ ràng
- Service Layer tự nhiên nằm ở API project, giải quyết luôn Rủi ro #2
- API có thể dùng độc lập cho mobile app hoặc external consumers sau này
- Blazor SSR vẫn hoạt động tốt: server render gọi API trên localhost (latency ~0ms)
- SignalR Hub nằm trong API, Blazor connect trực tiếp

**Cấu trúc Service Layer cần thêm vào Core:**

```
FootballBlog.Core/
├── Models/          ✅ đã có
├── Interfaces/
│   ├── Repositories/   ✅ đã có
│   └── Services/       ❌ cần thêm
│       ├── IPostService.cs
│       ├── ICategoryService.cs
│       └── ILiveScoreService.cs
├── DTOs/               ❌ cần thêm
│   ├── PostSummaryDto.cs
│   ├── PostDetailDto.cs
│   ├── CreatePostDto.cs
│   └── LiveMatchDto.cs
└── Services/           ❌ implement trong API hoặc Core (chọn Core để reuse)
    ├── PostService.cs
    └── LiveScoreService.cs
```

**Config HttpClient trong Web/Program.cs:**
```csharp
builder.Services.AddHttpClient<IPostApiClient, PostApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});
```

---

## RỦI RO #3 — DTOs / ViewModels (PENDING)

**Vấn đề:** Repository hiện trả thẳng domain entity ra view → có 3 nguy cơ:
1. `Author` entity chứa `PasswordHash` bị serialize ra ngoài
2. Circular reference: `Post → PostTag → Post` gây JSON crash
3. Over-fetching: load toàn bộ `Content` khi chỉ cần Title + Slug cho list

**Giải quyết:** Tạo DTOs trong `FootballBlog.Core/DTOs/`

```csharp
// Dùng cho trang danh sách — không load Content (nặng)
public record PostSummaryDto(
    int Id,
    string Title,
    string Slug,
    string? Thumbnail,
    string CategoryName,
    string AuthorName,
    DateTime PublishedAt
);

// Dùng cho trang chi tiết
public record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Thumbnail,
    string CategoryName,
    string CategorySlug,
    string AuthorName,
    DateTime PublishedAt,
    IList<string> Tags
);
```

---

## RỦI RO #4 — Authentication (RESOLVED)

### Quyết định: ASP.NET Core Identity + Cookie Auth (Blazor) + JWT (API)

**Lý do chọn Identity thay vì tự implement:**
- Industry standard, battle-tested, Microsoft-maintained
- Tự xử lý: PBKDF2 password hashing, account lockout, roles, claims
- Không có rủi ro bảo mật từ custom password logic
- .NET 8 có `AddIdentityApiEndpoints` — setup cả cookie + JWT cùng lúc

**Kiến trúc auth:**

```
Admin Panel (Blazor InteractiveServer)
    → Cookie Auth (Identity)
    → Lý do: Cookie tự refresh, hoạt động tốt với SSR, không cần lưu token

API Endpoints (REST)
    → JWT Bearer Token
    → Lý do: Stateless, dùng được cho mobile/external sau này
```

**Setup trong API/Program.cs:**
```csharp
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Cookie cho Blazor
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddJwtBearer(); // JWT cho API endpoints

// Hoặc dùng .NET 8 shortcut:
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Lưu ý migration:** Cần replace `ApplicationUser` hiện tại bằng `IdentityUser` hoặc extend từ `IdentityUser<int>`:

```csharp
// Thay vì custom ApplicationUser hiện tại:
public class ApplicationUser : IdentityUser<int>
{
    // IdentityUser đã có: Id, UserName, Email, PasswordHash, Roles
    // Thêm custom fields:
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<Post> Posts { get; set; } = [];
}
```

**Sẽ cần migration mới** để replace bảng Users bằng ASP.NET Identity tables (5 bảng chuẩn).

---

## RỦI RO #5 — Unit of Work Pattern (RESOLVED)

### Quyết định: Implement full Unit of Work

**Lý do quan trọng cho Football data:**
- Lưu LiveMatch + nhiều MatchEvent phải atomic (1 transaction)
- Nếu save event thất bại, rollback cả match update
- Repository hiện có `SaveChangesAsync()` riêng từng cái → không đảm bảo consistency

**Implementation:**

```csharp
// FootballBlog.Core/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IPostRepository Posts { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
    ILiveMatchRepository LiveMatches { get; }

    Task<int> CommitAsync(); // Gọi 1 lần duy nhất
    Task RollbackAsync();
}
```

```csharp
// FootballBlog.Infrastructure/Data/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IPostRepository Posts { get; }
    public ICategoryRepository Categories { get; }
    // ...

    public UnitOfWork(ApplicationDbContext context, ...)
    {
        _context = context;
        Posts = new PostRepository(context);
        // ...
    }

    public Task<int> CommitAsync() => _context.SaveChangesAsync();

    public async Task RollbackAsync()
    {
        // EF Core: discard tracked changes
        foreach (var entry in _context.ChangeTracker.Entries())
            entry.State = EntityState.Detached;
    }
}
```

**Cách dùng trong Service:**
```csharp
// Lưu match + events trong 1 transaction
public async Task UpdateLiveMatchAsync(LiveMatchDto dto)
{
    var match = await _uow.LiveMatches.GetByExternalIdAsync(dto.ExternalId);
    match.HomeScore = dto.HomeScore;
    match.AwayScore = dto.AwayScore;

    foreach (var evt in dto.NewEvents)
        await _uow.LiveMatches.AddEventAsync(match.Id, evt);

    await _uow.CommitAsync(); // 1 lần duy nhất → atomic
}
```

**Repository sẽ bỏ `SaveChangesAsync()` internal** — chỉ để thêm entity vào ChangeTracker.

---

## UX/UI — Chiến lược & Workflow

### Quyết định CSS Framework

| Layer | Framework | Lý do |
|-------|-----------|-------|
| Public Blog (SSR) | **Tailwind CSS** | Bundle nhỏ (purge unused), zero JS, SEO tốt, loading nhanh |
| Admin Panel | **MudBlazor** | Component library phong phú, form/table/dialog built-in, không cần SEO |

**Tailwind cho public pages:**
- HTML được render server-side hoàn toàn
- Google indexing content đầy đủ
- Core Web Vitals tốt (LCP, CLS)
- Setup: `npm install tailwindcss` + `tailwind.config.js` trong Web project

**MudBlazor cho Admin:**
- CRUD tables, dialogs, forms — không cần tự build
- Chỉ load trên Admin routes → không ảnh hưởng performance blog

---

### Workflow Figma → Blazor

**Bước 1: Tìm nguồn tham khảo design**

Trang tìm inspiration tốt nhất:
- **Dribbble** (dribbble.com) — search "football blog", "sports news website", "soccer app UI"
- **Behance** (behance.net) — search "football website design", "sports blog 2024"
- **Figma Community** (figma.com/community) — search "sports dashboard", "blog template"
- **Screenlane** (screenlane.com) — UI patterns theo component (card, navbar, hero section)
- **Refero** (refero.design) — real product UI references

**Yếu tố quan trọng khi chọn reference:**
- Layout hiển thị tốt trên mobile (responsive)
- Hero section nổi bật cho bài viết nổi bật
- Card layout cho danh sách bài viết
- Score widget compact nhưng rõ ràng
- Dark mode friendly (football fans thường xem ban đêm)

**Bước 2: Design trong Figma**

Thứ tự ưu tiên design:
1. `Home.figma` — hero + bài viết nổi bật + live score widget
2. `PostDetail.figma` — trang bài viết (typography quan trọng nhất)
3. `Admin/Dashboard.figma` — thống kê + quick actions
4. `Admin/PostEditor.figma` — rich text editor layout

Design tokens cần define trong Figma:
- **Colors**: primary (brand), neutral (text/bg), success/warning/error
- **Typography**: heading scale (h1→h6), body, caption
- **Spacing**: 4px base grid
- **Border radius**: sm/md/lg
- Export thành Tailwind config sau

**Bước 3: Cấu hình Figma MCP trong Claude Code**

Sau khi có design Figma, thêm MCP server vào `.claude/settings.json`:

```json
{
  "mcpServers": {
    "figma": {
      "command": "npx",
      "args": ["-y", "@figma/mcp-server-figma"],
      "env": {
        "FIGMA_ACCESS_TOKEN": "<your-personal-access-token>"
      }
    }
  }
}
```

Lấy Figma Access Token: Figma → Settings → Account → Personal access tokens

**Cách dùng MCP Figma trong Claude Code:**
```
"Đọc frame Home page trong Figma file [URL], generate Blazor component Home.razor 
với Tailwind CSS classes. Dùng SSR render mode, đảm bảo không có @rendermode."
```

Claude sẽ đọc trực tiếp Figma frames, extract layout/colors/spacing, tạo Blazor component chính xác theo design.

**Bước 4: Export design tokens → Tailwind config**

```js
// tailwind.config.js — sau khi có Figma tokens
module.exports = {
  content: ["./**/*.{razor,html,cs}"],
  theme: {
    extend: {
      colors: {
        primary: '#1a56db',   // lấy từ Figma color styles
        'pitch-green': '#2d6a4f',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
        display: ['Oswald', 'sans-serif'], // cho heading thể thao
      }
    }
  }
}
```

---

## Tóm tắt Action Items

| # | Task | Priority | Phase |
|---|------|----------|-------|
| 1 | Tạo `IUnitOfWork` + `UnitOfWork` trong Infrastructure | HIGH | Trước Phase 2 |
| 2 | Tạo `DTOs/` trong Core project | HIGH | Trước Phase 2 |
| 3 | Tạo `Interfaces/Services/` + `Services/` trong Core | HIGH | Trước Phase 2 |
| 4 | Replace `ApplicationUser` → extend `IdentityUser<int>` + migration | HIGH | Phase 3 |
| 5 | Config typed HttpClient trong Web → gọi API | HIGH | Trước Phase 2 |
| 6 | Setup Tailwind CSS trong Web project | MEDIUM | Trước Phase 2 |
| 7 | Install MudBlazor (chỉ cho Admin routes) | MEDIUM | Phase 3 |
| 8 | Đổi `LiveMatch.Status` và `MatchEvent.Type` thành enum | LOW | Trước Phase 4 |
| 9 | Tìm Figma reference → tạo design Home + PostDetail | MEDIUM | Song song Phase 2 |
| 10 | Cấu hình Figma MCP sau khi có design file | LOW | Sau khi có design |

---

## Ghi chú thêm

- **Không commit** `appsettings.Development.json` chứa connection string
- **Secrets**: dùng `dotnet user-secrets` cho local, AWS Parameter Store cho production
- **Enum thay string** cho `MatchStatus` và `EventType` — tránh magic strings, dễ refactor
- **Pagination**: tạo `PagedResult<T>` generic class dùng chung cho mọi list API
