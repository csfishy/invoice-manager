# Backend Tests

This test project focuses on production-relevant API flows:

- authentication and health checks
- bill CRUD, filtering, attachment validation, and audit logging
- category and reminder rule management
- dashboard summary and due-date behavior
- offline licensing status, invalid license import handling, and startup rejection for imported invalid licenses

Run the suite from the repository root:

```powershell
dotnet test backend/tests/InvoiceManager.Api.Tests.csproj
```
