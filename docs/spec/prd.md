# Product Requirements Document

## Product Name

Invoice Manager

## Goal

Provide a self-hosted bill management platform for customer environments to manage utility and tax bills with offline machine-bound licensing.

## Target Users

- Admin
- Billing operator
- Read-only viewer

## Supported Bill Types

- Water
- Electricity
- Gas
- Tax

## Core User Stories

- As an admin, I can create users and manage permissions.
- As an operator, I can create, edit, and search bills.
- As an operator, I can upload bill attachments.
- As an operator, I can filter unpaid and overdue bills.
- As an admin, I can export bill data.
- As an admin, I can review audit logs.
- As an admin, I can view current license status.
- As a customer, I can install the system locally and activate it with a machine-bound license.

## Functional Requirements

- Authentication and authorization
- Bill CRUD
- Attachment upload and association
- Filtering and search
- Dashboard and reminders
- Export
- Audit logs
- Licensing
- Backup and restore
- Customer self-hosted deployment

## Non-Functional Requirements

- Windows-friendly deployment
- Offline-capable license validation
- Secure signature-based license verification
- Maintainable codebase
- Persistent file and database storage

## Success Criteria

- Customer can deploy the full stack with Docker Compose on Windows.
- Operators can manage all supported bill types from a clear admin UI.
- Admins can inspect license validity without internet access.
- Backups can be created and restored with documented scripts.
- The system remains maintainable through clear separation of concerns.

