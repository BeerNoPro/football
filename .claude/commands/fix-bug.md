# Fix Bug — Structured Workflow

Khi được gọi (`/fix-bug <mô tả lỗi hoặc file log>`):

---

## BƯỚC 1 — Thu thập context (KHÔNG sửa code ở bước này)

### 1a. Đọc error log nếu có
- Nếu user cung cấp log/stacktrace → parse lấy: exception type, message, file:line
- Nếu không có → đọc `logs/error/error-{hôm nay}.log` và `logs/app/app-{hôm nay}.log`
- Xác định: lỗi xảy ra ở layer nào (Controller / Service / Repository / Job)?

### 1b. Locate affected code
Từ stacktrace hoặc mô tả, dùng `Grep` tìm:
- Class / method / interface liên quan
- File nào implement interface đó
- File nào **gọi** method đó (callers)

```
Grep pattern → tìm symbol
Glob → xác định file path
Read với offset+limit → chỉ đọc đoạn liên quan, KHÔNG đọc cả file
```

### 1c. Đọc Bugs.md
Kiểm tra xem lỗi này có liên quan đến architectural decision đã ghi ở `Bugs.md` không.

### 1d. Đọc rule liên quan
Tùy layer bị lỗi, đọc rule tương ứng:
- Repository / EF → `.claude/rules/database.md`
- Controller / API → `.claude/rules/api.md`
- Blazor → `.claude/rules/blazor.md`
- Logging → `.claude/rules/logging.md`

---

## BƯỚC 2 — Phân tích và Đề xuất (CHƯA sửa code)

Output bắt buộc theo format:

```
## Bug Analysis

**Lỗi:** <mô tả ngắn gọn>
**Nguyên nhân:** <giải thích tại sao xảy ra>
**Layer bị ảnh hưởng:** Controller / Service / Repository / Model / Job

## Files sẽ thay đổi
- `path/to/file.cs` — <thay đổi gì>
- `path/to/other.cs` — <thay đổi gì> (nếu có)

## Files KHÔNG thay đổi (đã kiểm tra, không liên quan)
- `path/to/safe.cs` — <lý do không cần sửa>

## Proposed Fix
<mô tả code thay đổi bằng lời hoặc diff ngắn>

## Risk
<có breaking change không? migration cần không? test nào có thể fail?>

---
Reply **"apply"** để tôi thực hiện, hoặc **"không"** nếu muốn điều chỉnh.
```

---

## BƯỚC 3 — Apply (CHỈ khi user reply "apply" hoặc tương đương)

1. Sửa code theo đúng proposed fix đã được approve
2. Chạy build check (hook tự động sau Edit/Write)
3. Nếu cần migration → hỏi riêng, không tự apply
4. Tóm tắt thay đổi thực tế (không lặp lại toàn bộ analysis)

---

## Nguyên tắc bất biến

- **KHÔNG bao giờ** sửa file ở Bước 1 hoặc Bước 2
- **KHÔNG** sửa file không có trong danh sách "Files sẽ thay đổi"
- **KHÔNG** refactor code xung quanh — chỉ fix đúng bug được báo
- Nếu fix cần đụng >3 files → cảnh báo trước, giải thích tại sao
