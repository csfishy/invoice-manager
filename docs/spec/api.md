# API Specification

## Purpose

Define the initial backend API surface for the admin frontend and customer deployment workflows.

## Base Path

`/api`

## Authentication

- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

## Users and Roles

- `GET /api/users`
- `POST /api/users`
- `GET /api/users/{id}`
- `PUT /api/users/{id}`
- `PATCH /api/users/{id}/roles`
- `GET /api/roles`

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
- account number
- vendor or authority name
- free-text search

## Attachments

- `POST /api/bills/{id}/attachments`
- `GET /api/bills/{id}/attachments`
- `GET /api/attachments/{attachmentId}`
- `DELETE /api/attachments/{attachmentId}`

## Audit Logs

- `GET /api/audit-logs`
- `GET /api/audit-logs/{id}`

## Export

- `POST /api/exports/bills`

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
