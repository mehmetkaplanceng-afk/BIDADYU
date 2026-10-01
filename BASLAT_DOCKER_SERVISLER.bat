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

docker compose up -d
echo.
echo Canli Docker Sunucu Loglari Izleniyor (Cikis icin Ctrl+C yapabilirsiniz)...
docker compose logs -f --tail=100

echo.
echo ========================================================
echo Sistem Tum Servisleriyle Basariyla Baslatildi!
echo Web Arayuzu: http://localhost:5173
echo API Swagger: http://localhost:5000/swagger
echo ========================================================
echo.
pause
