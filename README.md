# Invoice Manager

Self-hosted bill management system scaffold for customer deployment on Windows environments.

## Goal

This repository provides the initial project structure for a utility and tax bill management platform with:

- bill management for water, electricity, gas, and tax bills
- offline machine-bound licensing
- clear frontend and backend separation
- customer self-hosted deployment with Docker Compose
- Windows operational scripts for install, start, stop, backup, restore, update, and license import

## Planned Stack

- Frontend: Next.js + TypeScript
- Backend: ASP.NET Core Web API
- Database: PostgreSQL
- Deployment: Docker Compose
- Host environment: Windows customer machines or servers

## Repository Structure

```text
invoice-manager/
  AGENTS.md
  README.md
  .env.example
  docker-compose.yml
  frontend/
  backend/
  deployment/windows/
  docs/spec/
```

## What Is Included

- root environment template
- Docker Compose definition for frontend, backend, and PostgreSQL
- frontend and backend starter folders
- Windows deployment and operations scripts
- product, architecture, API, licensing, and deployment specifications

## What Is Not Implemented Yet

This scaffold does not yet include the full Next.js application or ASP.NET Core source implementation. It establishes the repo layout, operational scripts, and technical planning documents so development can begin on a clean structure.

## Quick Start

1. Copy `.env.example` to `.env`.
2. Adjust ports, secrets, and storage values for the customer environment.
3. Review the docs in [docs/spec](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec).
4. Add the frontend and backend application code into `frontend/` and `backend/src/`.
5. Use the Windows scripts in [deployment/windows](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows) for local operations.

## Key Docs

- [PRD](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/prd.md)
- [Architecture](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/architecture.md)
- [API](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/api.md)
- [Licensing](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/licensing.md)
- [Deployment](/D:/Users/csfishy/Documents/GitHub/invoice-manager/docs/spec/deployment.md)

## Next Recommended Steps

- scaffold the Next.js admin frontend
- scaffold the ASP.NET Core Web API solution with layered projects
- implement authentication, RBAC, audit logging, and bill CRUD
- implement offline license verification using an embedded public key
- add tests for API flows, license validation, and startup failure conditions
