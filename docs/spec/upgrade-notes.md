# Upgrade Notes

## Upgrade Principles

- preserve mounted data folders
- back up before applying any application image changes
- keep customer `.env` values unless a new release explicitly requires additions

## Standard Upgrade Procedure

1. Review the new release package and updated `.env.example`.
2. Merge any newly required environment variables into the customer `.env`.
3. Run [backup.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/backup.bat).
4. Replace application source files or package contents while preserving `data/` and `.env`.
5. Run [update.bat](/D:/Users/csfishy/Documents/GitHub/invoice-manager/deployment/windows/update.bat).
6. Verify health, login, licensing, dashboard, and bill workflows.

## Rollback Guidance

If the new build fails validation:

1. stop services
2. restore the prior package contents
3. restore the latest known-good backup if data changes must be reversed
4. start services again and revalidate

## Configuration Review Checklist

- confirm any new environment variables are present
- keep `JWT_SIGNING_KEY` and database credentials unchanged unless intentionally rotated
- keep `LICENSE_ALLOW_UNLICENSED_DEVELOPMENT_MODE=false` in customer environments
- verify storage paths and exposed ports still match the customer host
