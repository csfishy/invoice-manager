@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

call :require_docker || exit /b 1
call :require_env || exit /b 1
call :ensure_data_dirs || exit /b 1

echo Pulling base images...
docker compose pull || exit /b 1

echo Building application images...
docker compose build || exit /b 1

echo Starting services...
docker compose up -d || exit /b 1

echo.
echo Install completed.
echo Frontend: http://localhost:3000
echo Backend health: http://localhost:8080/api/health
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
  echo .env not found. Copy .env.example to .env and update customer values first.
  exit /b 1
)
exit /b 0

:ensure_data_dirs
if not exist "data" mkdir "data"
if not exist "data\postgres" mkdir "data\postgres"
if not exist "data\uploads" mkdir "data\uploads"
if not exist "data\license" mkdir "data\license"
if not exist "backups" mkdir "backups"
exit /b 0
