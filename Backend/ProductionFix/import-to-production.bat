@echo off
echo ========================================
echo   Reset Production - Import Local Backup
echo ========================================
echo.

set DB_NAME=LogoDesignPortalDb
set BACKUP=local_full_backup.sql

REM Try common MySQL paths
set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe
if exist "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe
if exist "C:\xampp\mysql\bin\mysql.exe" set MYSQL_PATH=C:\xampp\mysql\bin\mysql.exe
if "%MYSQL_PATH%"=="" set MYSQL_PATH=mysql

if not exist "%BACKUP%" (
    echo ERROR: %BACKUP% not found!
    echo Copy it from your local machine first.
    pause
    exit /b 1
)

echo WARNING: This will DELETE all production data and replace with local backup.
echo.
set /p confirm=Type YES to continue: 
if /i not "%confirm%"=="YES" (
    echo Cancelled.
    pause
    exit /b 0
)

echo.
echo Dropping and recreating database...
"%MYSQL_PATH%" -h localhost -u root -p --default-auth=mysql_native_password -e "DROP DATABASE IF EXISTS %DB_NAME%; CREATE DATABASE %DB_NAME% CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

if %ERRORLEVEL% NEQ 0 (
    echo Failed to reset database.
    pause
    exit /b 1
)

echo.
echo Importing backup...
"%MYSQL_PATH%" -h localhost -u root -p --default-auth=mysql_native_password %DB_NAME% < "%BACKUP%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Production database reset.
    echo Restart IIS Application Pool: Application Pools -^> Recycle
) else (
    echo.
    echo Import failed. Check MySQL password and connection.
)

echo.
pause
