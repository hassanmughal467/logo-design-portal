@echo off
echo ========================================
echo   Starting Frontend Server
echo ========================================
echo.
cd /d "%~dp0Frontend"
if not exist "node_modules" (
    echo Installing dependencies...
    call npm install
)
echo.
echo Starting Angular dev server...
echo Frontend will run on: http://0.0.0.0:4200
echo Share this URL: http://192.168.100.54:4200
echo.
echo Keep this window open!
echo.
call npm run start -- --host 0.0.0.0
pause
