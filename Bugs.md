# Bugs & Known Issues

---

## [BUG-001] Football API trả lỗi "Free plan does not have access to this season"

**Status**: Đã có workaround (SeasonOverride) — cần upgrade plan cho production

### Triệu chứng

```
WRN Football API returned errors on teams?league=39&season=2025:
{"plan":"Free plans do not have access to this season, try from 2022 to 2024."}
```

Job `SeedLeagueDataJob` chạy 15 league nhưng 0 data nào được insert.

### Nguyên nhân gốc

`CurrentSeason()` tính mùa giải hiện tại theo logic:
- Tháng >= 7 → dùng năm hiện tại
- Tháng < 7  → dùng năm hiện tại - 1

Ngày 2026-04-21 → season = **2025**.

**Free plan API-Football chỉ hỗ trợ season 2022–2024** (xem FLOW_API_FOOTBALLL.md §1 + §Summary).
Season 2025 yêu cầu gói trả phí (Starter trở lên).

### Fix tạm thời (Dev)

Thêm `SeasonOverride: 2024` vào `appsettings.Development.json`:

```json
"FootballApi": {
  "SeasonOverride": 2024
}
```

`SeedLeagueDataJob` và `FetchUpcomingMatchesJob` đọc `opts.SeasonOverride` — nếu set thì dùng, không thì tự tính.

> ⚠️ Season 2024 = mùa 2024/25 đã kết thúc → dữ liệu fixtures sẽ là kết quả lịch sử, không có trận upcoming. Đủ để test UI + standings, không đủ để test live score.

### Fix production

Upgrade API key lên **Starter plan** trở lên → remove `SeasonOverride` khỏi appsettings production.

Xem thêm: [dashboard.api-football.com](https://dashboard.api-football.com) — mục Subscription.

### Config cần kiểm tra khi switch season

| Config | Dev (Free plan) | Prod (Paid plan) |
|--------|-----------------|------------------|
| `FootballApi.SeasonOverride` | `2024` | _(bỏ trống — tự tính)_ |
| `FootballApi.DailyRequestLimit` | `100` | Theo plan (500/1000/...) |
| `FootballApi.FixturesPerLeague` | `20` | Tăng lên nếu cần |

### Ghi chú thêm

- Mùa 2024 có đủ: Teams, Standings, Fixtures (đã kết thúc), H2H
- Mùa 2024 thiếu: Fixtures upcoming, Live score thực tế
- `GetTeamsByLeagueAsync` / `GetFixturesByRangeAsync` / `GetStandingsAsync` hiện trả `null` khi API trả `errors` → job sẽ abort ngay league đầu tiên bị lỗi (không spam tiếp)

---
