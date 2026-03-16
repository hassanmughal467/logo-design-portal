@echo off
REM Run ApplyAllMigrations.sql on production MySQL
REM Edit the variables below for your environment

cd /d "%~dp0"

set MYSQL_HOST=localhost
set MYSQL_USER=root
set MYSQL_DATABASE=LogoDesignPortalDb

echo.
echo Running migration script on %MYSQL_DATABASE%...
echo.

mysql -h %MYSQL_HOST% -u %MYSQL_USER% -p %MYSQL_DATABASE% < "ApplyAllMigrations.sql"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Migration completed successfully.
) else (
    echo.
    echo Migration failed. Check:
    echo   - MySQL is installed and "mysql" is in PATH
    echo   - Host, user, and database name are correct
    echo   - Password is valid
)

echo.
pause
