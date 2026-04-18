# API Specification

## Base Path

`/api`

## Authentication

- `POST /api/auth/login`
- `GET /api/me`

## Dashboard

- `GET /api/dashboard/summary`

## Bills

- `GET /api/bills`
- `POST /api/bills`
- `GET /api/bills/{id}`
- `PUT /api/bills/{id}`
- `DELETE /api/bills/{id}`

### Bill Query Filters

- bill type
- status
- issue date range
- due date range
- billing period range
- account number
- provider or authority name
- customer name
- free-text search

## Attachments

- `POST /api/bills/{id}/attachments`
- `GET /api/attachments/{attachmentId}`
- `DELETE /api/attachments/{attachmentId}`

## Audit Logs

- `GET /api/audit-logs?page=1&pageSize=20`

## Licensing

- `GET /api/license/status`
- `POST /api/license/import`
- `GET /api/license/fingerprint`

## Health and Startup Checks

- `GET /api/health`
- `GET /api/health/license`

## Write Endpoint Rules

- All write endpoints use explicit DTOs.
- All write endpoints require validation.
- Important write actions generate audit logs.
- Role-based authorization applies to all non-public endpoints.

## Initial Role Expectations

- Admin: full access
- Operator: bill and attachment management
- Viewer: read-only access
