# dev-start.ps1 - Khởi động toàn bộ môi trường dev FootballBlog
# Chạy từ terminal hiện tại trong Windows Terminal:
#   .\dev-start.ps1           — khởi động bình thường
#   .\dev-start.ps1 -logs     — xóa toàn bộ folder logs/ trước khi chạy
# Script dùng `wt new-tab` để tạo tabs mới trong cửa sổ đang chạy.

param(
    [switch]$logs
)

$root = $PSScriptRoot

# 0a. Xóa logs nếu có flag -logs
if ($logs) {
    Write-Host "[logs] Clearing logs folder..." -ForegroundColor Magenta
    if (Test-Path "$root\logs") {
        Remove-Item "$root\logs" -Recurse -Force
        Write-Host "[logs] Done." -ForegroundColor Magenta
    } else {
        Write-Host "[logs] Folder not found, skipping." -ForegroundColor Gray
    }
}

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

# npm install neu chua co node_modules
if (-not (Test-Path "$root\FootballBlog.Web\node_modules")) {
    Write-Host "[npm] Installing dependencies..." -ForegroundColor Cyan
    Push-Location "$root\FootballBlog.Web"
    npm install
    Pop-Location
}

Write-Host "[2/4] Tailwind CSS watcher (new tab)..." -ForegroundColor Cyan
wt --window 0 split-pane --title "Tailwind" -d "$root\FootballBlog.Web" -- powershell -NoExit -Command "npm run watch:css"

Write-Host "[3/4] FootballBlog.API (new tab)..." -ForegroundColor Cyan
wt --window 0 new-tab --title "API" -d "$root" -- powershell -NoExit -Command "dotnet run --project FootballBlog.API --launch-profile https"

Write-Host "[4/4] FootballBlog.Web (new tab)..." -ForegroundColor Cyan
wt --window 0 new-tab --title "Web" -d "$root" -- powershell -NoExit -Command "dotnet run --project FootballBlog.Web --launch-profile https"

Write-Host ""
Write-Host "Done! 3 tabs opened:" -ForegroundColor Green
Write-Host "  Tab 'Tailwind' - npm watch:css" -ForegroundColor Yellow
Write-Host "  Tab 'API'      - https://localhost:7007/swagger" -ForegroundColor Yellow
Write-Host "  Tab 'Web'      - https://localhost:7241" -ForegroundColor Yellow
