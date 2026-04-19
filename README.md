# Invoice Manager

Self-hosted bill management system for customer-operated Windows environments.

## Included Capabilities

- login with role-based access for `Admin`, `Operator`, and `Viewer`
- bill CRUD for water, electricity, gas, and tax bills
- bill category management for Admin users
- attachment upload and download for bill files
- attachment validation for PDF and common image formats stored on mounted volume paths
- search and filtering by bill type, payment status, issue date, due date, billing period, customer, keyword, and attachment presence
- dashboard summary with unpaid totals, due-soon and overdue lists, latest uploads, and storage usage
- reminder rule settings for Admin users
- CSV export and printable summary reports for filtered bill lists
- audit log tracking for important actions
- offline-first machine-bound licensing status and license import
- offline request-code workflow for vendor-issued signed licenses
- Docker Compose deployment with Windows helper scripts

## Tech Stack

- Frontend: Next.js + TypeScript
- Backend: ASP.NET Core Web API
- Database: PostgreSQL
- Deployment: Docker Compose

## Current Architecture

- [frontend](/D:/Users/csfishy/Documents/GitHub/invoice-manager/frontend) contains the admin UI
- [backend/src/Api](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Api) hosts the Web API
- [backend/src/Application](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Application) contains DTOs and service contracts
- [backend/src/Domain](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Domain) contains core entities and enums
- [backend/src/Infrastructure](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Infrastructure) contains EF Core, auth, storage, seed data, and migrations
- [backend/src/Licensing](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Licensing) contains machine fingerprinting and offline license validation

## Monorepo Structure

```text
invoice-manager/
  frontend/
  backend/
  deployment/
  docs/
```

## Local Setup

1. Copy `.env.example` to `.env`.
2. Update secrets, ports, and storage values.
3. Restore the local EF tool with `dotnet tool restore` in [backend](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend) if you need to manage migrations manually.
4. Run [deployment/windows/install.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/install.bat) on Windows, or run `docker compose up -d --build`.
5. Open `http://localhost:3000`.
6. Sign in with the seeded admin account from `.env`.
7. Import a license file from the Licensing screen if you have one.
8. In development only, you may set `LICENSE_ALLOW_UNLICENSED_DEVELOPMENT_MODE=true` to bypass license enforcement while keeping status visibility.

## Development Defaults

- Admin username: value from `DEFAULT_ADMIN_USERNAME`
- Admin password: value from `DEFAULT_ADMIN_PASSWORD`
- Seeded secondary users:
  - `operator` / `operator123!`
  - `viewer` / `viewer123!`

## Validation

- `dotnet test backend/tests/InvoiceManager.Api.Tests.csproj`
- `dotnet run --project backend/src/Api/InvoiceManager.Api.csproj`
- `npm.cmd install && npm.cmd run dev` in [frontend](/D:/Users/csfishy/Documents/GitHub/invoice-manager/frontend)
- `npm.cmd run build` in [frontend](/D:/Users/csfishy/Documents/GitHub/invoice-manager/frontend)

## Licensing Note

The application validates license files using an embedded sample public key. For a real customer release, replace the embedded public key in [EmbeddedLicenseKeyProvider.cs](/D:/Users/csfishy/Documents/GitHub/invoice-manager/backend/src/Licensing/Services/EmbeddedLicenseKeyProvider.cs) with your production public key and keep the corresponding private signing key outside the application.

## Key Docs

- [PRD](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/prd.md)
- [Architecture](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/architecture.md)
- [API](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/api.md)
- [Licensing](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/licensing.md)
- [Deployment](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/deployment.md)
