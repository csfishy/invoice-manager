using InvoiceManager.Application.Auth;

namespace InvoiceManager.Application.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
