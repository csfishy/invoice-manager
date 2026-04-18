# Deployment Specification

## Target

Customer self-hosted deployment on a Windows environment.

## Runtime Components

- frontend container
- backend container
- postgres container

## Persistent Data

- `./data/postgres` for PostgreSQL data
- `./data/uploads` for uploaded bill files
- `./data/license` for imported license files
- `./backups` for exported operational backups

## Required Files

- `docker-compose.yml`
- `.env`
- `backend/dotnet-tools.json`
- `deployment/windows/install.bat`
- `deployment/windows/start.bat`
- `deployment/windows/stop.bat`
- `deployment/windows/backup.bat`
- `deployment/windows/restore.bat`
- `deployment/windows/import-license.bat`
- `deployment/windows/update.bat`

## Installation Flow

1. Install Docker Desktop on the target Windows machine.
2. Extract the application package to a stable local path.
3. Copy `.env.example` to `.env`.
4. Update environment variables, secrets, ports, and storage settings.
5. Run `deployment\windows\install.bat`.
6. Open the local frontend URL in the browser.
7. Sign in with the seeded admin credentials.
8. Import the customer license file.
9. Verify the license status from the admin page.

## Backup Scope

- PostgreSQL logical dump
- uploaded files
- license file data

## Restore Scope

- database contents
- uploaded files
- license file data

## Upgrade Scope

- replace application package or image source
- preserve mounted data folders
- rebuild and restart containers with `update.bat`
