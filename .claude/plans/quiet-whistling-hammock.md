# Plan: Flashscore Homepage — Static HTML

## Context
Tạo 1 file HTML tĩnh clone layout flashscore.vn để dùng làm prototype UI.
Workflow: HTML tĩnh → review/chỉnh → approve → tách Blazor components.
File này KHÔNG cần server, mở thẳng trên browser.

## Yêu cầu
- **Theme**: Dark (nền tối, giống flashscore gốc)
- **Data**: Mock data thật (~10-15 trận V.League, EPL, UCL)
- **Sections**: Header + Nav, Date bar, Sidebar leagues, Match list
- **Tech**: 1 file HTML duy nhất, Tailwind CDN (không cần build)

## Output file
```
FootballBlog.Web/wwwroot/prototype/flashscore-home.html
```

---

## Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│  HEADER: Logo | [⚽ Bóng Đá] [🎾 Tennis] ... | 🔍 | Đăng nhập │
├─────────────────────────────────────────────────────────────┤
│  DATE BAR: < Thứ 5 | Thứ 6 | [Hôm nay] | Thứ 2 | Thứ 3 > │
├──────────────┬──────────────────────────────────────────────┤
│  SIDEBAR     │  TABS: [Live 3] [Lịch thi đấu] [Kết quả]   │
│  ─────────   │  ──────────────────────────────────────────  │
│  ★ Yêu thích │  🏴󠁧󠁢󠁥󠁮󠁧󠁿 PREMIER LEAGUE          [▼]      │
│  🇻🇳 V.League │  20:00  Man City     1 - 0  Arsenal  [LIVE] │
│  🏴󠁧󠁢󠁥󠁮󠁧󠁿 EPL     │  22:15  Chelsea      2 - 1  Liverpool        │
│  🇩🇪 Bundes.. │                                              │
│  🇪🇸 La Liga  │  🇻🇳 V.LEAGUE 1                   [▼]      │
│  🏆 UCL      │  17:00  Hà Nội FC    1 - 2  HAGL    [KT]  │
│              │  19:15  TPHCM FC     0 - 0  Bình Dương     │
│  [Thêm giải] │                                              │
│              │  🏆 UEFA CHAMPIONS LEAGUE        [▼]      │
│              │  02:00  Real Madrid  3 - 1  Bayern          │
└──────────────┴──────────────────────────────────────────────┘
│  FOOTER: Links | App download | Social                       │
└─────────────────────────────────────────────────────────────┘
```

---

## Color Palette (Dark Theme)

| Element              | Color          |
|----------------------|----------------|
| Background chính     | `#1a1a2e`      |
| Header background    | `#16213e`      |
| Sidebar background   | `#0f3460`      |
| Card/row background  | `#1e2a3a`      |
| Row hover            | `#243447`      |
| Text chính           | `#e0e0e0`      |
| Text phụ (time)      | `#8899aa`      |
| Accent (score/live)  | `#e94560`      |
| Live badge           | `#ff4444` blink|
| Border               | `#2a3a4a`      |
| Nav active           | `#f97316` (orange) |

---

## Mock Data

### V.League 1
| Giờ   | Home       | Score | Away          | Status |
|-------|------------|-------|---------------|--------|
| 17:00 | Hà Nội FC  | 1 - 2 | HAGL          | KT     |
| 19:15 | TPHCM FC   | 0 - 0 | Bình Dương    | 65'    |
| 19:15 | Hải Phòng  | -     | Viettel FC    | 20:00  |

### Premier League
| Giờ   | Home       | Score | Away          | Status |
|-------|------------|-------|---------------|--------|
| 20:00 | Man City   | 1 - 0 | Arsenal       | LIVE 78'|
| 20:00 | Chelsea    | 2 - 1 | Liverpool     | KT     |
| 22:15 | Tottenham  | -     | Man United    | 22:15  |
| 22:15 | Newcastle  | -     | Everton       | 22:15  |

### UEFA Champions League
| Giờ   | Home        | Score | Away          | Status |
|-------|-------------|-------|---------------|--------|
| 02:00 | Real Madrid | 3 - 1 | Bayern Munich | KT     |
| 02:00 | Barcelona   | 2 - 2 | PSG           | KT     |
| 02:00 | Inter Milan | -     | Atletico      | 02:00  |

### Bundesliga
| Giờ   | Home        | Score | Away      | Status |
|-------|-------------|-------|-----------|--------|
| 21:30 | Bayern      | 4 - 0 | Dortmund  | KT     |

---

## Implementation Steps

1. **Tạo file** `FootballBlog.Web/wwwroot/prototype/flashscore-home.html`
2. **Header**: Logo + sport nav tabs + search icon + login button
3. **Date bar**: 7 ngày cuộn ngang, highlight "Hôm nay"
4. **Body layout**: CSS Grid 2 cột (sidebar 240px | main 1fr)
5. **Sidebar**: Danh sách giải có icon flag + toggle active
6. **Main tabs**: Live / Lịch thi đấu / Kết quả (toggle class active)
7. **Match groups**: Mỗi giải là 1 section có header league + rows trận đấu
8. **Match row**: time | home | score | away | status badge
9. **Status badges**: LIVE (đỏ nhấp nháy) | KT (xám) | giờ thi đấu (cam)
10. **Footer**: Links + app + social icons (SVG inline)
11. **JS tối giản**: Toggle tab active, sidebar item active (không cần framework)

---

## Verification
- Mở file trực tiếp trên browser (file:// hoặc Live Server VSCode)
- Check responsive tại 1280px, 1440px
- Dark theme nhìn rõ tất cả text
- Click tab Live/Kết quả chuyển đúng
- Click sidebar giải highlight đúng item

---

## UI Workflow Memory (lưu vào memory sau khi done)
Workflow chuẩn cho mọi trang UI mới:
1. User cung cấp URL tham khảo hoặc ảnh
2. Fetch/phân tích layout → hỏi theme + data + sections
3. Tạo HTML tĩnh 1 file (Tailwind CDN)
4. User review → approve
5. Tách Blazor components
