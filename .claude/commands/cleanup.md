# Dọn Dẹp Context Sau Khi Implement

Khi được gọi (`/cleanup`):

1. Liệt kê các file trong `.claude/plans/` và đánh giá từng file:
   - Đã implement xong → **Xóa**
   - Implement một phần → Ghi phần còn lại vào TODO.md rồi **Xóa**
   - Chưa implement → Giữ nguyên

2. Kiểm tra trùng lặp ngữ cảnh giữa các file:
   - CLAUDE.md vs rules/ → nếu thông tin trùng, xóa khỏi CLAUDE.md (rules load theo path)
   - commands/ vs CLAUDE.md → nếu có hướng dẫn trùng nhau, xóa bản thừa
   - TODO.md vs CLAUDE.md "Current Phase" → chỉ giữ ở TODO.md

3. Báo cáo: file nào đã xóa, file nào giữ và lý do

**Không xóa:** CLAUDE.md, TODO.md, Bugs.md, rules/, hooks/, settings.json
**Chỉ xóa khi chắc chắn** — hỏi lại nếu không rõ đã implement xong chưa
