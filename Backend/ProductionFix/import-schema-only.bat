@echo off
echo ========================================
echo   Import Schema to Production
echo ========================================
echo.

set DB_NAME=LogoDesignPortalDb
set BACKUP=local_schema_only.sql

set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe
if exist "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe
if exist "C:\xampp\mysql\bin\mysql.exe" set MYSQL_PATH=C:\xampp\mysql\bin\mysql.exe
if "%MYSQL_PATH%"=="" set MYSQL_PATH=mysql

if not exist "%BACKUP%" (
    echo ERROR: %BACKUP% not found! Copy it from local first.
    pause
    exit /b 1
)

echo This will REPLACE production schema with local schema.
echo Production DATA will be LOST. Tables will be recreated with correct structure.
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
    echo Failed. Add -P 3307 if MySQL uses port 3307.
    pause
    exit /b 1
)

echo Importing schema...
"%MYSQL_PATH%" -h localhost -u root -p --default-auth=mysql_native_password %DB_NAME% < "%BACKUP%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! Production schema updated. Recycle IIS Application Pool.
) else (
    echo Import failed.
)

echo.
pause
