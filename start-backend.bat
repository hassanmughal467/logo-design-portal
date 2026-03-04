@echo off
echo ========================================
echo   Starting Backend Server
echo ========================================
echo.
echo Backend API: https://localhost:5001
echo Swagger UI:  https://localhost:5001/swagger
echo Keep this window open!
echo.
cd /d "%~dp0Backend\src\LogoDesignPortal.API"
dotnet run --launch-profile https
pause
