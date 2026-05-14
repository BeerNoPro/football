# TODO — Football Blog

> Cập nhật: 2026-05-11. Phân loại theo mức độ ưu tiên.

---

## CRITICAL — Fix trước khi production

### C0. Hangfire startup block port bind → Fly proxy timeout (502 cold start)
**File:** `FootballBlog.API/Program.cs:291–400`

**Vấn đề:** Toàn bộ Hangfire schema install + seeding xảy ra **trước** `app.Run()` → app bind port 8080 sau ~19 giây. Fly.io proxy chỉ chờ 8 giây → timeout → user nhận 502 trong mỗi cold start.

**Thứ tự hiện tại (sai):**
```
builder.Build()           → Hangfire kết nối DB, tạo schema "hangfire" (~8-10s)
UseHangfireDashboard()    → khởi tạo JobStorage.Current
RecurringJob.AddOrUpdate() → ghi vào Hangfire DB
Seeding (lines 365-397)   → DB queries API keys + admin user
app.Run()                 → MỚI bind port 8080  ← quá muộn
```

**Fix:** Dùng `IHostApplicationLifetime.ApplicationStarted` để defer Hangfire server init sau khi app đã bind port:
```csharp
// Sau app.Run() vẫn nhận traffic, Hangfire init trong background
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() => {
    // RecurringJob.AddOrUpdate(...) calls ở đây
    // Seeding ở đây
});
app.Run(); // bind port ngay, không chờ Hangfire
```

**Kết quả:** Cold start giảm từ ~19s xuống ~3-4s, proxy không timeout nữa.

---

### C1. Hangfire worker count quá thấp
**File:** `FootballBlog.API/Program.cs:226`
**Vấn đề:** Chỉ có 2 worker threads cho toàn bộ 9 Hangfire jobs. Peak match day: LiveScorePollingJob (mỗi phút) + GeneratePredictionJob + TelegramNotificationJob chen nhau 2 slot → job queue backlog, prediction miss deadline 06:00 VN.

**Fix:**
```csharp
builder.Services.AddHangfireServer(opt =>
{
    opt.WorkerCount = Math.Max(4, Environment.ProcessorCount - 1);
});
```

---

### C3. Polly retry block worker thread khi API timeout
**File:** `FootballBlog.API/Program.cs:208-210`

**Vấn đề:** `HandleTransientHttpError()` bao gồm timeout. 1 request timeout 30s → 3 retry với backoff 2s/4s/8s = **192 giây bị block**. LiveScorePollingJob chạy mỗi phút, nếu API timeout → worker starvation, các job khác không chạy được.

**Fix:**
```csharp
// Thêm timeout policy + giảm còn 2 retries
var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
var retry = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(2, attempt => TimeSpan.FromSeconds(attempt * 2));

builder.Services.AddHttpClient<IFootballApiClient, FootballApiClient>(...)
    .AddPolicyHandler(timeout.WrapAsync(retry));
```

---

### C4. SignalR broadcast full DTO + all events mỗi phút
**File:** `FootballBlog.API/Jobs/LiveScorePollingJob.cs:112-129`

**Vấn đề:** Mỗi phút, mỗi live match broadcast toàn bộ `LiveMatchDto` bao gồm tất cả events (5-20 items). 20 live matches peak weekend = ~50KB payload × 60 lần/giờ × số concurrent user → bandwidth explosion.

**Fix:** Chỉ broadcast diff — score/status thay đổi + events MỚI kể từ lần broadcast trước:
```csharp
// Lưu lastBroadcastAt per match vào Redis
// Chỉ gửi events.Where(e => e.CreatedAt > lastBroadcast)
// Nếu score + status không thay đổi → skip broadcast
```

---

### C6. Default admin credentials trong appsettings.json
**File:** `FootballBlog.API/appsettings.json` → `DefaultAdmin.Password = "Admin123"`

