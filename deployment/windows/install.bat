@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

if not exist ".env" (
  echo .env not found. Copy .env.example to .env and update the values first.
  exit /b 1
)

if not exist "data\postgres" mkdir "data\postgres"
if not exist "data\uploads" mkdir "data\uploads"
if not exist "data\license" mkdir "data\license"

echo Pulling and building containers...
docker compose pull
docker compose build

echo Starting services...
docker compose up -d

echo Install completed.
endlocal
