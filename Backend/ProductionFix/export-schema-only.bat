@echo off
echo ========================================
echo   Export LOCAL Schema Only (no data)
echo ========================================
echo.

set DB_NAME=LogoDesignPortalDb
set OUTPUT=local_schema_only.sql

set MYSQL_PATH=
if exist "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe
if exist "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysqldump.exe" set MYSQL_PATH=C:\Program Files\MySQL\MySQL Server 5.7\bin\mysqldump.exe
if exist "C:\xampp\mysql\bin\mysqldump.exe" set MYSQL_PATH=C:\xampp\mysql\bin\mysqldump.exe
if "%MYSQL_PATH%"=="" set MYSQL_PATH=mysqldump

echo Exporting SCHEMA ONLY from %DB_NAME% (no data)...
echo Output: %OUTPUT%
echo.

"%MYSQL_PATH%" -h 127.0.0.1 -u root -p --default-auth=mysql_native_password --no-data --routines --triggers %DB_NAME% > "%OUTPUT%"

if %ERRORLEVEL% EQU 0 (
    echo SUCCESS! Copy %OUTPUT% to production server and run import-schema-only.bat
) else (
    echo FAILED. Try MySQL Workbench: Server -^> Data Export -^> check "Dump Structure Only"
)

echo.
pause
