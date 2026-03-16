@echo off
echo ========================================
echo   Export Local Database for Production
echo ========================================
echo.

set DB_NAME=LogoDesignPortalDb
set OUTPUT=local_full_backup.sql

REM Try common MySQL paths
set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe
if exist "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysqldump.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 5.7\bin\mysqldump.exe
if exist "C:\xampp\mysql\bin\mysqldump.exe" set MYSQL_PATH=C:\xampp\mysql\bin\mysqldump.exe
if "%MYSQL_PATH%"=="" set MYSQL_PATH=mysqldump

echo Exporting %DB_NAME% to %OUTPUT%...
echo You will be asked for your MySQL password.
echo.

"%MYSQL_PATH%" -h 127.0.0.1 -u root -p --default-auth=mysql_native_password --single-transaction --routines --triggers %DB_NAME% > "%OUTPUT%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo SUCCESS! File created: %OUTPUT%
    echo Copy this file to the production server and run the import (see RESET_PRODUCTION_FROM_LOCAL.md)
) else (
    echo.
    echo FAILED. Check: 1) MySQL running  2) Password correct  3) Database %DB_NAME% exists
)

echo.
pause
