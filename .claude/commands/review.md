# Review Code Trước Khi Commit

Khi được gọi (`/review`):

1. Chạy `git diff --staged` để xem các thay đổi đã stage
2. Nếu chưa có gì staged, chạy `git diff` để xem unstaged changes
3. Review theo checklist:

**Correctness**
- [ ] Logic có đúng với yêu cầu không?
- [ ] Edge cases đã được xử lý chưa?
- [ ] Async/await dùng đúng chưa?

**Security**
- [ ] Có SQL injection, XSS risk không?
- [ ] Input validation đầy đủ chưa?
- [ ] Không có secret/password hardcode?

**Performance**
- [ ] Query DB có dùng AsNoTracking() cho read-only không?
- [ ] Có N+1 query problem không?
- [ ] Blazor component có dùng đúng render mode không?

**Code Style (theo .claude/rules/code-style.md)**
- [ ] Naming conventions đúng chưa?
- [ ] Không có dead code, unused imports?

4. Tóm tắt: những gì ổn, những gì cần sửa trước khi commit
5. KHÔNG tự động chạy `git commit` — để người dùng quyết định
