@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

docker version >nul 2>nul
if errorlevel 1 (
  echo Docker Desktop is not running. Nothing to stop through docker compose.
  exit /b 1
)

docker compose down || exit /b 1

echo Services stopped.
exit /b 0
