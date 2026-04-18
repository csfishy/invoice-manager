using InvoiceManager.Application.Licensing;
using InvoiceManager.Licensing.Configuration;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Api.Auth;

public sealed class RequireValidLicenseFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<LicensingOptions>>().Value;
        if (options.AllowUnlicensedDevelopmentMode && context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return await next(context);
        }

        var licenseStatusService = context.HttpContext.RequestServices.GetRequiredService<ILicenseStatusService>();
        var status = await licenseStatusService.GetCurrentStatusAsync(context.HttpContext.RequestAborted);

        if (!status.IsValid)
        {
            return Results.Json(new
            {
                title = status.Message,
                status = status.Status,
                requiresActivation = status.RequiresActivation
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}
