# Deploy Guide

## Môi trường

| Env | URL | Platform |
|-----|-----|---------|
| Local | https://localhost:7007 (API) · https://localhost:7241 (Web) | Docker Compose |
| Staging | Railway auto-deploy từ branch `main` | Railway |
| Production | AWS EC2 + RDS | AWS |

---

## Staging — Railway

Railway tự deploy khi push lên `main`.

**Environment variables cần set trên Railway:**

```
ConnectionStrings__DefaultConnection=postgresql://user:pass@host:5432/db
ConnectionStrings__Redis=redis://host:6379
ASPNETCORE_ENVIRONMENT=Staging
WebBaseUrl=https://<web-railway-url>
ApiBaseUrl=https://<api-railway-url>
```

---

## Production — AWS

| Service | Config | Mục đích |
|---------|--------|---------|
| EC2 t3.small | 2GB RAM | Chạy Web + API containers |
| RDS PostgreSQL t3.micro | 1GB RAM | Database |
| ElastiCache Redis t3.micro | | Cache + SignalR backplane |
| S3 | | Media uploads |
| CloudFront | | CDN static assets |
| ACM | | SSL certificate |

**Bảo mật:**
- KHÔNG commit `appsettings.Production.json`
- Dùng AWS SSM Parameter Store hoặc Secrets Manager cho secrets
- EC2 Instance Profile để access S3 — KHÔNG dùng access key

**Connection string Production:**
```
Host=<rds-endpoint>;Port=5432;Database=footballblog;Username=<user>;Password=<pass>;SSL Mode=Require
```

---

## GitHub Actions CI/CD

Pipeline đề xuất (Phase 7):

```
PR mở     →  build + test (CI)
Push main →  build Docker → push ECR → deploy Railway (staging)
Tag v*.*.*  →  deploy AWS (prod, cần manual approval)
```

---

## Cấu hình API Keys (Phase 4+)

Dùng `dotnet user-secrets` — KHÔNG commit vào git:

```bash
cd FootballBlog.API

# Football API (https://api-football.com)
dotnet user-secrets set "FootballApi:ApiKey" "<key>"

# Claude AI
dotnet user-secrets set "AI:Claude:ApiKey" "<key>"

# Google Gemini
dotnet user-secrets set "AI:Gemini:ApiKey" "<key>"

# Telegram Bot
dotnet user-secrets set "Telegram:BotToken" "<token>"
dotnet user-secrets set "Telegram:PredictionChannelId" "<channel_id>"
```

**appsettings.json structure** (không chứa secret, commit bình thường):

```json
{
  "FootballApi": {
    "BaseUrl": "https://v3.football.api-sports.io",
    "ApiKey": "",
    "DailyRequestLimit": 100
  },
  "AI": {
    "DefaultProvider": "Claude",
    "Claude": { "ApiKey": "", "Model": "claude-opus-4-6", "MaxTokens": 2000 },
    "Gemini": { "ApiKey": "", "Model": "gemini-2.0-flash" }
  },
  "Telegram": {
    "BotToken": "",
    "DefaultChatId": "",
    "PredictionChannelId": ""
  },
  "Prediction": {
    "GenerateHoursBeforeKickoff": 24,
    "AutoPublishPost": true
  }
}
```

---

## Ước tính chi phí / tháng

| Scenario | Chi phí |
|----------|---------|
| Staging (Railway) | ~$10-20 |
| Production nhỏ (AWS minimal) | ~$60-90 |
| Football API free tier | $0 (100 req/ngày) |
| Football API paid (nếu vượt) | ~$10/tháng |
| Claude API (~10 predictions/ngày) | ~$13/tháng |
| Gemini Flash (thay thế Claude) | ~$1/tháng |
