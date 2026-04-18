@echo off
setlocal

set "ROOT_DIR=%~dp0..\.."
set "BACKUP_ROOT=%ROOT_DIR%\backups"
set "STAMP=%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%-%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%"
set "STAMP=%STAMP: =0%"
set "TARGET_DIR=%BACKUP_ROOT%\%STAMP%"

cd /d "%ROOT_DIR%"

if not exist ".env" (
  echo .env not found.
  exit /b 1
)

for /f "usebackq tokens=1,* delims==" %%A in (".env") do (
  if /i not "%%A"=="" if /i not "%%A:~0,1%"=="#" set "%%A=%%B"
)

if not exist "%BACKUP_ROOT%" mkdir "%BACKUP_ROOT%"
if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

echo Exporting database...
docker compose exec -T postgres pg_dump -U %POSTGRES_USER% -d %POSTGRES_DB% > "%TARGET_DIR%\database.sql"

if exist "data\uploads" (
  echo Copying uploaded files...
  xcopy /E /I /Y "data\uploads" "%TARGET_DIR%\uploads" > nul
)

if exist "data\license" (
  echo Copying license data...
  xcopy /E /I /Y "data\license" "%TARGET_DIR%\license" > nul
)

echo Backup created at %TARGET_DIR%
endlocal
