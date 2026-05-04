# Deploy Guide

## Môi trường

| Env | Platform | Chi phí | Mục đích |
|-----|----------|---------|---------|
| Local | Docker Compose | $0 | Dev |
| Staging | Fly.io + Neon + Upstash | **$0** | Test môi trường, demo |
| Production | AWS hoặc VPS | $27-90/tháng | User thật |

---

## Staging — Fly.io (Miễn phí)

### Stack miễn phí
| Service | Platform | Free tier |
|---------|----------|-----------|
| API + Web | Fly.io | 3 VMs, 256MB RAM/VM |
| PostgreSQL | Neon | 0.5GB, không pause |
| Redis | Upstash | 10,000 req/ngày |

---

### Bước 1 — Chuẩn bị tài khoản

```bash
# Cài Fly CLI
curl -L https://fly.io/install.sh | sh   # Mac/Linux
# Windows: https://fly.io/install.ps1

# Đăng ký / đăng nhập (cần thẻ tín dụng nhưng không bị charge nếu trong free tier)
fly auth signup
fly auth login
```

- **Neon**: đăng ký tại https://neon.tech → tạo project → copy connection string
- **Upstash**: đăng ký tại https://upstash.com → tạo Redis database → copy URL (chỉ lấy phần `redis://...`, bỏ `redis-cli --tls -u`)

---

### Bước 2 — Tạo Dockerfile

**`Dockerfile.api`** (đặt ở root):
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish FootballBlog.API/FootballBlog.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FootballBlog.API.dll"]
```

**`Dockerfile.web`** (đặt ở root):
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish FootballBlog.Web/FootballBlog.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FootballBlog.Web.dll"]
```

---

### Bước 3 — Deploy lên Fly.io

```bash
# Deploy API
fly launch --name footballblog-api --dockerfile Dockerfile.api --region sin --no-deploy
fly secrets set --app footballblog-api \
  ConnectionStrings__DefaultConnection="<neon-connection-string>" \
  ConnectionStrings__Redis="<upstash-redis-url>" \
  Jwt__Key="<random-32-char-string>" \
  WebBaseUrl="https://footballblog-web.fly.dev" \
  ASPNETCORE_ENVIRONMENT="Staging"
fly deploy --app footballblog-api --dockerfile Dockerfile.api

# Deploy Web
fly launch --name footballblog-web --dockerfile Dockerfile.web --region sin --no-deploy
fly secrets set --app footballblog-web \
  ApiBaseUrl="https://footballblog-api.fly.dev" \
  ASPNETCORE_ENVIRONMENT="Staging"
fly deploy --app footballblog-web --dockerfile Dockerfile.web
```

> `--region sin` = Singapore (gần Việt Nam nhất trong free tier)

---

### Bước 4 — Chạy EF Migration lần đầu

```bash
# Chạy migration từ máy local trỏ vào Neon DB
dotnet ef database update \
  --project FootballBlog.Infrastructure \
  --startup-project FootballBlog.API \
  --connection "<neon-connection-string>"
```

---

### Bước 5 — Kiểm tra

```bash
fly logs --app footballblog-api   # xem log realtime
fly status --app footballblog-api # xem trạng thái
fly open --app footballblog-web   # mở browser
```

---

## CI/CD — GitHub Actions

Khi push lên `master` → tự động build + deploy lên Fly.io.

**`.github/workflows/deploy.yml`**:
```yaml
name: Deploy to Fly.io

on:
  push:
    branches: [master]

jobs:
  deploy-api:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: superfly/flyctl-actions/setup-flyctl@master
      - run: fly deploy --app footballblog-api --dockerfile Dockerfile.api --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}

  deploy-web:
    runs-on: ubuntu-latest
    needs: deploy-api
    steps:
      - uses: actions/checkout@v4
      - uses: superfly/flyctl-actions/setup-flyctl@master
      - run: fly deploy --app footballblog-web --dockerfile Dockerfile.web --remote-only
        env:
          FLY_API_TOKEN: ${{ secrets.FLY_API_TOKEN }}
```

