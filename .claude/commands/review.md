# Review Code Trước Khi Commit

Khi được gọi (`/review [focus]`):

---

## BƯỚC 0 — Nạp context dự án (luôn làm trước)

Đọc nhanh 3 file này trước khi review bất kỳ code nào:

```
STRUCTURE.md        — architecture, domain model, job lifecycle, known issues
docs/FLOW_2.md      — production flow, provider order, bugs đã fix, checklist còn lại
TODO.md             — open issues phân loại CRITICAL/HIGH/MEDIUM
```

Mục đích: hiểu pattern nào đang đúng, anti-pattern nào đã biết, contract nào cần giữ nguyên.

---

## BƯỚC 1 — Xác định phạm vi thay đổi

```bash
rtk git diff --staged        # xem những gì đã stage
rtk git diff                 # xem những gì chưa stage
rtk git diff HEAD~1          # review commit cuối
rtk git diff HEAD~3..HEAD    # review 3 commit gần nhất
```

`rtk git diff` dùng `rtk diff` bên trong → chỉ hiện changed lines, bỏ context thừa.

Từ diff, lập danh sách:
- **Files thay đổi** — tên file + loại thay đổi (add/modify/delete)
- **Symbols thay đổi** — method/class/interface nào bị sửa

---

## BƯỚC 2 — Tìm context liên quan

### 2a. Locate đúng đoạn cần đọc

```bash
# Tìm symbol → lấy line number
rtk grep "TênMethod|TênClass" FootballBlog.API/Path/To/File.cs

# Xem chỉ signatures (strips body — rất ít token)
rtk read FootballBlog.API/Path/To/File.cs -l aggressive

# Tóm tắt nhanh 2 dòng
rtk smart FootballBlog.API/Path/To/File.cs
```

### 2b. Tìm interface tương ứng

```bash
rtk find "IXxx*.cs" FootballBlog.Core/Interfaces/
```

### 2c. Tìm callers của method bị sửa

```bash
rtk grep "TênMethod(" FootballBlog.API/ FootballBlog.Web/ FootballBlog.Core/
```

Grouped by file → dễ thấy caller nào bị ảnh hưởng, có vỡ contract không.

### 2d. Kiểm tra migration nếu có thay đổi Model/DbContext

```bash
rtk ls FootballBlog.Infrastructure/Data/Migrations/
```

Migration mới nhất có khớp với entity thay đổi không?

---

## BƯỚC 3 — Checklist review theo layer

Chỉ check những mục **liên quan** đến layer đang review.

### Service Layer
- [ ] Async/await đúng — không `.Result` / `.Wait()`
- [ ] Dùng `IUnitOfWork`, không inject DbContext trực tiếp
- [ ] `CommitAsync()` chỉ gọi 1 lần — không gọi trong repository hay trong upsert helper
- [ ] Log đúng level: Debug bắt đầu | Info thành công | Warning not found | Error exception
- [ ] Trả DTO — không expose entity ra ngoài service

### Repository Layer
- [ ] Read-only query có `.AsNoTracking()`
- [ ] Có `.TagWithCaller()` trước mọi terminal method (`ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`)
- [ ] Không `.Include()` thừa trên list query — chỉ include khi caller cần field đó
- [ ] Pagination dùng `.Skip().Take()` — không load all rồi filter
- [ ] Không expose `IQueryable` ra ngoài repository

### Controller / API
- [ ] Response dùng `ApiResponse<T>` wrapper
- [ ] Không null check thừa — service đã xử lý
- [ ] Input validation ở đây, không trong service
- [ ] Route convention đúng: `GET /api/posts/{id}` không phải `/api/GetPost`

### Blazor Component
- [ ] SSR page: không có `@rendermode` hoặc dùng `InteractiveServer` có lý do rõ ràng
- [ ] Không gọi DbContext trực tiếp từ `.razor` — chỉ qua HttpClient/API
- [ ] `@if` / `@foreach` có null check để tránh render lỗi

### Model / Entity
- [ ] Migration đã được tạo chưa?
- [ ] DB naming đúng: bảng snake_case số nhiều, cột snake_case
- [ ] Navigation properties có `= null!` hoặc `= new()` phù hợp

### Security (mọi layer)
- [ ] Không hardcode secret / connection string
- [ ] Input từ user được validate trước khi dùng
- [ ] Không có SQL raw string ghép từ user input

