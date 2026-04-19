@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

call :require_docker || exit /b 1
call :require_env || exit /b 1

echo Creating safety backup before upgrade...
call "%~dp0backup.bat"
if errorlevel 1 (
  echo Pre-upgrade backup failed. Upgrade cancelled.
  exit /b 1
)

echo Rebuilding application images...
docker compose build || exit /b 1

echo Restarting services with updated images...
docker compose up -d || exit /b 1

echo Upgrade completed.
exit /b 0

:require_docker
docker version >nul 2>nul
if errorlevel 1 (
  echo Docker Desktop is not running. Start Docker Desktop and retry.
  exit /b 1
)
exit /b 0

:require_env
if not exist ".env" (
  echo .env not found.
  exit /b 1
)
exit /b 0
