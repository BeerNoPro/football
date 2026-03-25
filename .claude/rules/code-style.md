# C# Code Style Rules

## Naming
- Class, Method, Property: PascalCase (e.g., `PostService`, `GetById`)
- Variable, parameter: camelCase (e.g., `postId`, `slugText`)
- Interface: prefix I (e.g., `IPostRepository`)
- Private field: prefix _ (e.g., `_dbContext`)

## Async/Await
- Tất cả DB calls và HTTP calls phải dùng async/await
- Method async phải có hậu tố Async (e.g., `GetPostAsync`, `CreatePostAsync`)
- Không dùng `.Result` hoặc `.Wait()` — gây deadlock

## Dependency Injection
- Inject qua constructor, không dùng service locator
- Đăng ký services trong Program.cs hoặc extension methods

## Error Handling
- Dùng try/catch ở tầng API controller, không bắt lỗi ở tầng service trừ khi có lý do cụ thể
- Log lỗi bằng ILogger, không dùng Console.WriteLine

## Comments
- Comment bằng tiếng Việt nếu logic phức tạp
- Không comment code hiển nhiên
- Dùng XML doc (`///`) cho public API methods

## General
- Không dùng `var` cho kiểu không rõ ràng
- Prefer expression body cho method 1 dòng
- Không để code thừa (unused using, dead code)
