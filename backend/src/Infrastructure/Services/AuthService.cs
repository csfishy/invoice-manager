using InvoiceManager.Application.Auth;
using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Users;
using InvoiceManager.Infrastructure.Auth;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Services;

public sealed class AuthService(
    InvoiceManagerDbContext dbContext,
    IPasswordHasher<AppUser> passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(
            x => x.Username.ToLower() == normalizedUsername,
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return jwtTokenService.CreateToken(user);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId && x.IsActive)
            .Select(x => new CurrentUserDto(x.Id, x.Username, x.DisplayName, x.Role))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
