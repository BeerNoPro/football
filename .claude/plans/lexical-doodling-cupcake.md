# BA/SA Analysis — Football App: Business Requirements & Deliverables

## Context

Người dùng yêu cầu phân tích toàn diện nghiệp vụ ứng dụng bóng đá từ góc độ BA + SA:
- Định hình lại các module nghiệp vụ chính (match display, AI prediction, blog, telegram)
- Bổ sung các trang HTML prototype còn thiếu (dựa trên research FlashScore / SofaScore / WhoScored)
- Cập nhật TODO.md với business rules cụ thể
- Cập nhật CLAUDE.md với service abstractions mới
- Dọn dẹp plan file đã hoàn thành (`glimmering-tinkering-gadget.md`)

Trạng thái hiện tại: Phase 1 ✅, 15 HTML prototypes ✅ (tất cả file trong plan cũ đã tồn tại)

---

## 1. BUSINESS MODULE MAP

### Module 1 — Match Center (Core UX)

**Mục tiêu:** Người dùng xem lịch thi đấu, theo dõi live score, xem chi tiết từng trận.

**Business rules:**
- Hiển thị matches grouped by league → sorted by kickoff time
- Tabs: Live | Today | Tomorrow | Upcoming (7 ngày)
- Live matches: poll mỗi 30s qua SignalR (chỉ khi có live match)
- Match detail layers (mỗi click đào sâu hơn):
  - Layer 0: Match row (home | score | away | status)
  - Layer 1: match-detail.html (Events + Stats + Lineups + H2H)
  - Layer 2: Team profile (click tên đội → team-profile.html)
  - Layer 3: Player detail (click cầu thủ → player card popup, không cần full page)
- Status badge logic: `LIVE` (xanh pulse) | `HT` (half-time, xám) | `FT` (kết thúc) | giờ đấu (lime)
- H2H: lấy 5 trận gần nhất, show W/D/L streak

**Pages:** home.html ✓, match-detail.html ✓, league-page.html ✓, **team-profile.html ← CẦN TẠO**

---

### Module 2 — AI Prediction Engine (Core Value Prop)

**Mục tiêu:** Tự động phân tích trận đấu sắp diễn ra và tạo ra dự đoán có giá trị.

**Business rules — Prediction Pipeline:**
```
T-48h: FetchUpcomingMatchesJob       → lấy matches từ Football API, lưu DB
T-24h: FetchMatchContextJob          → lấy h2h, form, lineups, referee stats
T-24h: GeneratePredictionJob         → gọi AI với MatchContext prompt
T-24h: PublishPredictionJob          → tạo blog post + gửi Telegram
T+0:   LiveScorePollingJob           → cập nhật live events
T+FT:  UpdatePredictionResultJob     → so sánh dự đoán vs kết quả thực
T+FT+1h: EditTelegramMessageJob      → edit Telegram message với kết quả thực
```

**Confidence Score tiers:**
- ≥ 75: "Dự đoán chắc chắn" — auto publish blog + Telegram
- 50-74: "Dự đoán trung bình" — publish Telegram, blog cần admin review
- < 50: "Dự đoán yếu" — chỉ lưu DB, không publish

**Prediction data model (per match):**
- Score prediction: `PredictedHomeScore : PredictedAwayScore`
- Outcome: Home Win / Draw / Away Win
- Event predictions: Total Goals O/U 2.5, Both Teams Score Y/N, Cards > 3
- Confidence: 0-100 (AI-generated)
- Analysis text: 200-400 từ tiếng Việt

**MatchContext object (input cho AI prompt):**
```json
{
  "match": { "home": "...", "away": "...", "league": "...", "kickoff": "..." },
  "form": { "home": ["W","W","D","L","W"], "away": ["L","D","W","W","D"] },
  "h2h": [{ "date": "...", "score": "2-1", "winner": "home" }],
  "homeStats": { "avgGoalsFor": 1.8, "avgGoalsAgainst": 0.9, "cleanSheets": 5 },
  "awayStats": { "avgGoalsFor": 1.2, "avgGoalsAgainst": 1.4, "cleanSheets": 2 },
  "lineups": { "home": [...], "away": [...] },
  "referee": { "name": "...", "avgCardsPerGame": 4.2, "avgGoalsPerGame": 2.8 }
}
```