**Vấn đề:** Nếu commit lên GitHub public → credential lộ.

**Fix:**
1. Chuyển sang `dotnet user-secrets` cho local dev
2. Dùng AWS Parameter Store / Fly.io secrets cho production
3. Enforce đổi password sau lần login đầu

---

### C7. Media upload vào wwwroot/uploads/ — mất khi container restart
**File:** `FootballBlog.API/Controllers/MediaController.cs`

**Vấn đề:** Fly.io dùng ephemeral storage — mọi file upload mất khi deploy hoặc restart.

**Fix:** Tích hợp Cloudflare R2 (S3-compatible, free 10GB):
```csharp
// Thêm AWSSDK.S3 hoặc dùng HttpClient trực tiếp với R2 endpoint
// Upload stream → trả về public URL → lưu URL vào DB thay vì local path
```

---

## HIGH — Fix trong sprint tới

### H1. Redis rate limiter có race condition
**File:** `FootballBlog.Infrastructure/Services/RedisFootballApiRateLimiter.cs:25-76`

**Vấn đề:** INCR → check → DECR là 3 Redis operation riêng biệt, không atomic. 2 parallel job call đồng thời có thể vượt quota 100 req/day một vài request.

**Fix:** Thay bằng Lua script atomic:
```lua
local count = redis.call('incr', KEYS[1])
if count == 1 then redis.call('expire', KEYS[1], ARGV[2]) end
if count > tonumber(ARGV[1]) then
    redis.call('decr', KEYS[1])
    return 0
end
return 1
```

---

### H2. FetchPostMatchDataJob reload entity đã có + commit trong loop
**File:** `FootballBlog.API/Jobs/FetchPostMatchDataJob.cs:36-64`

**Vấn đề:**
1. Fetch `pending` matches từ `GetFinishedWithoutStatsAsync()` xong lại gọi `GetByExternalIdAsync()` từng match → double query
2. `CommitAsync()` trong foreach loop

**Fix:**
```csharp
foreach (var match in targets)
{
    var (statsJson, eventsJson) = await apiClient.GetFixturePostMatchDataAsync(match.ExternalId);
    if (statsJson is null) break; // quota hit

    match.StatsJson = statsJson;
    match.EventsJson = eventsJson;
    await uow.Matches.UpdateAsync(match); // dùng object đã có
}
await uow.CommitAsync(); // 1 lần duy nhất
```

---

### H3. N+1 subquery trong GetWithoutPredictionAsync
**File:** `FootballBlog.Infrastructure/Repositories/MatchRepository.cs:27-34`

**Vấn đề:** `.Where(m => !m.Predictions.Any(p => p.Phase == PreMatch))` sinh subquery per row. Kết hợp 3 Include (HomeTeam, AwayTeam, League) → Cartesian product.

**Fix:** Include Predictions rồi filter in-memory, hoặc dùng LEFT JOIN explicit:
```csharp
.Include(m => m.Predictions)
// .Where() bỏ phần Any() — filter sau ToListAsync()
var results = await query.ToListAsync();
return results.Where(m => !m.Predictions.Any(p => p.Phase == PredictionPhase.PreMatch));
```

---

## MEDIUM — Cần xử lý trước 1K DAU

### M1. Thêm output cache cho /api/categories và /api/tags
Categories và Tags thay đổi rất ít nhưng được gọi mỗi page request.

**Fix:**
```csharp
[HttpGet, OutputCache(PolicyName = "StaticData")] // 30 phút
public async Task<IActionResult> GetAll() { ... }

// Program.cs:
options.AddPolicy("StaticData", p => p.Expire(TimeSpan.FromMinutes(30)).Tag("static"));
```

---

### M2. Admin pages thiếu Error Boundaries
**File:** Tất cả `FootballBlog.Web/Components/Pages/Admin/*/Index.razor`

**Vấn đề:** `OnInitializedAsync()` không có try/catch → network fail → blank page, user không biết lý do.