**Setup GitHub Secrets:**
```bash
fly tokens create deploy   # copy token này
# Vào GitHub repo → Settings → Secrets → New secret
# Name: FLY_API_TOKEN
# Value: <token vừa copy>
```

Flow sau khi setup:
```
git push origin master
        ↓
GitHub Actions chạy tự động (~3-5 phút build)
        ↓
Fly.io deploy version mới (zero-downtime rolling deploy)
        ↓
App live tại https://footballblog-web.fly.dev
```

---

## Nâng lên Production — Lộ trình

### Giai đoạn 1: Staging (hiện tại) — $0
```
Fly.io + Neon + Upstash
→ Test môi trường, không có user thật
→ Giới hạn: 256MB RAM, 10k Redis req/ngày
```

### Giai đoạn 2: Production nhỏ — ~$27/tháng
```
VPS Vultr/DigitalOcean 2GB RAM ($12/tháng)
  + Managed PostgreSQL ($15/tháng)
  + Upstash Redis free tier vẫn dùng được

Lý do chọn VPS thay AWS: rẻ hơn 2-3x, control đủ dùng
Docker Compose chạy y hệt local — migrate dễ
```

### Giai đoạn 3: AWS Production — ~$60-90/tháng
```
EC2 t3.small   ~$15/tháng  — chạy containers
RDS t3.micro   ~$25/tháng  — PostgreSQL managed
ElastiCache    ~$15/tháng  — Redis
S3 + CloudFront ~$5/tháng  — media + CDN
```

**AWS Free Tier** (12 tháng đầu nếu tài khoản mới):
| Service | Free tier |
|---------|-----------|
| EC2 t2.micro | 750 giờ/tháng (~1 VM 24/7) |
| RDS t2.micro | 750 giờ/tháng + 20GB storage |
| S3 | 5GB storage |
| CloudFront | 1TB data transfer |

> ⚠️ t2.micro chỉ có 1GB RAM — .NET chạy được nhưng sát giới hạn. Dùng để test AWS, không dùng production thật.

---

## Migrate Fly.io → AWS (khi cần)

Không cần sửa code — chỉ thay environment variables:

```bash
# Fly.io
ConnectionStrings__DefaultConnection = neon-url

# AWS (thay bằng)
ConnectionStrings__DefaultConnection = "Host=<rds-endpoint>;Port=5432;Database=footballblog;..."
ConnectionStrings__Redis             = "<elasticache-endpoint>:6379"
```

Dockerfile giữ nguyên, build trên EC2 hoặc dùng ECR + ECS.

---

## API Keys (dotnet user-secrets — không commit)

```bash
cd FootballBlog.API

dotnet user-secrets set "FootballApi:ApiKey" "<key>"
dotnet user-secrets set "Jwt:Key" "<random-32-char>"
dotnet user-secrets set "AI:Claude:ApiKey" "<key>"
dotnet user-secrets set "AI:Gemini:ApiKey" "<key>"
dotnet user-secrets set "Telegram:BotToken" "<token>"
dotnet user-secrets set "Telegram:ChannelId" "<channel_id>"
```

Trên server → dùng `fly secrets set` (Fly.io) hoặc AWS SSM Parameter Store (AWS).

---

## Ước tính chi phí tổng

| Giai đoạn | Chi phí | Khi nào chuyển |
|-----------|---------|----------------|
| Fly.io free | $0 | Ngay bây giờ |
| VPS $27/tháng | $27 | Khi có user thật |
| AWS ~$70/tháng | $60-90 | Khi cần scale / enterprise |
| Football API free | $0 | 100 req/ngày |
| Claude API | ~$13/tháng | ~10 predictions/ngày |
| Gemini Flash (thay Claude) | ~$1/tháng | Tiết kiệm hơn |
