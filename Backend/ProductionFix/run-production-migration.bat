@echo off
echo ========================================
echo   Production Database Migration
echo   (Fixes 500 errors on orders/analytics)
echo ========================================
echo.

REM Database name - change if your production DB has a different name
set DB_NAME=LogoDesignPortalDb

REM Try common MySQL locations
set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe
if exist "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe
if exist "C:\xampp\mysql\bin\mysql.exe" set MYSQL_PATH=C:\xampp\mysql\bin\mysql.exe
if "%MYSQL_PATH%"=="" set MYSQL_PATH=mysql

echo IMPORTANT: Backup your database first!
echo   mysqldump -u root -p %DB_NAME% ^> backup.sql
echo.
echo Running migration on database: %DB_NAME%
echo You will be asked for your MySQL password.
echo.

"%MYSQL_PATH%" -h localhost -u root -p %DB_NAME% < "%~dp0production-migration-fix.sql"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Migration completed.
    echo Restart your API: IIS -^> Application Pools -^> Recycle
) else (
    echo.
    echo FAILED. Check: 1) MySQL is running  2) Password correct  3) Database %DB_NAME% exists
)

echo.
pause