**Pages:** admin-predictions.html ✓, **predictions.html (public) ← CẦN TẠO**

---

### Module 3 — Blog & Content

**Mục tiêu:** Nội dung tự động từ AI prediction + bài viết thủ công từ admin.

**Business rules:**
- AI blog post: auto-generated từ prediction, slug = `{home}-vs-{away}-{date}-prediction`
- Blog post structure: Hero match card → Prediction box (score + confidence) → Analysis text → Stats comparison → Related matches
- SEO: title `{Home} vs {Away} Prediction – {League} {Date} | FootballBlog`
- Draft → Published flow: AI posts publish ngay nếu confidence ≥ 75, còn lại require admin review
- Categories mặc định cho AI posts: "Dự đoán AI" category tự động assign
- Tags: tên 2 đội + tên league + "dự đoán"

**Pages:** post-detail.html ✓, category-tag.html ✓, search-results.html ✓

---

### Module 4 — Telegram Notification

**Mục tiêu:** Gửi dự đoán tới Telegram channel, cập nhật sau trận.

**Business rules:**
- Send prediction message T-24h với format:
  ```
  🏟 Premier League — Man City vs Arsenal
  📅 Ngày 15/04/2026 | 20:00 ICT
  
  🤖 Dự đoán AI (Độ tin cậy: 78%)
  ⚽ Tỉ số: 2 - 1 (Man City thắng)
  📊 Over 2.5 Goals: Có
  🟨 Tổng thẻ: 3-4
  
  📝 Phân tích: Man City đang trong phong độ tốt...
  🔗 [Xem phân tích đầy đủ](link)
  ```
- Edit message sau FT với kết quả thực (green ✅ hoặc red ❌)
- Lưu `TelegramMessageId` trong `MatchPrediction` để edit được
- Multi-channel: `DefaultChatId` (general) và `PredictionChannelId` (prediction-only)

---

### Module 5 — Admin Panel

**Mục tiêu:** Quản lý toàn bộ nội dung + monitoring jobs.

**Business rules:**
- Dashboard: KPIs (bài viết hôm nay, predictions pending, API quota còn lại, Telegram messages sent)
- Prediction management: filter by status (Generated/Published/Failed), manual retrigger
- Job monitoring: xem Hangfire dashboard tại `/hangfire` (protected)
- Post editor: rich text (Quill.js), thumbnail upload → S3/local, SEO preview
- Categories/Tags: slug auto-generate từ tên

**Pages:** admin-*.html ✓ (6 pages đã có)

---

## 2. HTML PROTOTYPES CẦN TẠO MỚI

Dựa trên BA analysis + research FlashScore/SofaScore, **2 trang quan trọng còn thiếu:**

### P7: `predictions.html` — Public Predictions Hub

**Mô tả:** Trang tổng hợp tất cả dự đoán AI cho các trận sắp đấu. Đây là landing page chính thu hút user.

**Layout:** 3 cột (giống home.html)

**Center panel — sections:**
1. **Header:** "Dự đoán AI" + subtitle "Phân tích tự động bởi Claude AI"
2. **Filter bar:** League selector | Date picker | Confidence threshold slider
3. **Featured prediction** (card lớn): trận đấu lớn nhất hôm nay, hiển thị nổi bật
4. **Prediction list** grouped by league:
   - Prediction card: match header (logo + tên + giờ) + prediction row (score dự đoán + outcome badge) + confidence meter (thanh ngang có % label) + mini stats (O/U, BTTS) + link "Xem phân tích"
5. **Accuracy tracker** ở cuối: "Tháng này: 68% chính xác | 42 dự đoán"

**Right panel:** AI blog posts liên quan (giống home.html)

**Mock data:** 3-4 leagues, 8-10 predictions, mix confidence levels

**File:** `FootballBlog.Web/wwwroot/prototype/predictions.html`