**Fix:** Wrap mỗi admin page:
```razor
@if (_error is not null)
{
    <MudAlert Severity="Severity.Error">@_error</MudAlert>
}

protected override async Task OnInitializedAsync()
{
    try { await LoadData(); }
    catch (Exception ex) { _error = ex.Message; }
}
```

---

### M3. Không có pagination max cap
Controllers nhận `pageSize` từ query string mà không giới hạn.

**Fix:** Clamp trong controller hoặc service layer:
```csharp
pageSize = Math.Clamp(pageSize, 1, 100);
```

---

### M4. Quill HTML không sanitize — XSS risk
**File:** `FootballBlog.API/Controllers/PostsController.cs` (CreatePost, UpdatePost)

**Fix:** Thêm `HtmlSanitizer` NuGet package, sanitize `Content` trước khi lưu:
```csharp
var sanitizer = new HtmlSanitizer();
dto.Content = sanitizer.Sanitize(dto.Content);
```

---

### M5. ApiKeyRotator cache 5 phút không sync khi horizontal scale
**File:** `FootballBlog.Infrastructure/Services/ApiKeyRotator.cs:19`

**Vấn đề:** Khi scale lên 2+ API pod, add key mới mất 5 phút để propagate.

**Fix:** Sau khi add/update key qua `ApiKeysController`, gọi `InvalidateCacheAsync()` để clear Redis cache ngay lập tức.

---

## Scalability — Phân Tích Theo Quy Mô Traffic

### 1K DAU (Đang ở đây / sắp tới)

**Vấn đề sẽ gặp:**
- Job queue backlog do worker count thấp (C1)
- DB round-trips cao khi FetchUpcomingMatchesJob chạy (C2)
- Không có cache cho categories/tags (M1)

**Giải pháp — không cần đổi infra:**
- Fix C1, C2, C3, M1 là đủ
- Fly.io 1GB/1 CPU hiện tại vẫn ổn
- PostgreSQL free tier ổn (< 5K rows/table)

**Estimated cost:** $0 thêm (Fly.io free tier)

---

### 10K DAU

**Vấn đề sẽ gặp:**
- SignalR broadcast full DTO saturate băng thông (C4) — 10K user × 20 matches × 50KB/phút = 10GB/giờ outbound
- Missing DB index (C5) bắt đầu slow query khi Match table > 50K rows
- N+1 query trong GetWithoutPredictionAsync (H3) tạo DB load cao mỗi giờ
- Hangfire job delay do worker starvation (C1) ảnh hưởng UX

**Giải pháp:**
- Fix tất cả CRITICAL + HIGH issues
- Upgrade Fly.io: 2GB RAM, 2 CPU (khoảng $20-30/tháng)
- Bật PostgreSQL connection pooling (PgBouncer) — Fly.io có built-in
- Tách Hangfire sang dedicated machine (Fly.io machine riêng) để job không compete với HTTP requests
- Thêm Redis cache layer cho fixture data (cache 5 phút, invalidate khi job fetch xong)

**Estimated cost:** ~$40-60/tháng

---

### 100K DAU

**Vấn đề sẽ gặp:**
- Single API instance không đủ — cần horizontal scale, nhưng hiện tại ApiKeyRotator in-memory cache không sync (M5)
- PostgreSQL single node đạt giới hạn write throughput khi LiveScorePollingJob upsert mỗi phút
- SignalR với Redis backplane sẽ tạo Redis memory pressure khi 100K connection
- Media storage cần CDN (C7) — latency cao nếu serve từ Fly.io Singapore cho user quốc tế
- Hangfire PostgreSQL storage trở thành bottleneck (schema `hangfire` bị lock nhiều)

