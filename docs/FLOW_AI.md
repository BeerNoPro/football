
## Làm việc với AI (Claude Code)

> Dùng đúng lệnh + đúng context → AI hiểu đúng → ít rủi ro sửa nhầm.

### Slash Commands

| Command | Dùng khi nào |
|---------|-------------|
| `/fix-bug <mô tả>` | Có lỗi runtime / logic — AI đọc context trước, propose trước, apply sau khi bạn approve |
| `/debug-log [error\|jobs\|api]` | Đọc log hôm nay, tóm tắt lỗi, gợi ý nguyên nhân |
| `/new-feature <tên>` | Scaffold feature mới đúng cấu trúc project |
| `/migration <tên>` | Tạo EF Core migration an toàn, giải thích Up/Down trước khi apply |
| `/blazor-page <tên>` | Tạo Blazor SSR page mới |
| `/api-client <tên>` | Tạo typed HttpClient cho Web |
| `/review` | Review code trước khi commit — checklist correctness, security, perf |
| `/cleanup` | Dọn context cuối session, xóa plans đã xong |

---

### Nguyên tắc dùng AI trong project này

**1. Fix bug phải qua `/fix-bug`** — không gõ thẳng "fix lỗi X" vì AI sẽ sửa ngay mà không đọc đủ context.

**2. Propose trước, apply sau** — mọi `/fix-bug` và `/migration` đều output analysis block trước. Bạn đọc, đồng ý rồi mới reply "apply".

**3. Đóng conversation sau 15-20 message** — context quá dài làm AI mất ngữ cảnh quan trọng. Dùng `/cleanup` rồi mở tab mới.

---

### Prompt mẫu theo tình huống

#### Gặp lỗi runtime
```
/fix-bug
Lỗi: NullReferenceException tại MatchRepository.cs:87
Stacktrace: [paste từ logs/error/error-hôm-nay.log]
Xảy ra khi: gọi API GET /api/matches/upcoming
```

#### Lỗi không rõ nguyên nhân — đọc log trước
```
/debug-log error
```
Sau khi thấy lỗi → copy stacktrace → dùng `/fix-bug` như trên.

#### Thêm feature mới
```
/new-feature
Tên: LiveScoreService
Mô tả: Implement ILiveScoreService — poll Football API, cập nhật LiveMatch trong DB,
broadcast qua SignalR khi score thay đổi.
Layer: Core/Services + Infrastructure (nếu cần repo mới)
Liên quan: ILiveScoreService (interface đã có), LiveMatch entity, LiveScorePollingJob
```

#### Thêm cột / đổi schema DB
```
/migration AddViewCountToPosts
Thay đổi: thêm cột ViewCount (int, default 0) vào bảng Posts
```

#### Tạo trang Blazor mới
```
/blazor-page
Tên: LiveScore/MatchDetail
Render mode: InteractiveServer (cần realtime)
Data: MatchSummaryDto, LiveMatchDto
Route: /match/{id:int}
```

#### Tạo UI prototype
```
Tạo prototype HTML cho trang MatchDetail.
Tham khảo design system từ prototype/home.html.
Sections cần có: match header (teams + score + status), timeline events, AI prediction panel.
Mock data: Man City vs Arsenal, 2-1, phút 67.
```

#### Review trước khi commit
```
/review
Focus: PostService.cs và PostsController.cs vừa sửa
Kiểm tra thêm: có N+1 query không? AsNoTracking đúng chỗ chưa?
```

#### Hỏi về code hiện tại
```
Đọc FootballBlog.API/Jobs/LiveScorePollingJob.cs và giải thích
adaptive gate hoạt động như thế nào. Chỉ đọc file đó, không sửa gì.
```

---

### Mẫu prompt tốt vs xấu

| Xấu — dễ sửa nhầm | Tốt — đủ context |
|--------------------|-----------------|
| "fix lỗi null reference" | `/fix-bug` + paste stacktrace đầy đủ |
| "thêm field vào Match" | `/migration AddVenueToMatch` + mô tả kiểu dữ liệu |
| "tạo service dự đoán" | `/new-feature` + nêu interface đã có, layer nào, liên quan gì |
| "sửa query cho nhanh hơn" | Chỉ rõ file:line, đính kèm query hiện tại, mô tả vấn đề perf |
