@echo off
echo ========================================================
echo BIDADYU BT Yonetim Platformu - Docker Baslatici
echo ========================================================
echo.

echo Docker Containerlari Baslatiliyor...
echo - Backend API (Port: 5000)
echo - Frontend UI (Port: 5173)
echo - Agent Service
echo.

docker compose up --pull always

echo.
echo ========================================================
echo Sistem Tum Servisleriyle Basariyla Baslatildi!
echo Web Arayuzu: http://localhost:5173
echo API Swagger: http://localhost:5000/swagger
echo ========================================================
echo.
pause
