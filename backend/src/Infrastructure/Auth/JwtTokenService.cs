using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InvoiceManager.Application.Auth;
using InvoiceManager.Domain.Users;
using InvoiceManager.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InvoiceManager.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AuthResponseDto CreateToken(AppUser user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.LifetimeMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new("display_name", user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto(
            accessToken,
            expiresAtUtc,
            user.Id,
            user.Username,
            user.DisplayName,
            user.Role);
    }
}
