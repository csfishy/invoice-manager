# Deployment Specification

## Target Environment

- Windows customer workstation or server
- Docker Desktop running with Linux containers
- local persistent storage for database, uploads, license files, and backups

## Runtime Components

- `frontend` container for the Next.js admin UI
- `backend` container for the ASP.NET Core API
- `postgres` container for the application database

## Container Build Files

- [frontend/Dockerfile](/D:/Users/csfishy/Documents/GitHub/invoice-manager/frontend/Dockerfile)
- [backend/Dockerfile](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/Dockerfile)
- [docker-compose.yml](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docker-compose.yml)
- [.env.example](/D:/Users/csfishy/Documents/GitHub/invoice-manager/.env.example)

## Persistent Data Layout

- `./data/postgres` stores PostgreSQL data files
- `./data/uploads` stores bill attachments
- `./data/license` stores imported license files
- `./backups` stores logical backups created by Windows scripts

These folders must remain outside container lifecycles and should be included in customer file-system backup policy.

## Health Checks

The Compose stack includes health checks for:

- PostgreSQL with `pg_isready`
- backend API with `/api/health`
- frontend UI with the root page

Service startup order uses health-based dependency conditions so the frontend waits for the backend and the backend waits for PostgreSQL.

## Installation Flow

1. Install Docker Desktop and confirm the Docker engine is running.
2. Extract the release package to a stable folder such as `D:\InvoiceManager`.
3. Copy `.env.example` to `.env`.
4. Update secrets, ports, CORS, and licensing values for the customer environment.
5. Run [install.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/install.bat).
6. Open `http://localhost:3000` or the configured frontend URL.
7. Sign in with the initial admin credentials from `.env`.
8. Import the customer license file.
9. Verify dashboard, bills, audit logs, and licensing pages are available.

## Operations Scripts

- [install.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/install.bat): pulls/builds images and starts the stack
- [start.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/start.bat): starts services
- [stop.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/stop.bat): stops services
- [backup.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/backup.bat): creates a timestamped backup
- [restore.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/restore.bat): restores a selected backup folder
- [import-license.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/import-license.bat): copies a customer license file into the mounted license path
- [update.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/update.bat): creates a safety backup, rebuilds images, and restarts services

## Customer Delivery Notes

- Keep `LICENSE_ALLOW_UNLICENSED_DEVELOPMENT_MODE=false` for customer deployments.
- Replace sample credentials and JWT secrets before customer handoff.
- Treat the `.env` file as sensitive operational configuration.
