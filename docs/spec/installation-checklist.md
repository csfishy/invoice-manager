# Customer Installation Checklist

## Before Installation

- Docker Desktop installed
- Docker engine running
- application package extracted to a stable folder
- `.env` created from `.env.example`
- production secrets and passwords updated
- customer license file received or request-code process ready

## During Installation

- run [install.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/install.bat)
- confirm `docker compose ps` shows all services as running
- confirm backend health responds on `http://localhost:8080/api/health`
- confirm frontend opens on the configured URL

## After Installation

- sign in with admin credentials
- import customer license file
- confirm license status shows valid
- create or review a sample bill
- confirm uploads path is writable by uploading an attachment
- confirm audit logs are visible
- run a manual backup with [backup.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/backup.bat)

## Handover Items

- customer knows where `.env`, `data`, and `backups` folders are stored
- customer has the Windows script shortcuts or instructions
- customer has documented admin credentials
- customer understands backup cadence and restore ownership
