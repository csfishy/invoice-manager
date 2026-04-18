@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

echo Rebuilding application images...
docker compose build

echo Restarting services...
docker compose up -d

echo Update completed.
endlocal
