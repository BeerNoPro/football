# Review Code Trước Khi Commit

Khi được gọi (`/review [focus]`):

---

## BƯỚC 1 — Xác định phạm vi thay đổi

Tất cả lệnh `git` tự động được RTK hook rewrite → output được filter + compress.

```bash
git diff --staged        # xem những gì đã stage
git diff                 # xem những gì chưa stage
git diff HEAD~1          # review commit cuối cùng
git diff HEAD~3..HEAD    # review 3 commit gần nhất
```

Từ diff, lập danh sách:
- **Files thay đổi** — tên file + loại thay đổi (add/modify/delete)
- **Symbols thay đổi** — method/class/interface nào bị sửa

---

## BƯỚC 2 — Tìm context liên quan (không đọc cả file)

### 2a. Locate đúng đoạn cần đọc

```bash
grep -n "TênMethod\|TênClass" FootballBlog.API/Path/To/File.cs | head -10
```

Lấy line number → Read file với `offset` + `limit` chỉ đoạn đó (±30 dòng).

### 2b. Tìm interface tương ứng

```bash
find . -name "IXxx*.cs" -path "*/Interfaces/*" | head -5
```

### 2c. Tìm callers của method bị sửa

```bash
grep -rn "TênMethod(" FootballBlog.API/ FootballBlog.Web/ --include="*.cs" | head -20
```

RTK compress kết quả thừa → chỉ giữ những dòng có giá trị.
Xác định: method sửa có làm vỡ contract không? Caller nào bị ảnh hưởng?

### 2d. Kiểm tra migration nếu có thay đổi Model/DbContext

```bash
ls -t FootballBlog.Infrastructure/Migrations/*.cs | head -3
```

So sánh migration mới nhất với entity thay đổi — có khớp không?

---

## BƯỚC 3 — Checklist review theo layer

Chỉ check những mục **liên quan** đến layer đang review.

### Service Layer
- [ ] Async/await đúng — không `.Result` / `.Wait()`
- [ ] Dùng `IUnitOfWork`, không inject DbContext trực tiếp
- [ ] `CommitAsync()` chỉ gọi 1 lần — không gọi trong repository
- [ ] Log đúng level: Debug bắt đầu | Info thành công | Warning not found | Error exception
- [ ] Trả DTO — không expose entity ra ngoài service

### Repository Layer
- [ ] Read-only query có `.AsNoTracking()`
- [ ] Có `.Select()` chỉ lấy fields cần — không load toàn bộ entity
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

## BƯỚC 4 — Output

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
