@echo off
echo ========================================
echo   Starting Frontend Server
echo ========================================
echo.
echo Login page: http://localhost:4200/auth/login
echo Keep this window open!
echo.
cd /d "%~dp0Frontend"
ng serve --host 0.0.0.0
pause
