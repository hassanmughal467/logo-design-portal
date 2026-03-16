@echo off
echo ========================================
echo   Production Database Migration Fix
echo ========================================
echo.

REM Try common MySQL locations
set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe
if exist "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe
if exist "C:\xampp\mysql\bin\mysql.exe" set MYSQL_PATH=C:\xampp\mysql\bin\mysql.exe
if "%MYSQL_PATH%"=="" set MYSQL_PATH=mysql

echo Running migration...
echo You will be asked for your MySQL root password.
echo.

"%MYSQL_PATH%" -h localhost -u root -p LogoDesignPortalDb < "%~dp0fix-standardprice.sql"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Migration completed.
    echo Restart your API in IIS: Application Pools -^> Recycle
) else (
    echo.
    echo FAILED. Check: 1) MySQL is running  2) Password is correct  3) Database LogoDesignPortalDb exists
)

echo.
pause
