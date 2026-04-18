# Project Rules

## Product Goal

Build a self-hosted bill management system for customer installation.

The system manages utility and tax bills:

- water
- electricity
- gas
- tax

It must support offline machine-bound licensing and customer self-installation.

## Tech Stack

- Frontend: Next.js + TypeScript
- Backend: ASP.NET Core Web API
- Database: PostgreSQL
- Deployment: Docker Compose
- OS target: Windows customer environment

## Architecture Rules

- Keep frontend and backend clearly separated.
- Keep licensing logic isolated in a dedicated module.
- Keep OCR/import logic isolated from core bill CRUD logic.
- All secrets must come from environment variables.
- Do not hardcode private keys or production secrets.
- Use clean and maintainable folder structure.
- Prefer explicit code over clever abstractions.

## Backend Rules

- Use DTOs for request/response models.
- Add validation for all write endpoints.
- Add audit logging for important operations.
- Add role-based access control.
- Keep business logic out of controllers.
- Use EF Core migrations.

## Frontend Rules

- Use clear admin-oriented UI.
- Prioritize forms, filters, tables, and detail pages.
- Keep styling simple and production-friendly.
- Use reusable components for form fields, tables, status badges, dialogs.

## Licensing Rules

- Licensing must be offline-first.
- Bind license to machine fingerprint.
- Fingerprint must be derived from multiple hardware identifiers.
- Only store hashed fingerprint.
- License file must be signed externally with asymmetric cryptography.
- App must validate with embedded public key only.
- Provide admin page to inspect license status.

## Delivery Rules

- Prepare for customer self-hosted deployment.
- Add Dockerfiles and docker-compose.yml.
- Add Windows scripts:
  - install
  - start
  - stop
  - backup
  - restore
  - import-license
- Persist database and uploaded files with volumes.
- Add deployment documentation.

## Testing Rules

- Add tests for core API flows.
- Add tests for license validation.
- Add tests for invalid startup conditions.
- Add practical seed data for development.

## Documentation Rules

- Update README whenever major capability changes.
- Add docs for deployment, backup/restore, and licensing.
- Write concise but practical instructions.

