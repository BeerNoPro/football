#!/usr/bin/env pwsh
<#
.DESCRIPTION
Convert all CRLF line endings to LF in the repository
#>

param(
    [string]$Path = (Get-Location),
    [switch]$Commit
)

Write-Host "Converting CRLF to LF in: $Path" -ForegroundColor Cyan

$extensions = @('*.cs', '*.razor', '*.html', '*.css', '*.js', '*.ts', '*.json', '*.xml', '*.md', '*.yml', '*.yaml', '*.ps1', '*.csproj', '*.sln')
$count = 0
$files = @()

foreach ($ext in $extensions) {
    $files += Get-ChildItem -Path $Path -Filter $ext -Recurse -ErrorAction SilentlyContinue
}

$files = $files | Where-Object { $_.FullName -notmatch '\\(bin|obj|node_modules|\.git)\\' }

Write-Host "Found $($files.Count) files to process" -ForegroundColor Yellow

foreach ($file in $files) {
    try {
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
        $text = [System.Text.Encoding]::UTF8.GetString($bytes)

        if ($text.Contains("`r`n")) {
            $newText = $text -replace "`r`n", "`n"
            [System.IO.File]::WriteAllText($file.FullName, $newText, [System.Text.Encoding]::UTF8)
            $count++
            Write-Host "OK: $($file.Name)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "ERR: $($file.Name) - $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Converted $count files" -ForegroundColor Cyan

if ($Commit) {
    Write-Host "Running: git add --renormalize -A" -ForegroundColor Yellow
    git add --renormalize -A
    Write-Host "Files staged" -ForegroundColor Green
}
else {
    Write-Host "Use -Commit to stage changes" -ForegroundColor Yellow
}
