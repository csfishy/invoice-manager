# Invoice Manager

Self-hosted bill management system for customer-operated Windows environments. The product is designed for offline-capable customer installations that need practical billing operations, attachment storage, auditability, and machine-bound licensing.

## Included Capabilities

- JWT login with role-based access for `Admin`, `Operator`, and `Viewer`
- bill CRUD for water, electricity, gas, and tax bills
- bill category management and reminder rule settings
- attachment upload, metadata lookup, download, and validation
- dashboard cards, due-soon lists, overdue lists, latest uploads, and storage usage summary
- filtered search by bill type, payment status, customer, keyword, issue date, due date, period, and attachment presence
- CSV export and printable bill summary reports
- audit log tracking for important write operations and licensing actions
- offline-first machine-bound licensing with request-code and signed license import flow
- Docker Compose deployment with Windows helper scripts for install, start, stop, backup, restore, update, and license import

## Tech Stack

- Frontend: Next.js + TypeScript
- Backend: ASP.NET Core Web API
- Database: PostgreSQL
- Deployment: Docker Compose

## Project Structure

```text
invoice-manager/
  frontend/                     Next.js admin UI
  backend/                      ASP.NET Core Web API, EF Core, tests
    src/
      Api/                      minimal API endpoints and auth pipeline
      Application/              DTOs and service contracts
      Domain/                   entities and enums
      Infrastructure/           EF Core, auth, storage, seed data, migrations
      Licensing/                fingerprinting and offline license validation
    tests/                      integration-style API and licensing tests
  deployment/
    windows/                    customer-friendly Windows batch scripts
  docs/
    spec/                       product, API, deployment, licensing, backup docs
```

## Local Setup

1. Copy `.env.example` to `.env`.
2. Set at least `JWT_SIGNING_KEY`, `POSTGRES_PASSWORD`, and `LICENSE_FINGERPRINT_SALT` to non-default values.
3. Start Docker Desktop on Windows.
4. Run `docker compose up -d --build` from the repository root.
5. Open [http://localhost:3000](http://localhost:3000).
6. Sign in with the seeded admin account from `.env`.
7. Use the Licensing page to copy the machine request code or import a signed license file.

For local development without Docker:

1. Run `dotnet tool restore` in [backend](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend).
2. Start the API with `dotnet run --project backend/src/Api/InvoiceManager.Api.csproj`.
3. In [frontend](/D:/Users/csfishy/Documents/GitHub/invoice-manager/frontend), run `npm.cmd install` and `npm.cmd run dev`.
4. Open [http://localhost:3000](http://localhost:3000).

## Development Defaults

- Admin username: value from `DEFAULT_ADMIN_USERNAME`
- Admin password: value from `DEFAULT_ADMIN_PASSWORD`
- Seeded secondary users:
- `operator` / `operator123!`
- `viewer` / `viewer123!`

## Running Tests

Backend integration and licensing tests:

```powershell
dotnet test backend/tests/InvoiceManager.Api.Tests.csproj
```

Frontend production build verification:

```powershell
cd frontend
npm.cmd install
npm.cmd run build
```

Useful local checks:

```powershell
docker compose config
docker compose ps
docker compose logs backend --tail=100
```

## Environment Variables

| Variable | Required | Purpose |
| --- | --- | --- |
| `FRONTEND_PORT` | Yes | Host port mapped to the Next.js container. |
| `BACKEND_PORT` | Yes | Host port mapped to the API container. |
| `POSTGRES_DB` | Yes | PostgreSQL database name. |
| `POSTGRES_USER` | Yes | PostgreSQL username. |
| `POSTGRES_PASSWORD` | Yes | PostgreSQL password. |
| `POSTGRES_PORT` | Yes | Host port mapped to PostgreSQL. |
| `ASPNETCORE_ENVIRONMENT` | Yes | ASP.NET Core environment name. |
| `DATABASE_PROVIDER` | Yes | Active provider, typically `Postgres` or `Sqlite` in tests. |
| `JWT_ISSUER` | Yes | JWT issuer claim value. |
| `JWT_AUDIENCE` | Yes | JWT audience claim value. |
| `JWT_SIGNING_KEY` | Yes | Symmetric signing key for JWT token generation and validation. |
| `JWT_LIFETIME_MINUTES` | Yes | Access token lifetime in minutes. |
| `DEFAULT_ADMIN_USERNAME` | Yes | Seeded admin username for development and first login. |
| `DEFAULT_ADMIN_PASSWORD` | Yes | Seeded admin password for development and first login. |
| `LICENSE_FILE_PATH` | Yes | Mounted path where the imported license file is stored. |
| `LICENSE_FINGERPRINT_SALT` | Yes | Salt used when hashing the derived machine fingerprint. |
| `LICENSED_PRODUCT_NAME` | Yes | Product name expected in request codes and license files. |
| `LICENSE_ALLOW_UNLICENSED_DEVELOPMENT_MODE` | No | Development-only bypass for protected business endpoints when no valid license is installed. |
| `UPLOADS_PATH` | Yes | Mounted storage path for bill attachments. |
| `ATTACHMENTS_MAX_FILE_SIZE_BYTES` | Yes | Maximum attachment size accepted by the API. |
| `ATTACHMENTS_ALLOWED_EXTENSIONS` | Yes | Comma-separated allowlist for upload extensions. |
| `CORS_ALLOWED_ORIGINS` | Yes | Allowed frontend origins for browser API access. |
| `NEXT_PUBLIC_API_BASE_URL` | Yes | API base URL consumed by the Next.js frontend. |

## Licensing Note

The application validates license files using an embedded sample public key. Replace the public key in [EmbeddedLicenseKeyProvider.cs](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Licensing/Services/EmbeddedLicenseKeyProvider.cs) before a real customer release, and keep the matching private signing key outside the application.

Startup behavior is intentionally strict for imported licenses:

- first run with no license file still allows the app to start so the customer can copy a request code
- imported but invalid or mismatched licenses are rejected during startup outside the development bypass path
- protected business endpoints are blocked when the current license status is invalid

## Sample Screenshots

Placeholders for customer-facing documentation screenshots:

- `docs/screenshots/login-page.png` - login page
- `docs/screenshots/dashboard.png` - dashboard summary
- `docs/screenshots/bill-list.png` - bill list and filters
- `docs/screenshots/bill-detail.png` - bill detail and attachments
- `docs/screenshots/license-status.png` - license status and request code

## Key Docs

- [PRD](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/prd.md)
- [Architecture](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/architecture.md)
- [API](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/api.md)
- [Licensing](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/licensing.md)
- [Deployment](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/deployment.md)
- [Backup and Restore](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/backup-restore.md)
- [Upgrade Notes](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/upgrade-notes.md)
- [Installation Checklist](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/installation-checklist.md)
