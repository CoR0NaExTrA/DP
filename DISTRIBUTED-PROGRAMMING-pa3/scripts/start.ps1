# start.ps1
Write-Host "Запуск веб-приложений..." -ForegroundColor Green

# Запуск экземпляров приложения
$projectPath = "..\RankCalculator\RankCalculator.csproj" 

Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$projectPath`" --urls http://0.0.0.0:5001"
Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$projectPath`" --urls http://0.0.0.0:5002"

Write-Host "Ожидание запуска приложений..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Система запущена!"