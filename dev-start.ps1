# dev-start.ps1 - Khởi động toàn bộ môi trường dev FootballBlog
# Chạy từ terminal hiện tại trong Windows Terminal:
#   .\dev-start.ps1
# Script dùng `wt split-pane` để chia panes ngay trong cửa sổ đang chạy.

$root = $PSScriptRoot

# 0. Kill các process cũ đang lock port/file
Write-Host "[0/4] Killing old processes..." -ForegroundColor Gray
Get-Process -Name "FootballBlog.Web","FootballBlog.API","dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

# 1. Docker - chờ xong mới tiếp
Write-Host "[1/4] Docker compose up..." -ForegroundColor Cyan
docker compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker failed. Dung lai." -ForegroundColor Red
    exit 1
}

# 2-4. Split panes trong cùng cửa sổ Windows Terminal hiện tại
# --window 0  = cửa sổ WT đang focus
# split-pane  = chia pane (tương đương Ctrl+D)
# -d          = working directory

# npm install neu chua co node_modules
if (-not (Test-Path "$root\FootballBlog.Web\node_modules")) {
    Write-Host "[npm] Installing dependencies..." -ForegroundColor Cyan
    Push-Location "$root\FootballBlog.Web"
    npm install
    Pop-Location
}

Write-Host "[2/4] Tailwind CSS watcher (split pane)..." -ForegroundColor Cyan
wt --window 0 split-pane -d "$root\FootballBlog.Web" -- powershell -NoExit -Command "npm run watch:css"

Write-Host "[3/4] FootballBlog.API (split pane)..." -ForegroundColor Cyan
wt --window 0 split-pane -d "$root" -- powershell -NoExit -Command "dotnet run --project FootballBlog.API --launch-profile https"

Write-Host "[4/4] FootballBlog.Web (split pane)..." -ForegroundColor Cyan
wt --window 0 split-pane -d "$root" -- powershell -NoExit -Command "dotnet run --project FootballBlog.Web --launch-profile https"

Write-Host ""
Write-Host "Done! 3 panes opened:" -ForegroundColor Green
Write-Host "  Pane 1 - Tailwind watch:css" -ForegroundColor Yellow
Write-Host "  Pane 2 - API  : https://localhost:7007/swagger" -ForegroundColor Yellow
Write-Host "  Pane 3 - Web  : https://localhost:7241" -ForegroundColor Yellow
