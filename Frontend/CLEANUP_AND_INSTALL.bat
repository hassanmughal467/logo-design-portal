@echo off
echo ========================================
echo Cleaning Frontend Dependencies
echo ========================================
echo.

echo Step 1: Removing node_modules...
if exist node_modules (
    rd /s /q node_modules
    echo ✓ node_modules removed
) else (
    echo ℹ node_modules not found
)

echo.
echo Step 2: Removing package-lock.json...
if exist package-lock.json (
    del package-lock.json
    echo ✓ package-lock.json removed
) else (
    echo ℹ package-lock.json not found
)

echo.
echo Step 3: Cleaning npm cache...
call npm cache clean --force
echo ✓ npm cache cleaned

echo.
echo ========================================
echo Installing Dependencies
echo ========================================
echo.

echo Step 4: Running npm install...
call npm install

echo.
echo ========================================
echo Done! You can now run: npm start
echo ========================================
pause