---

### P8: `team-profile.html` — Team Profile Page

**Mô tả:** Trang chi tiết đội bóng, user click vào tên đội từ match row hoặc standings.

**Layout:** 3 cột (giống home.html)

**Center panel — sections:**
1. **Team header:** Logo lớn + tên đội + league + mùa giải
2. **Season stats bar:** Vị trí BXH | Điểm | Thắng/Hòa/Thua | Bàn thắng/thủng lưới
3. **Tabs:** [Lịch đấu | Kết quả | Đội hình | Thống kê | H2H]
4. **Tab Lịch đấu:** upcoming matches (dùng lại `.match-row`)
5. **Tab Kết quả:** last 10 matches (badge W/D/L + score)
6. **Tab Đội hình:** squad list grouped by position (GK/DEF/MID/FWD), mỗi cầu thủ: số áo + tên + quốc tịch
7. **Tab Thống kê:** avg goals for/against per game, clean sheets, form chart (5 kết quả gần nhất W/D/L icons)
8. **Tab H2H:** dropdown chọn đội để so sánh, history 5 trận gần nhất

**Mock data:** Man City — Premier League 2025/26

**File:** `FootballBlog.Web/wwwroot/prototype/team-profile.html`

---

### A7: `admin-job-monitor.html` — Job Monitoring Dashboard

**Mô tả:** Admin xem trạng thái Hangfire jobs + API quota.

**Layout:** Admin layout (admin-common.css)

**Sections:**
1. **API Quota card:** Football API — 47/100 req hôm nay (thanh ngang màu lime → đỏ khi gần hết)
2. **Active jobs:** Live table các job đang chạy (Job name | Status | Last run | Next run | Duration)
3. **Job history:** 20 dòng gần nhất (Success/Failed badge + timestamp + duration)
4. **Failed jobs detail:** expandable row với error message + stack trace preview
5. **Manual trigger buttons:** "Fetch Matches Now" | "Run Predictions" | "Send Telegram"
6. **Telegram status:** Messages sent today | Last message timestamp | Bot status

**File:** `FootballBlog.Web/wwwroot/prototype/admin-job-monitor.html`

---

## 3. CẬP NHẬT TODO.md

**Thêm vào Phase 2:**
```markdown
- [ ] HTML prototype: predictions.html (public AI predictions hub)
- [ ] HTML prototype: team-profile.html (team detail page)
- [ ] HTML prototype: admin-job-monitor.html (Hangfire + API quota monitoring)
```

**Thêm vào Phase 4 (chi tiết hơn):**
```markdown
- [ ] FetchMatchContextJob: lấy h2h, form 5 trận, referee stats từ Football API
- [ ] MatchContextData JSONB field trên Match entity (lưu raw context cho AI)
- [ ] Redis cache: match context TTL 2h, standings TTL 30min, team stats TTL 1h
- [ ] API quota tracker: Redis counter reset mỗi ngày UTC 00:00, alert khi ≤ 20 req còn
```

**Thêm vào Phase 5 (chi tiết hơn):**
```markdown
- [ ] Prompt template entity: lưu DB (version, template text, expected_output_schema)
- [ ] A/B test provider: 70% Claude, 30% Gemini để so sánh chất lượng
- [ ] Confidence tier routing: ≥75 auto-publish, 50-74 pending review, <50 discard
- [ ] UpdatePredictionResultJob: so sánh predicted vs actual sau FT, cập nhật accuracy
- [ ] AccuracyStats table: monthly aggregate (total, correct, correct_outcome, correct_score)
```

**Thêm Phase 6 (chi tiết hơn):**
```markdown
- [ ] Telegram message format template (configurable per channel)
- [ ] EditTelegramMessageJob: trigger T+FT+1h, edit với kết quả thực + accuracy badge
- [ ] Bot command /lichdat: query upcoming matches 48h, format markdown table
- [ ] Bot command /dudoan: query prediction cho trận cụ thể
```

