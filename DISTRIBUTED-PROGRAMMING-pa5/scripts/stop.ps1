Write-Host "Остановка компонентов..." -ForegroundColor Yellow

Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Все компоненты остановлены!" -ForegroundColor Green