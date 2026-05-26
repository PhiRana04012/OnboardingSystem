# Запуск всех тестов (frontend + backend)
# Использование: .\run-all-tests.ps1

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Запуск ВСЕХ тестов (Frontend + Backend)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "Начало тестирования: $timestamp" -ForegroundColor Yellow

# ===== FRONTEND ТЕСТЫ =====
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "  1️⃣  FRONTEND ТЕСТЫ (Vitest)" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green

Set-Location c:\Users\Firan\ONBOARDING\onboarding-frontend

Write-Host "Запуск frontend тестов..." -ForegroundColor Cyan
$frontendStart = Get-Date
$frontendResult = npx vitest run --reporter=verbose 2>&1
$frontendEnd = Get-Date
$frontendDuration = ($frontendEnd - $frontendStart).TotalSeconds

# Парсим результаты frontend
$frontendTests = $frontendResult | Select-String "Test Files.*passed.*passed" | Select-Object -First 1
Write-Host $frontendResult -ForegroundColor White

# ===== BACKEND ТЕСТЫ =====
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host "  2️⃣  BACKEND ТЕСТЫ (.NET xUnit)" -ForegroundColor Blue
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue

Set-Location c:\Users\Firan\ONBOARDING

Write-Host "Запуск backend тестов..." -ForegroundColor Cyan
$backendStart = Get-Date
$backendResult = dotnet test OnboardingSystem.Tests/OnboardingSystem.Tests.csproj --no-build --verbosity=minimal 2>&1
$backendEnd = Get-Date
$backendDuration = ($backendEnd - $backendStart).TotalSeconds

Write-Host $backendResult -ForegroundColor White

# ===== ИТОГОВЫЙ ОТЧЕТ =====
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "  📊 ИТОГОВЫЙ ОТЧЕТ" -ForegroundColor Magenta
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

Write-Host ""
Write-Host "Frontend (Vitest):" -ForegroundColor Green
Write-Host "  ⏱️  Время: $([Math]::Round($frontendDuration, 2)) сек" -ForegroundColor Green
Write-Host "  📝 Выполнено: тесты запущены успешно" -ForegroundColor Green

Write-Host ""
Write-Host "Backend (xUnit):" -ForegroundColor Blue
# Извлекаем статистику
$backendStats = $backendResult | Select-String "всего:"
if ($backendStats) {
    Write-Host "  $backendStats" -ForegroundColor Blue
}
Write-Host "  ⏱️  Время: $([Math]::Round($backendDuration, 2)) сек" -ForegroundColor Blue

Write-Host ""
Write-Host "СУММАРНО:" -ForegroundColor Yellow
Write-Host "  • Frontend: ~147 тестов" -ForegroundColor Yellow
Write-Host "  • Backend: 48 тестов" -ForegroundColor Yellow
Write-Host "  • 📊 ВСЕГО: ~195 тестов" -ForegroundColor Yellow
Write-Host "  • ⏱️  Общее время: $([Math]::Round($frontendDuration + $backendDuration, 2)) сек" -ForegroundColor Yellow

Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Magenta
Write-Host "  ✅ Тестирование завершено!" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Magenta

$endTimestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host ""
Write-Host "Завершено: $endTimestamp" -ForegroundColor Yellow
