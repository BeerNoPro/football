# Fix Bug — UAT Investigation Workflow

Khi được gọi (`/fix-bug <mô tả lỗi hoặc stacktrace>`):

---

## BƯỚC 1 — Thu thập context đầy đủ (KHÔNG sửa code)

### 1a. Đọc log

- Nếu user cung cấp stacktrace → parse: exception type, message, file:line
- Nếu không có → đọc `logs/error/web-error-{hôm nay}.log`, `logs/app/app-{hôm nay}.log`, `logs/build/build-{hôm nay}.log`
- Xác định: lỗi xảy ra ở layer nào? Trigger là gì (request / job / user action)?

### 1b. Trace toàn bộ luồng liên quan

Từ file:line trong stacktrace, dùng `Grep` + `Glob` + `Read` (offset+limit) để trace:

```
1. Class/method bị lỗi trực tiếp
2. Callers — ai gọi method đó?
3. Interface — implementation nào được inject?
4. Layer trên: Controller → Service → Repository → DB/HTTP
5. Nếu là Blazor: Component → ApiClient → API Controller → Service
```

**Quy tắc đọc file:**
- `Grep` pattern → tìm symbol trước
- `Glob` → xác định path chính xác
- `Read` với `offset+limit` → chỉ đọc đoạn liên quan, KHÔNG đọc cả file

### 1c. Đọc Bugs.md

Kiểm tra xem lỗi này có liên quan đến architectural decision hoặc known issue đã ghi trong `Bugs.md` không.

### 1d. Đọc rule tương ứng với layer bị lỗi

| Layer | Rule file |
|-------|-----------|
| Repository / EF Core | `.claude/rules/database.md` |
| Controller / API endpoint | `.claude/rules/api.md` |
| Blazor Component / Page | `.claude/rules/blazor.md` |
| Logging / Serilog | `.claude/rules/logging.md` |
| Security / Auth | `.claude/rules/security.md` |
| UI / Prototype | `.claude/rules/ui-design.md` |

### 1e. Xác định skill phù hợp

Sau khi hiểu rõ bug, xác định fix cần dùng skill nào:

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
2. Build check tự động sau mỗi Edit/Write
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
