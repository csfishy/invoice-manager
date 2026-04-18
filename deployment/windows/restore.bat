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

if not exist ".env" (
  echo .env not found.
  exit /b 1
)

for /f "usebackq tokens=1,* delims==" %%A in (".env") do (
  if /i not "%%A"=="" if /i not "%%A:~0,1%"=="#" set "%%A=%%B"
)

echo Restoring database...
type "%BACKUP_DIR%\database.sql" | docker compose exec -T postgres psql -U %POSTGRES_USER% -d %POSTGRES_DB%

if exist "%BACKUP_DIR%\uploads" (
  echo Restoring uploaded files...
  if not exist "data\uploads" mkdir "data\uploads"
  xcopy /E /I /Y "%BACKUP_DIR%\uploads" "data\uploads" > nul
)

if exist "%BACKUP_DIR%\license" (
  echo Restoring license data...
  if not exist "data\license" mkdir "data\license"
  xcopy /E /I /Y "%BACKUP_DIR%\license" "data\license" > nul
)

echo Restore completed.
endlocal
