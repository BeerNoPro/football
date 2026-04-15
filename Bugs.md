# Architectural Decisions & Technical Notes

> Ghi lại quyết định kiến trúc quan trọng và known issues. Cập nhật: 2026-04-15

---

## RỦI RO #3 — DTOs / ViewModels ✅ DONE

**Vấn đề:** Repository trả entity ra view → leak `PasswordHash`, circular ref, over-fetch.

**Giải quyết:** DTOs trong `FootballBlog.Core/DTOs/`:
- `PostSummaryDto` — list (không load Content)
- `PostDetailDto` — chi tiết (có Content + Tags)
- `MatchSummaryDto` / `MatchPredictionDto` — football
- `PagedResult<T>` — pagination chung

**Còn lại:** Khi Phase 3 xong Identity, cập nhật `AuthorName` mapping trong `PostService` dùng `DisplayName` thay vì `Username`.

---

## RỦI RO #4 — Authentication 🔄 PARTIAL

**Quyết định:** ASP.NET Core Identity + Cookie (Blazor Admin) + JWT (API).

**Đã làm:**
- `ApplicationUser : IdentityUser<int>` ✅
- `AddIdentity<ApplicationUser, IdentityRole<int>>` đăng ký trong Program.cs ✅

**Chưa làm:**
- Cookie Auth middleware wire cho Blazor (Phase 3)
- JWT Bearer endpoint cho API (Phase 3)
- Login/Logout pages

**Setup mẫu cần thêm vào API/Program.cs:**
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddJwtBearer();
```

---

## RỦI RO #5 — MatchEvent.Type là string ❌ PENDING

**Vấn đề:** `MatchEvent.Type` đang là `string` (magic string "GOAL", "YELLOW_CARD"...) → dễ typo, không type-safe.

**Giải quyết cần làm:**
```csharp
// Thêm vào FootballBlog.Core/Models/EventType.cs
public enum EventType { Goal, YellowCard, RedCard, Substitution, Penalty, Offside }

// Cập nhật MatchEvent.cs
public EventType Type { get; set; }
```
Sau đó chạy `dotnet ef migrations add AddEventTypeEnum` và apply.

---

## CSS Framework Decision ✅ DONE

| Layer | Framework | Lý do |
|-------|-----------|-------|
| Public Blog (SSR) | Tailwind CSS | Bundle nhỏ, zero JS, SEO tốt |
| Admin Panel | MudBlazor | Component phong phú, form/table/dialog built-in |

**Tailwind:** đã setup, `npm run watch:css` trong `FootballBlog.Web/`.
**MudBlazor:** chưa install — Phase 3.

---

## Workflow HTML Prototype → Blazor ✅ DONE

Xem `.claude/rules/ui-design.md` cho quy trình đầy đủ.

File prototype chính: `wwwroot/prototype/home.html`
Design tokens: `#141414` bg | `#c8f04d` accent | `#4ade80` live green | layout 3 cột.

---

## Action Items Còn Lại

| # | Task | Priority | Phase |
|---|------|----------|-------|
| 1 | Wire Cookie Auth (Blazor) + JWT (API) | HIGH | Phase 3 |
| 2 | Install MudBlazor (Admin routes only) | HIGH | Phase 3 |
| 3 | Admin pages: Dashboard, Posts CRUD, Categories, Tags | HIGH | Phase 3 |
| ~~4~~ | ~~MatchEvent.Type → EventType enum + migration~~ | ~~LOW~~ | ✅ DONE |
| 5 | ILiveScoreService implementation + DI register | HIGH | Phase 4 |
| 6 | SignalR LiveScoreHub + Redis backplane | HIGH | Phase 4 |
| 7 | Blazor LiveScore pages + widget | MEDIUM | Phase 4 |
| 8 | IAIPredictionProvider interface + Claude impl | HIGH | Phase 5 |
| 9 | GeneratePredictionJob + PublishPredictionJob | HIGH | Phase 5 |
| 10 | Telegram.Bot + ITelegramService | MEDIUM | Phase 6 |

---

## Ghi chú

- **Không commit** `appsettings.Development.json` chứa connection string
- **Secrets**: `dotnet user-secrets` (local) | AWS Parameter Store (prod)
- **Upload S3**: chưa có — cần MediaController + `AWSSDK.S3` NuGet (Phase 3)
