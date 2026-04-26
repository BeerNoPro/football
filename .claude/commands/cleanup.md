# Dọn Dẹp Context Sau Khi Implement

Khi được gọi (`/cleanup`):

## Bước 1 — Liệt kê plans hiện có

```bash
ls -la .claude/plans/ 2>/dev/null || echo "Không có plans/"
```

Đánh giá từng file:
- Đã implement xong → **Xóa**
- Implement một phần → Ghi phần còn lại vào TODO.md rồi **Xóa**
- Chưa implement → Giữ nguyên

## Bước 2 — Kiểm tra TODO.md

```bash
grep -n "⬜\|TODO\|pending" TODO.md | head -20
```

Đảm bảo tasks chưa xong đã được ghi vào TODO.md trước khi xóa plan.

## Bước 3 — Kiểm tra trùng lặp ngữ cảnh

```bash
# Xem nội dung commands/ và rules/ có khớp gì với CLAUDE.md không
grep -n "IUnitOfWork\|CommitAsync\|ApiResponse" CLAUDE.md | head -10
grep -rn "IUnitOfWork\|CommitAsync\|ApiResponse" .claude/rules/ | head -10
```

- CLAUDE.md vs rules/ → nếu thông tin trùng, xóa khỏi CLAUDE.md (rules load theo path)
- commands/ vs CLAUDE.md → nếu có hướng dẫn trùng nhau, xóa bản thừa
- TODO.md vs CLAUDE.md "Current Phase" → chỉ giữ ở TODO.md

## Bước 4 — Báo cáo

```
### Đã xóa
- `.claude/plans/xyz.md` — lý do

### Giữ nguyên
- `.claude/plans/abc.md` — lý do (chưa xong / còn dùng)

### Trùng lặp tìm thấy
- <mô tả nếu có>
```

**Không xóa:** CLAUDE.md, TODO.md, Bugs.md, rules/, hooks/, settings.json  
**Chỉ xóa khi chắc chắn** — hỏi lại nếu không rõ đã implement xong chưa
