# UI Design — Workflow & Design System

## Cách làm UI trong dự án này

Không dùng Figma. UI được thiết kế trực tiếp bằng cách:
1. Lấy tham khảo từ **URL web** hoặc **ảnh screenshot** do user cung cấp
2. Tạo **file HTML tĩnh** để xem trực tiếp trên browser
3. User review, chỉnh sửa, approve rồi mới tách Blazor component

---

## Workflow chuẩn khi có yêu cầu UI mới

### Bước 1 — Nhận input tham khảo
| Input | Cách xử lý |
|-------|-----------|
| URL web | Dùng `WebFetch` phân tích layout, color, typography |
| Ảnh / screenshot | Đọc ảnh trực tiếp (user paste vào chat) |
| Mô tả bằng lời | Hỏi thêm trước khi code |

### Bước 2 — Hỏi 3 điều trước khi code
1. **Theme**: Dark / Light? (default: Dark theo design system dự án)
2. **Data**: Mock data tên thật hay placeholder?
3. **Sections**: Liệt kê các section cần làm để confirm scope

### Bước 3 — Tạo HTML tĩnh
- **Output**: `FootballBlog.Web/wwwroot/prototype/<tên-trang>.html`
- **Tech**: 1 file HTML duy nhất + Tailwind CDN — không cần build, không cần server
- Mở bằng Live Server (VSCode) hoặc `file://` trực tiếp
- JS tối giản: chỉ toggle class, không dùng React/Vue/Alpine

### Bước 4 — User review & approve
- User mở browser, kiểm tra trực quan, chỉnh trong DevTools nếu cần
- Có thể paste ảnh mới vào chat để so sánh và điều chỉnh

### Bước 5 — Tách Blazor components (sau khi approve)
- **Public pages**: Tailwind CSS → `FootballBlog.Web/Components/`
- **Admin pages**: MudBlazor → `FootballBlog.Web/Components/Admin/`

---

## Design System (Dark Theme)

File tham chiếu chính: `prototype/combined-home.html`

### Color Tokens
```css
--bg:          #141414   /* background chính */
--bg-sidebar:  #111111   /* sidebar */
--bg-card:     #1c1c1c   /* card, panel */
--bg-dark:     #0d0d0d   /* league header, row đặc biệt */
--accent:      #c8f04d   /* lime — highlight, active, CTA */
--accent-dim:  rgba(200,240,77,0.10)
--text:        #efefef   /* text chính */
--muted:       #666      /* text phụ, placeholder */
--muted2:      #999      /* text phụ mức 2 */
--border:      #242424   /* border mặc định */
--border2:     #2e2e2e   /* border nhấn */
--live:        #4ade80   /* xanh lá — live indicator */
--live-dim:    rgba(74,222,128,0.12)
```

### Layout chuẩn (3 cột)
```
┌──────────────┬──────────────────────┬────────────┐
│ Left 240px   │ Center (1fr)         │ Right 300px│
│ Sidebar Nav  │ Main content         │ AI Blog    │
└──────────────┴──────────────────────┴────────────┘
height: 100vh, overflow: hidden, mỗi cột scroll độc lập
```

### Thành phần tái sử dụng đã có trong combined-home.html
- **Left sidebar**: search + country tree (collapse/expand) + settings + user/logout
- **Hero banner**: gradient dark + accent tag + title
- **Match toolbar**: live pill + search + filter dropdown
- **Tab + Date bar**: cùng 1 hàng, gọn
- **League group**: collapse/expand, dark header
- **Match row**: 5 cột — time | home+logo | score | away+logo | badge
- **Status badges**: LIVE (xanh), KT (xám), scheduled (lime)
- **Right panel AI blog**: featured post + list với AI prediction score

---

## Prototype Files

| File | Vai trò | Ghi chú |
|------|---------|---------|
| `prototype/combined-home.html` | **File chính — reference** | Style FootballBeer + data FlashScore + AI blog |
| `prototype/footballbeer-home.html` | Bản nháp style gốc | Lấy từ ảnh Figma community |
| `prototype/flashscore-home.html` | Bản nháp data gốc | Clone flashscore.vn layout |

> Khi tạo trang mới, **copy style từ combined-home.html** làm base, không bắt đầu từ scratch.

---

## Quy tắc agent cần nhớ

- Không bao giờ bắt đầu code UI mà không có tham khảo (URL hoặc ảnh)
- Không tạo file mới khi có thể mở rộng file prototype hiện có
- Lưu rule vào `.claude/rules/` của project, không lưu vào system memory
- Sau khi approve prototype → xóa plan file, giữ lại prototype HTML