**Giải pháp:**
- **Scale out API:** 2-3 Fly.io instances (horizontal), Redis backplane đã sẵn sàng ✓
- **PostgreSQL read replica:** Tách read query (GetUpcomingAsync, GetAllAsync) sang replica. EF Core: `UseQuerySplittingBehavior` + read-only DbContext
- **Migrate Hangfire storage:** Chuyển sang Redis storage (Hangfire.Redis.StackExchange) để tránh lock PostgreSQL
- **CDN:** Cloudflare free tier cho static assets + media. Set `Cache-Control: max-age=31536000` cho images
- **LiveScore optimization:** Thay vì polling mỗi phút, dùng Server-Sent Events hoặc webhook từ api-football (nếu có)
- **Rate limit public API:** Thêm 100 req/phút/IP cho tất cả public endpoints (hiện chỉ có login)
- **Fix M5:** ApiKeyRotator phải invalidate Redis cache ngay sau khi update key

**Estimated cost:** ~$150-300/tháng (2-3 API instances + managed PostgreSQL + Redis upsize)

---

### 1M DAU

**Vấn đề sẽ gặp:**
- Kiến trúc monolith (API duy nhất) không đủ — Hangfire jobs cạnh tranh CPU/memory với HTTP requests
- PostgreSQL dù có replica vẫn bottleneck ở write (live score upsert)
- SignalR connection limit per server (~50K connections/instance) → cần nhiều instance hơnb
- api-football free tier 100 req/day hoàn toàn không đủ (cần paid plan hoặc multiple accounts)
- Single Telegram bot có rate limit 30 messages/giây → không đủ nếu nhiều giải đấu đồng thời

**Giải pháp — đây là lúc cần re-architect:**

**1. Tách Jobs service ra khỏi API:**
```
API Service (HTTP + SignalR)     ← Scale theo request
Jobs Service (Hangfire workers)  ← Scale theo job load
```
Dùng message queue (Redis Streams hoặc RabbitMQ) để 2 service communicate.

**2. CQRS cho Match data:**
- Write side: Jobs service → PostgreSQL primary
- Read side: API → PostgreSQL replica (hoặc Redis cache)
- Fixture data: Cache vào Redis với TTL 5 phút, invalidate khi job fetch xong

**3. LiveScore — đổi sang event-driven:**
- Thay LiveScorePollingJob minutely → webhook/SSE từ data provider
- Hoặc dùng dedicated live score service (ScoreBat API, LiveScore API có webhook)
- Push update qua Redis Pub/Sub → SignalR hubs consume

**4. Database sharding / time-series:**
- Match historical data (Finished + StatsJson) → move sang cold storage (S3 Parquet)
- Hot data (Scheduled + Live, 30 ngày gần) → PostgreSQL
- MatchContextData JSONB → xem xét chuyển sang MongoDB nếu query phức tạp

**5. Edge caching:**
- `/api/fixtures` → Cloudflare Cache Rules (5 phút TTL, vary by date+league)
- `/api/posts` → Cloudflare Cache (5 phút, invalidate khi publish)
- Blazor SSR pages → Edge-side rendering hoặc static pre-render

**6. AI prediction queue:**
- Tách GeneratePredictionJob thành dedicated worker pool
- Queue prediction requests → worker consume theo priority (premium leagues trước)
- Gemini/Claude rate limit quản lý ở queue level, không phải job level

**Estimated cost:** $1,000-3,000/tháng (managed services, multiple instances, CDN)
**Team size needed:** 2-3 backend engineers để maintain infrastructure

---

## Low Priority / Nice-to-have

- [ ] JWT sliding window (hiện tại 7 ngày absolute)
- [ ] FullTime prediction phase (post-match AI summary)
- [ ] Prediction accuracy breakdown theo Win/Draw/Loss (hiện tại chỉ đúng/sai tổng)
- [ ] Pagination max cap enforcement (pageSize clamp to 100)
- [ ] `/api/categories` và `/api/tags` output cache
- [ ] FluentValidation cho tất cả request DTOs
- [ ] Per-league season start date config (hiện hardcode tháng 7)