**Thêm section mới "Business Rules" vào TODO.md:**
```markdown
## Business Rules

### Match Status Flow
Scheduled → Live (kickoff) → HT (45min) → Live (46min) → FT | Postponed | Cancelled

### Prediction Confidence Tiers
- ≥ 75: Auto-publish blog + Telegram
- 50-74: Telegram only, blog pending review
- < 50: DB only, no publish

### API Rate Limits
- Football API: 100 req/day — cache aggressively
- Claude API: monitor token usage, Gemini fallback khi >80% budget dùng
- Telegram Bot: 30 messages/sec max

### Live Polling Strategy
- Idle (no live match): tắt polling, dùng scheduled job 6h
- Live match detected: activate 30s SignalR polling
- HT: giảm xuống 60s
- FT + 5min: deactivate polling
```

---

## 4. CẬP NHẬT CLAUDE.md

**Thêm vào section Service Abstractions:**
```
IMatchContextService    — [Phase 4] tổng hợp form, h2h, lineups → MatchContextData
IPromptTemplateService  — [Phase 5] render prompt từ template + MatchContext
IAccuracyTrackingService — [Phase 5] so sánh predicted vs actual, update stats
IBotCommandHandler      — [Phase 6] xử lý /lichdat, /dudoan Telegram commands
```

**Thêm Business Rules section:**
```markdown
## Business Rules (Quick Ref)
- Prediction auto-publish: confidence ≥ 75
- Live polling: 30s khi live, tắt khi idle
- Football API quota: 100/day — cache match data TTL 2h
- Telegram edit: T+FT+1h edit message với kết quả thực
- Blog slug pattern: {home-slug}-vs-{away-slug}-{yyyy-mm-dd}-prediction
```

**Thêm IUnitOfWork Properties mới:**
```csharp
uow.Countries           // ICountryRepository (Phase 4)
uow.Leagues             // ILeagueRepository (Phase 4)
uow.Teams               // ITeamRepository (Phase 4)
uow.PromptTemplates     // IPromptTemplateRepository (Phase 5)
uow.AccuracyStats       // IAccuracyStatsRepository (Phase 5)
```

---

## 5. CLEANUP TASKS

- **Xóa** `.claude/plans/glimmering-tinkering-gadget.md` — tất cả 15 prototype files đã tồn tại, plan đã hoàn thành
- **Xóa** `.claude/plans/football-db-schema-design.md` nếu đã được implement vào Phase 4 tasks trong TODO.md

---

## 6. EXECUTION ORDER

### Bước 1 — Tạo 3 HTML prototypes mới
1. `predictions.html` (P7) — priority cao nhất, showcase core value
2. `team-profile.html` (P8) — UX depth, click flow từ home
3. `admin-job-monitor.html` (A7) — operational visibility

### Bước 2 — Cập nhật TODO.md
- Thêm business rules section
- Chi tiết hóa Phase 4-6 tasks
- Thêm 3 prototype tasks vào Phase 2

### Bước 3 — Cập nhật CLAUDE.md
- Thêm service abstractions mới
- Thêm business rules quick ref
- Thêm IUnitOfWork properties Phase 4-5

### Bước 4 — Cập nhật Bugs.md
- Đổi tên thành `Architecture.md` (nội dung đã vượt phạm vi "bugs")
- Thêm section Prediction Accuracy Tracking design decision
- Thêm section Telegram Edit Strategy

### Bước 5 — Cleanup plans
- Xóa `glimmering-tinkering-gadget.md`
- Xóa `football-db-schema-design.md` (nội dung gộp vào TODO.md Phase 4)

---

## Verification

Sau khi implement:
- Mở `predictions.html` trong browser → kiểm tra prediction cards, confidence meter, filter bar
- Mở `team-profile.html` → click qua 5 tabs, verify H2H dropdown
- Mở `admin-job-monitor.html` → kiểm tra API quota bar, job table
- Đọc TODO.md mới → confirm business rules section có đầy đủ logic
- Đọc CLAUDE.md mới → confirm service abstractions update đồng bộ
- Click flow test: home.html → click đội → team-profile.html → click trận → match-detail.html
