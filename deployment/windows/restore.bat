@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
cd /d "%ROOT_DIR%"

if "%~1"=="" (
  echo Usage: restore.bat ^<backup-folder-path^>
  exit /b 1
)

set "BACKUP_DIR=%~1"

if not exist "%BACKUP_DIR%\database.sql" (
  echo Backup database.sql not found in %BACKUP_DIR%
  exit /b 1
)

call :require_docker || exit /b 1
call :require_env || exit /b 1
call :load_env || exit /b 1

echo Stopping application containers before restore...
docker compose stop frontend backend >nul 2>nul

echo Recreating public schema...
docker compose exec -T postgres psql -U %POSTGRES_USER% -d %POSTGRES_DB% -c "DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;"
if errorlevel 1 (
  echo Failed to reset the database schema.
  exit /b 1
)

echo Restoring database...
type "%BACKUP_DIR%\database.sql" | docker compose exec -T postgres psql -U %POSTGRES_USER% -d %POSTGRES_DB%
if errorlevel 1 (
  echo Database restore failed.
  exit /b 1
)

if exist "%BACKUP_DIR%\uploads" (
  echo Restoring uploaded files...
  if not exist "data\uploads" mkdir "data\uploads"
  xcopy /E /I /Y "%BACKUP_DIR%\uploads" "data\uploads" >nul
)

if exist "%BACKUP_DIR%\license" (
  echo Restoring license data...
  if not exist "data\license" mkdir "data\license"
  xcopy /E /I /Y "%BACKUP_DIR%\license" "data\license" >nul
)

echo Starting application containers again...
docker compose up -d backend frontend || exit /b 1

echo Restore completed.
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

:load_env
for /f "usebackq tokens=1,* delims==" %%A in (".env") do (
  if /i not "%%A"=="" if /i not "%%A:~0,1%"=="#" set "%%A=%%B"
)
exit /b 0
