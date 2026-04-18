using System.Security.Claims;
using System.Text;
using InvoiceManager.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceManager.Api.Auth;

public sealed class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name is not null && name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}
