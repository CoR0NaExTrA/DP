Write-Host "Запуск системы..." -ForegroundColor Green

# RankCalculator (2 экземпляра)
$rankProject = "..\RankCalculator\RankCalculator.csproj"

Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$rankProject`" --urls http://0.0.0.0:5001"

# EventsLogger (2 экземпляра)
$loggerProject = "..\EventsLogger\EventsLogger.csproj"

Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$loggerProject`" --urls http://0.0.0.0:5002"
Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$loggerProject`" --urls http://0.0.0.0:5003"

Write-Host "Ожидание запуска..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Система запущена!" -ForegroundColor Green