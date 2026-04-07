# Architectural Decisions & Technical Notes

> Tài liệu này ghi lại các quyết định kiến trúc quan trọng, rủi ro đã xác định, và hướng giải quyết.
> Cập nhật: 2026-04-02

---

## RỦI RO #3 — DTOs / ViewModels (PENDING)

**Vấn đề:** Repository hiện trả thẳng domain entity ra view → có 3 nguy cơ:
1. `Author` entity chứa `PasswordHash` bị serialize ra ngoài
2. Circular reference: `Post → PostTag → Post` gây JSON crash
3. Over-fetching: load toàn bộ `Content` khi chỉ cần Title + Slug cho list

**Giải quyết:** ✅ Đã tạo DTOs trong `FootballBlog.Core/DTOs/`

```csharp
// Dùng cho trang danh sách — không load Content (nặng)
public record PostSummaryDto(
    int Id, string Title, string Slug, string? Thumbnail,
    string CategoryName, string CategorySlug,
    string AuthorName, DateTime PublishedAt
);

// Dùng cho trang chi tiết
public record PostDetailDto(
    int Id, string Title, string Slug, string Content, string? Thumbnail,
    string CategoryName, string CategorySlug,
    string AuthorName, DateTime PublishedAt, IList<string> Tags
);
```

**Còn thiếu:** Khi chuyển sang ASP.NET Identity (Phase 3), cần update `AuthorName` mapping trong `PostService.ToSummaryDto()` / `ToDetailDto()` để dùng `DisplayName` thay vì `Username`.

---

## RỦI RO #4 — Authentication (PENDING Phase 3)

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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddJwtBearer();
```

**Lưu ý migration:** Replace `ApplicationUser` hiện tại bằng class extend từ `IdentityUser<int>`:

```csharp
public class ApplicationUser : IdentityUser<int>
{
    // IdentityUser đã có: Id, UserName, Email, PasswordHash, Roles
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<Post> Posts { get; set; } = [];
}
```

**Sẽ cần migration mới** để replace bảng Users bằng ASP.NET Identity tables (5 bảng chuẩn).

---

## UX/UI — Chiến lược & Workflow

### Quyết định CSS Framework

| Layer | Framework | Lý do |
|-------|-----------|-------|
| Public Blog (SSR) | **Tailwind CSS** | Bundle nhỏ (purge unused), zero JS, SEO tốt, loading nhanh |
| Admin Panel | **MudBlazor** | Component library phong phú, form/table/dialog built-in, không cần SEO |

**Tailwind đã setup:** `npm install` trong `FootballBlog.Web/` → `npm run build:css` hoặc `npm run watch:css`.

**MudBlazor cho Admin:** Install ở Phase 3.

---

### Workflow HTML Prototype → Blazor

Không dùng Figma. UI được làm theo quy trình:

1. **Tham khảo:** URL web (WebFetch) hoặc ảnh screenshot (paste vào chat)
2. **Tạo prototype:** 1 file HTML tĩnh, Tailwind CDN, mở trực tiếp trên browser
3. **Review:** Chỉnh DevTools → approve
4. **Convert:** Tách thành Blazor component (`.razor`)

**Design system đã có — xem `.claude/rules/ui-design.md`:**
- Color tokens: `#141414` bg, `#c8f04d` accent lime, `#4ade80` live green
- Layout: 3 cột (240px sidebar | 1fr center | 300px right panel)
- File tham chiếu chính: `wwwroot/prototype/combined-home.html`

**Thứ tự ưu tiên trang cần làm:**
1. `Home` — live score + bài viết nổi bật (đã có prototype)
2. `PostDetail` — trang bài viết (typography quan trọng nhất)
3. `Admin/Dashboard` — dùng MudBlazor, thiết kế riêng

---

## Tóm tắt Action Items còn lại

| # | Task | Priority | Phase |
|---|------|----------|-------|
| 1 | Replace `ApplicationUser` → extend `IdentityUser<int>` + migration | HIGH | Phase 3 |
| 2 | Install MudBlazor (chỉ cho Admin routes) | MEDIUM | Phase 3 |
| 3 | Đổi `LiveMatch.Status` và `MatchEvent.Type` thành enum | LOW | Trước Phase 4 |
| 4 | Tạo HTML prototype PostDetail page (typography + AI analysis) | MEDIUM | Song song Phase 2 |
| 5 | Convert prototype Home → Blazor components (sau khi approve) | MEDIUM | Phase 2 |
| 6 | Thiết kế DB schema cho Match + MatchPrediction | HIGH | Phase 5 |
| 7 | Tạo IAIPredictionProvider interface + Claude implementation | HIGH | Phase 5 |
| 8 | Thiết kế MatchContext DTO (input cho AI prompt) | HIGH | Phase 5 |
| 9 | Install Telegram.Bot + ITelegramService | MEDIUM | Phase 6 |
| 10 | Lưu API keys vào dotnet user-secrets (FootballApi, Claude, Gemini, Telegram) | HIGH | Trước Phase 5 |

---

## Ghi chú thêm

- **Không commit** `appsettings.Development.json` chứa connection string
- **Secrets**: dùng `dotnet user-secrets` cho local, AWS Parameter Store cho production
- **Enum thay string** cho `MatchStatus` và `EventType` — tránh magic strings, dễ refactor
- **Pagination**: tạo `PagedResult<T>` generic class dùng chung cho mọi list API
