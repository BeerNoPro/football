# Fix Bug — UAT Investigation Workflow

Khi được gọi (`/fix-bug <mô tả lỗi hoặc stacktrace>`):

---

## BƯỚC 1 — Thu thập context đầy đủ (KHÔNG sửa code)

### 1a. Đọc log

Nếu user cung cấp stacktrace → parse: exception type, message, file:line.

Nếu không có stacktrace → đọc log hôm nay:
```bash
grep -n "ERR\|FTL\|Exception" logs/error/error-$(date +%Y-%m-%d).log | tail -50
grep -n "ERR\|FTL\|Exception" logs/app/app-$(date +%Y-%m-%d).log | tail -50
```

Xác định: lỗi xảy ra ở layer nào? Trigger là gì (request / job / user action)?

### 1b. Trace toàn bộ luồng liên quan

Từ file:line trong stacktrace, dùng bash để trace:

```bash
# Tìm class/method bị lỗi
grep -rn "TênClass\|TênMethod" FootballBlog.API/ FootballBlog.Core/ --include="*.cs" | head -20

# Tìm callers — ai gọi method đó?
grep -rn "TênMethod(" FootballBlog.API/ FootballBlog.Web/ FootballBlog.Core/ --include="*.cs" | head -20

# Tìm interface implementation
grep -rn "IXxxService\|: XxxService" FootballBlog.Infrastructure/ --include="*.cs" | head -10
```

RTK hook tự wrap tất cả lệnh bash → output được compress, tiết kiệm token.

Luồng trace theo thứ tự:
```
1. Class/method bị lỗi trực tiếp
2. Callers — ai gọi method đó?
3. Interface — implementation nào được inject?
4. Layer trên: Controller → Service → Repository → DB/HTTP
5. Nếu là Blazor: Component → ApiClient → API Controller → Service
```

Đọc đúng đoạn cần thiết — không đọc cả file:
```bash
# Xem quanh line bị lỗi (±20 dòng)
sed -n '{start},{end}p' FootballBlog.API/Path/To/File.cs
```

### 1c. Kiểm tra Bugs.md

```bash
grep -n "từ khóa liên quan" Bugs.md | head -10
```

Xem lỗi này có liên quan đến architectural decision hoặc known issue không.

### 1d. Đọc rule tương ứng với layer bị lỗi

| Layer | Rule file |
|-------|-----------|
| Repository / EF Core | `.claude/rules/database.md` |
| Controller / API endpoint | `.claude/rules/api.md` |
| Blazor Component / Page | `.claude/rules/blazor.md` |
| Logging / Serilog | `.claude/rules/logging.md` |
| Security / Auth | `.claude/rules/security.md` |

### 1e. Xác định skill phù hợp

| Tình huống | Skill |
|-----------|-------|
| Sửa code hiện có (< 3 files) | Fix trực tiếp trong workflow này |
| Cần tạo/sửa API client | `/api-client` |
| Cần tạo/sửa Blazor page | `/blazor-page` |
| Cần EF migration mới | `/migration` |
| Fix kéo theo feature mới | `/new-feature` |

---

## BƯỚC 2 — Phân tích và Đề xuất (CHƯA sửa code)

Output bắt buộc theo format:

```
## Bug Analysis

**Lỗi:** <mô tả ngắn gọn>
**Nguyên nhân gốc (root cause):** <giải thích chuỗi nguyên nhân — không dừng ở triệu chứng>
**Layer bị ảnh hưởng:** Controller / Service / Repository / Model / Job / Blazor

## Trace luồng lỗi
<Mô tả ngắn: A gọi B gọi C → C throw → B propagate → A hiện lỗi>

## Files đã điều tra
- `path/to/file.cs:line` — <phát hiện gì>
- `path/to/other.cs:line` — <phát hiện gì>

## Files sẽ thay đổi
- `path/to/file.cs` — <thay đổi gì, tại sao>

## Files KHÔNG thay đổi (đã kiểm tra)
- `path/to/safe.cs` — <lý do không liên quan>

## Proposed Fix
<mô tả thay đổi bằng lời hoặc diff ngắn>

## Skill cần dùng
<fix trực tiếp / hoặc: "Sau khi apply fix này → dùng /xxx để ...">

## Risk
<breaking change? migration cần không? test nào có thể fail? side effect nào?>

---
Reply **"apply"** để thực hiện, hoặc **"không"** nếu muốn điều chỉnh.
```

---

## BƯỚC 3 — Apply (CHỈ khi user reply "apply" hoặc tương đương)

1. Sửa code theo đúng proposed fix đã được approve
2. Build check sau mỗi thay đổi:
```bash
dotnet build FootballBlog.sln --no-restore -v minimal 2>&1 | grep -E "error|warning|Build" | tail -20
```
3. Nếu fix cần migration → hỏi riêng, không tự apply
4. Nếu fix cần skill khác → gợi ý dùng skill đó ngay sau khi apply xong
5. Tóm tắt thay đổi thực tế (không lặp lại toàn bộ analysis)

---

## Nguyên tắc bất biến

- **KHÔNG bao giờ** sửa file ở Bước 1 hoặc Bước 2
- **KHÔNG** sửa file không có trong danh sách "Files sẽ thay đổi"
- **KHÔNG** refactor code xung quanh — chỉ fix đúng bug được báo
- **KHÔNG** dừng ở triệu chứng — phải tìm root cause (chuỗi nguyên nhân thực sự)
- Nếu fix cần đụng >3 files → cảnh báo trước, giải thích tại sao
- UAT mode: ưu tiên hiểu hệ thống trước khi đề xuất — không đoán mò
