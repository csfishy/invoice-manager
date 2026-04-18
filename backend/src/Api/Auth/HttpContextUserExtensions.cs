using System.Security.Claims;

namespace InvoiceManager.Api.Auth;

public static class HttpContextUserExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal user)
    {
        var subject = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return Guid.Parse(subject ?? throw new InvalidOperationException("Authenticated user id is missing."));
    }

    public static string GetRequiredUsername(this ClaimsPrincipal user)
    {
        return user.Identity?.Name ?? throw new InvalidOperationException("Authenticated username is missing.");
    }
}
