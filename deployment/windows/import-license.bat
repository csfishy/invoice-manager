@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

if "%~1"=="" (
  echo Usage: import-license.bat ^<license-file-path^>
  exit /b 1
)

if not exist "%~1" (
  echo License file not found: %~1
  exit /b 1
)

if not exist "data\license" mkdir "data\license"

copy /Y "%~1" "data\license\license.json" >nul
if errorlevel 1 (
  echo Failed to copy the license file.
  exit /b 1
)

echo License imported to data\license\license.json
echo Restart backend if the application is already running:
echo   docker compose restart backend
exit /b 0
