@echo off
chcp 65001 >nul
echo ========================================================
echo   BIDADYU BT Yönetim Platformu - Canlı Başlatıcı
echo ========================================================
echo.

echo [1/3] Backend Servisi Başlatılıyor (Port: 5000)...
start "BIDADYU Backend API" cmd /k "title BIDADYU Backend API && dotnet run --project src\backend\BIDADYUManagement.Api\BIDADYUManagement.Api.csproj --launch-profile http"

echo [2/3] Frontend Web Arayüzü Başlatılıyor (Port: 5173)...
start "BIDADYU Frontend UI" cmd /k "title BIDADYU Frontend UI && cd src\frontend && npm run dev"

echo [3/3] Agent Uygulaması Başlatılıyor (System Tray)...
if exist "src\agent\BIDADYUAgent\bin\Release\net8.0-windows\win-x64\BIDADYUAgent.exe" (
    start "" "src\agent\BIDADYUAgent\bin\Release\net8.0-windows\win-x64\BIDADYUAgent.exe"
) else (
    start "" "src\agent\BIDADYUAgent\bin\Debug\net8.0-windows\BIDADYUAgent.exe"
)

echo.
echo ========================================================
echo   Sistem Tüm Servisleriyle Başarıyla Başlatıldı!
echo   Web Arayüzü: http://localhost:5173
echo   API Swagger: http://localhost:5000/swagger
echo ========================================================
echo.
pause
