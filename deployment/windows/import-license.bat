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

copy /Y "%~1" "data\license\license.json" > nul

echo License imported to data\license\license.json
endlocal
