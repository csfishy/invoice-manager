using System.Security.Claims;
using System.Text.Json.Serialization;
using InvoiceManager.Api.Auth;
using InvoiceManager.Api.Validation;
using InvoiceManager.Application.Auth;
using InvoiceManager.Application.Bills;
using InvoiceManager.Application.Health;
using InvoiceManager.Application.Licensing;
using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Bills;
using InvoiceManager.Domain.Users;
using InvoiceManager.Infrastructure.Configuration;
using InvoiceManager.Infrastructure.DependencyInjection;
using InvoiceManager.Infrastructure.Persistence;
using InvoiceManager.Licensing.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddLicensingServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser())
    .AddPolicy("OperatorAccess", policy => policy.RequireRole(UserRole.Operator.ToString(), UserRole.Admin.ToString()))
    .AddPolicy("AdminAccess", policy => policy.RequireRole(UserRole.Admin.ToString()));

builder.Services.AddCors(options =>
{
    options.AddPolicy("app", policy =>
    {
        var allowedOrigins = (builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? "http://localhost:3000")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("app");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new HealthStatusDto("ok", "invoice-manager-api", DateTime.UtcNow)));

app.MapGet("/api/health/license", async (ILicenseStatusService licenseStatusService, CancellationToken cancellationToken) =>
{
    return Results.Ok(await licenseStatusService.GetCurrentStatusAsync(cancellationToken));
});

app.MapPost("/api/auth/login", async (
    LoginRequestDto request,
    IAuthService authService,
    IAuditLogService auditLogService,
    CancellationToken cancellationToken) =>
{
    var errors = RequestValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var response = await authService.LoginAsync(request, cancellationToken);
    if (response is null)
    {
        await auditLogService.LogAsync(null, request.Username, "auth.login.failed", "User", request.Username, "Failed login attempt.", cancellationToken: cancellationToken);
        return Results.Unauthorized();
    }

    await auditLogService.LogAsync(response.UserId, response.Username, "auth.login.succeeded", "User", response.UserId.ToString(), "User logged in.", cancellationToken: cancellationToken);
    return Results.Ok(response);
});

app.MapGet("/api/me", async (ClaimsPrincipal user, IAuthService authService, CancellationToken cancellationToken) =>
{
    var currentUser = await authService.GetCurrentUserAsync(user.GetRequiredUserId(), cancellationToken);
    return currentUser is null ? Results.NotFound() : Results.Ok(currentUser);
}).RequireAuthorization("Authenticated");

app.MapGet("/api/dashboard/summary", async (IDashboardService dashboardService, CancellationToken cancellationToken) =>
{
    return Results.Ok(await dashboardService.GetSummaryAsync(cancellationToken));
}).RequireAuthorization("Authenticated");

app.MapGet("/api/bills", async (
    BillType? billType,
    PaymentStatus? paymentStatus,
    DateOnly? issueDateFrom,
    DateOnly? issueDateTo,
    DateOnly? dueDateFrom,
    DateOnly? dueDateTo,
    DateOnly? periodFrom,
    DateOnly? periodTo,
    string? customer,
    string? keyword,
    bool? hasAttachment,
    int? page,
    int? pageSize,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var query = new BillQueryDto(
        billType,
        paymentStatus,
        issueDateFrom,
        issueDateTo,
        dueDateFrom,
        dueDateTo,
        periodFrom,
        periodTo,
        customer,
        keyword,
        hasAttachment,
        page ?? 1,
        pageSize ?? 20);
    return Results.Ok(await billService.GetBillsAsync(query, cancellationToken));
}).RequireAuthorization("Authenticated");

app.MapGet("/api/categories", async (
    bool includeInactive,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    return Results.Ok(await billService.GetCategoriesAsync(includeInactive, cancellationToken));
}).RequireAuthorization("Authenticated");

app.MapGet("/api/categories/{categoryId:guid}", async (Guid categoryId, IBillService billService, CancellationToken cancellationToken) =>
{
    var category = await billService.GetCategoryAsync(categoryId, cancellationToken);
    return category is null ? Results.NotFound() : Results.Ok(category);
}).RequireAuthorization("Authenticated");

app.MapPost("/api/categories", async (
    CreateBillCategoryRequestDto request,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var errors = RequestValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    try
    {
        var created = await billService.CreateCategoryAsync(user.GetRequiredUserId(), user.GetRequiredUsername(), request, cancellationToken);
        return Results.Created($"/api/categories/{created.Id}", created);
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { title = exception.Message });
    }
}).RequireAuthorization("AdminAccess");

app.MapPut("/api/categories/{categoryId:guid}", async (
    Guid categoryId,
    UpdateBillCategoryRequestDto request,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var errors = RequestValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    try
    {
        var updated = await billService.UpdateCategoryAsync(categoryId, user.GetRequiredUserId(), user.GetRequiredUsername(), request, cancellationToken);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { title = exception.Message });
    }
}).RequireAuthorization("AdminAccess");

app.MapDelete("/api/categories/{categoryId:guid}", async (
    Guid categoryId,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var deleted = await billService.DeleteCategoryAsync(categoryId, user.GetRequiredUserId(), user.GetRequiredUsername(), cancellationToken);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { title = exception.Message });
    }
}).RequireAuthorization("AdminAccess");

app.MapGet("/api/bills/{billId:guid}", async (Guid billId, IBillService billService, CancellationToken cancellationToken) =>
{
    var bill = await billService.GetBillAsync(billId, cancellationToken);
    return bill is null ? Results.NotFound() : Results.Ok(bill);
}).RequireAuthorization("Authenticated");