---

## BƯỚC 4 — Checklist đặc thù Football Blog

Check kỹ khi review code liên quan đến jobs, AI, API Football, hoặc SignalR.

### Hangfire Jobs
- [ ] **Idempotent**: job chạy lại 2 lần không tạo duplicate data (check bằng `GetByExternalIdAsync` hoặc unique index)
- [ ] **Throw on failure**: không `return false` / không swallow exception — phải `throw` để Hangfire retry
- [ ] **CommitAsync ngoài loop**: không gọi `CommitAsync()` trong `foreach` upsert — commit 1 lần/batch sau khi tất cả entity đã vào ChangeTracker
- [ ] **Navigation property thay FK int**: khi tạo entity mới cần FK của entity chưa commit, dùng navigation property (`League.Country = country`) thay vì gán `CountryId = country.Id` (Id có thể còn = 0)
- [ ] **Log đầu + cuối job**: `LogInformation` khi bắt đầu + khi kết thúc kèm số record xử lý

### AI Provider
- [ ] **Thứ tự**: Gemini primary → Claude fallback (không đảo ngược)
- [ ] **Cả 2 fail → throw**: không silent fail, không return null — để Hangfire retry
- [ ] **Chỉ PremiumLeagues**: GeneratePredictionJob và HalfTimePredictionJob chỉ chạy cho league trong `PremiumLeagueIds` config

### Football API / Rate Limiter
- [ ] Mọi call tới `FootballApiClient` đều đi qua 2 lớp: `RedisFootballApiRateLimiter` (10 req/min) + `ApiUsageTracker` (100 req/day)
- [ ] Khi API trả `null` → log warning + dừng lại, không tiếp tục process

### LiveScore / SignalR
- [ ] `GetLiveMatchesAsync()` **không** có `.Include(m => m.Events)` — events chỉ load từ API response khi broadcast
- [ ] Broadcast dùng `fixture.MatchId` (nullable) — kiểm tra `.HasValue` trước khi gửi vào group

### Upsert Pattern (FetchUpcomingMatchesJob)
- [ ] Các upsert helper trả `entity object` (Country/League/Team), không trả `int`
- [ ] Cache `Dictionary<string, Country>` / `Dictionary<int, League>` / `Dictionary<int, Team>` — tránh lookup DB lặp lại trong cùng run
- [ ] `CommitAsync()` chỉ gọi 1 lần/ngày ở cuối `foreach (DateOnly date in datesToFetch)`

---

## BƯỚC 5 — Anti-patterns đã từng xảy ra trong project

Những lỗi này đã được fix — nếu thấy lại trong code mới là regression:

| Anti-pattern | Đúng |
|---|---|
| `ApiKeySeeder` check `if (table.Any())` → skip toàn bộ | Check per-provider: `where k.Provider == provider` |
| `GeneratePredictionJob` `return false` khi AI fail | `throw new InvalidOperationException(...)` để Hangfire retry |
| `FetchSquadJob` không đăng ký DI | `builder.Services.AddScoped<FetchSquadJob>()` |
| `IEnumerable` gọi `.Count()` nhiều lần | `.ToList()` trước, dùng `list.Count` |
| `FetchUpcomingMatchesJob` ghi đè `Round`/`VenueName` bằng `null` | `if (!string.IsNullOrEmpty(fixture.Round))` null-guard |
| `CommitAsync()` trong mỗi upsert helper | Commit 1 lần/ngày sau toàn bộ batch |
| `GetLiveMatchesAsync` có `.Include(m => m.Events)` | Bỏ Include — events lấy từ API response |

---

## BƯỚC 6 — Output

```
## Review Summary

**Phạm vi:** <danh sách files đã đọc>
**Callers kiểm tra:** <method nào đã trace>

### Ổn ✅
- <điều gì đang đúng>

### Cần sửa trước commit ❌
- `file.cs:line` — <vấn đề cụ thể + cách fix>

### Gợi ý (không bắt buộc) 💡
- <cải thiện tùy chọn>
```

**KHÔNG tự chạy `git commit`** — để người dùng quyết định sau khi đọc summary.

---

## Ví dụ gọi lệnh

```
/review
/review PostService.cs        ← focus vào 1 file
/review Phase4                ← review toàn bộ thay đổi Phase 4
/review FootballBlog.API/Jobs ← review theo folder
```
