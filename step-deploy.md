# Deploy Steps — Fly.io (PowerShell)

## Step 1 — Cài Fly CLI
```powershell
iwr https://fly.io/install.ps1 -useb | iex
```

## Step 2 — Đăng nhập
```powershell
fly auth login
```

## Step 3 — Tạo app API
```powershell
fly launch --name footballblog-api --dockerfile Dockerfile.api --region sin --no-deploy
```

## Step 4 — Set secrets cho API ✅
```powershell
fly secrets set --app footballblog-api `
  "ConnectionStrings__DefaultConnection=Host=ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_e0CpEJA4RdOW;SSL Mode=Require;Trust Server Certificate=true" `
  "ConnectionStrings__Redis=redis://default:gQAAAAAAAX8OAAIgcDFjOWFlNjhkOTRhZjU0MDI2YjJkYWI5NjdlMzhmNzBmYg@generous-grackle-98062.upstash.io:6379" `
  "Jwt__Key=OaxI14vz2xgpnVNFV+KHxRLrHFkLZwnmNN/TWje2xG0=" `
  "WebBaseUrl=https://footballblog-web.fly.dev" `
  "ASPNETCORE_ENVIRONMENT=Staging"
```

## Step 5 — Deploy API ⏳ (đang chạy)
```powershell
fly deploy --app footballblog-api --dockerfile Dockerfile.api
```
> Spinning bình thường — đợi 2-5 phút đến khi thấy "success" hoặc lỗi

## Step 6 — Chạy EF Migration (1 lần duy nhất)
```powershell
$env:ConnectionStrings__DefaultConnection = "Host=ep-shy-voice-aqrdqaz6-pooler.c-8.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_e0CpEJA4RdOW;SSL Mode=Require;Trust Server Certificate=true"
$env:Jwt__Key = "OaxI14vz2xgpnVNFV+KHxRLrHFkLZwnmNN/TWje2xG0="

dotnet ef database update `
  --project FootballBlog.Infrastructure `
  --startup-project FootballBlog.API
```

## Step 7 — Tạo + Deploy Web

### 7a — Tạo app Web
```powershell
fly launch --name footballblog-web --dockerfile Dockerfile.web --region sin --no-deploy
```

### 7b — Set secrets Web
```powershell
fly secrets set --app footballblog-web `
  "ApiBaseUrl=https://footballblog-api.fly.dev" `
  "ASPNETCORE_ENVIRONMENT=Staging"
```

### 7c — Deploy Web
```powershell
fly deploy --app footballblog-web --dockerfile Dockerfile.web
```

## Step 8 — Setup CI/CD (GitHub Actions)

### 8a — Tạo Fly token
```powershell
fly tokens create deploy
```
> Copy token hiện ra

### 8b — Thêm vào GitHub
```
GitHub repo → Settings → Secrets and variables → Actions → New repository secret
Name:  FLY_API_TOKEN
Value: FlyV1 fm2_lJPECAAAAAAAE/X1xBCDXtFESG345t0W8WMpZ5VpwrVodHRwczovL2FwaS5mbHkuaW8vdjGWAJLOABjxsx8Lk7lodHRwczovL2FwaS5mbHkuaW8vYWFhL3YxxDzTDoozR50lR5ew+IaxkT2j3sqTpHgZUQD3Autfcc2fd28/Tjkghh6ip7IIpP0uJhewhBPYEv5kVwqwATPETn1ijtlF7bhMuWMlMAuKyCdU8zYGUm/wNvi+qRjfbahMH1XzX0vg8krkpJPQQsKEfORIqF7iFpuxWTVl5WOZgkoY81L1e3dixSmSKV1EVw2SlAORgc4BEXE+HwWRgqdidWlsZGVyH6J3Zx8BxCAU7FKpeuoCdDbPZzmYneJH3A7OJTxIikVZIyGAyqF/LA==,fm2_lJPETn1ijtlF7bhMuWMlMAuKyCdU8zYGUm/wNvi+qRjfbahMH1XzX0vg8krkpJPQQsKEfORIqF7iFpuxWTVl5WOZgkoY81L1e3dixSmSKV1EV8QQEUtEBrEfBCMOJXke5W5THsO5aHR0cHM6Ly9hcGkuZmx5LmlvL2FhYS92MZgEks5p+fy3zo+SAtUXzgAX5mgKkc4AF+ZoDMQQmy9dPDNbZXV5R7mdxl+468QgC+QNaLKiwPdbpcw0slB94BsJ1ZdP1+ArnBPOWCejVBs=
```

### 8c — Test CI/CD
```powershell
git add .
git commit -m "add deploy config"
git push origin master
```
> Vào GitHub → Actions tab → xem pipeline chạy tự động

---

## Sau khi xong hoàn toàn

| URL | Service |
|-----|---------|
| https://footballblog-api.fly.dev/swagger | API Swagger |
| https://footballblog-web.fly.dev | Web public |

## Xem log khi có lỗi
```powershell
fly logs --app footballblog-api
fly logs --app footballblog-web
```
