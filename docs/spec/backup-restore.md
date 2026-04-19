# Backup and Restore

## Backup Scope

The customer backup set includes:

- PostgreSQL logical dump as `database.sql`
- uploaded attachment files from `data/uploads`
- imported license files from `data/license`
- a simple `manifest.txt` describing the backup contents

## Creating a Backup

Run:

- [backup.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/backup.bat)

Output:

- `backups/<timestamp>/database.sql`
- `backups/<timestamp>/uploads`
- `backups/<timestamp>/license`
- `backups/<timestamp>/manifest.txt`

Recommended practice:

- create a backup before every upgrade
- keep a rotating off-machine copy according to customer policy
- verify the backup folder contains `database.sql` before considering the operation complete

## Restoring a Backup

Run:

- `deployment\windows\restore.bat <backup-folder-path>`

Restore behavior:

- stops frontend and backend containers
- drops and recreates the PostgreSQL public schema
- imports `database.sql`
- restores uploads and license files if present
- restarts backend and frontend containers

## Restore Warnings

- restore is destructive for the current database contents
- confirm the selected backup folder is the intended recovery point
- if the customer wants a rollback option, create a fresh backup before restore

## Validation After Restore

1. Open the frontend URL.
2. Sign in with an admin account.
3. Confirm bills, attachments, and dashboard totals appear correct.
4. Confirm the licensing page still shows the expected customer license.