app.MapPost("/api/bills", async (
    CreateBillRequestDto request,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var errors = RequestValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    try
    {
        var created = await billService.CreateBillAsync(user.GetRequiredUserId(), user.GetRequiredUsername(), request, cancellationToken);
        return Results.Created($"/api/bills/{created.Id}", created);
    }
    catch (InvalidOperationException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["billCategoryId"] = [exception.Message] });
    }
}).RequireAuthorization("OperatorAccess");

app.MapPut("/api/bills/{billId:guid}", async (
    Guid billId,
    UpdateBillRequestDto request,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var errors = RequestValidators.Validate(request);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    try
    {
        var updated = await billService.UpdateBillAsync(billId, user.GetRequiredUserId(), user.GetRequiredUsername(), request, cancellationToken);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    }
    catch (InvalidOperationException exception)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["billCategoryId"] = [exception.Message] });
    }
}).RequireAuthorization("OperatorAccess");

app.MapDelete("/api/bills/{billId:guid}", async (
    Guid billId,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var deleted = await billService.DeleteBillAsync(billId, user.GetRequiredUserId(), user.GetRequiredUsername(), cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("AdminAccess");

app.MapPost("/api/bills/{billId:guid}/attachments", async (
    Guid billId,
    HttpRequest request,
    ClaimsPrincipal user,
    IBillService billService,
    IFileStorageService fileStorageService,
    CancellationToken cancellationToken) =>
{
    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files["file"] ?? form.Files.FirstOrDefault();
    if (file is null || file.Length <= 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["A file is required."] });
    }

    if (!fileStorageService.IsSupportedExtension(file.FileName))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["file"] = ["Only PDF and common image formats are supported."]
        });
    }

    if (file.Length > fileStorageService.GetMaxFileSizeBytes())
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["file"] = [$"File size exceeds the {fileStorageService.GetMaxFileSizeBytes() / (1024 * 1024)} MB limit."]
        });
    }

    await using var stream = file.OpenReadStream();
    var attachment = await billService.AddAttachmentAsync(
        billId,
        user.GetRequiredUserId(),
        user.GetRequiredUsername(),
        file.FileName,
        file.ContentType,
        stream,
        cancellationToken);

    return attachment is null ? Results.NotFound() : Results.Ok(attachment);
}).RequireAuthorization("OperatorAccess");

app.MapGet("/api/attachments/{attachmentId:guid}/metadata", async (Guid attachmentId, IBillService billService, CancellationToken cancellationToken) =>
{
    var metadata = await billService.GetAttachmentAsync(attachmentId, cancellationToken);
    return metadata is null ? Results.NotFound() : Results.Ok(metadata);
}).RequireAuthorization("Authenticated");

app.MapGet("/api/attachments/{attachmentId:guid}", async (Guid attachmentId, IBillService billService, CancellationToken cancellationToken) =>
{
    var metadata = await billService.GetAttachmentAsync(attachmentId, cancellationToken);
    if (metadata is null)
    {
        return Results.NotFound();
    }

    var stream = await billService.OpenAttachmentAsync(attachmentId, cancellationToken);
    return stream is null
        ? Results.NotFound()
        : Results.File(stream, metadata.ContentType, metadata.OriginalFileName);
}).RequireAuthorization("Authenticated");

app.MapDelete("/api/attachments/{attachmentId:guid}", async (
    Guid attachmentId,
    ClaimsPrincipal user,
    IBillService billService,
    CancellationToken cancellationToken) =>
{
    var deleted = await billService.DeleteAttachmentAsync(attachmentId, user.GetRequiredUserId(), user.GetRequiredUsername(), cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("OperatorAccess");

app.MapGet("/api/audit-logs", async (
    int? page,
    int? pageSize,
    IAuditLogService auditLogService,
    CancellationToken cancellationToken) =>
{
    return Results.Ok(await auditLogService.GetAuditLogsAsync(page ?? 1, pageSize ?? 20, cancellationToken));
}).RequireAuthorization("AdminAccess");

app.MapGet("/api/license/status", async (ILicenseStatusService licenseStatusService, CancellationToken cancellationToken) =>
{
    return Results.Ok(await licenseStatusService.GetCurrentStatusAsync(cancellationToken));
}).RequireAuthorization("AdminAccess");

app.MapGet("/api/license/fingerprint", async (ILicenseStatusService licenseStatusService, CancellationToken cancellationToken) =>
{
    var fingerprint = await licenseStatusService.GetCurrentFingerprintHashAsync(cancellationToken);
    return Results.Ok(new LicenseFingerprintDto(fingerprint, DateTime.UtcNow));
}).RequireAuthorization("AdminAccess");

app.MapPost("/api/license/import", async (
    HttpRequest request,
    ClaimsPrincipal user,
    ILicenseStatusService licenseStatusService,
    IAuditLogService auditLogService,
    CancellationToken cancellationToken) =>
{
    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files["file"] ?? form.Files.FirstOrDefault();
    if (file is null || file.Length <= 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = ["A license file is required."] });
    }

    await using var stream = file.OpenReadStream();
    var result = await licenseStatusService.ImportLicenseAsync(stream, cancellationToken);

    await auditLogService.LogAsync(
        user.GetRequiredUserId(),
        user.GetRequiredUsername(),
        "license.imported",
        "License",
        result.LicenseId ?? "unknown",
        $"Imported license file {file.FileName}",
        new { file.FileName, result.Status, result.IsValid },
        cancellationToken);

    return Results.Ok(result);
}).RequireAuthorization("AdminAccess");

app.Run();

public partial class Program;
