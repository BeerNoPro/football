---
paths:
  - "**/*.cs"
  - "**/*.razor"
  - "**/*.razor.cs"
---

# C# Code Style

- Naming: Class/Method/Property → PascalCase | variable/param → camelCase | interface → `IFoo` | private field → `_foo`
- Async: tất cả DB+HTTP dùng `async/await` với hậu tố `Async` — KHÔNG `.Result`/`.Wait()`
- DI: inject qua constructor | không service locator
- Error: try/catch ở controller | log bằng `ILogger<T>`, không `Console.WriteLine`
- Comments: tiếng Việt cho logic phức tạp | XML `///` cho public API methods
- Misc: không `var` khi kiểu mơ hồ | expression body cho 1 dòng | xóa unused using/dead code
