@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

docker version >nul 2>nul
if errorlevel 1 (
  echo Docker Desktop is not running. Start Docker Desktop and retry.
  exit /b 1
)

docker compose up -d || exit /b 1

echo Services started.
exit /b 0
