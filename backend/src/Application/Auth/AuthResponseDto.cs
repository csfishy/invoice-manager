using InvoiceManager.Domain.Users;

namespace InvoiceManager.Application.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Username,
    string DisplayName,
    UserRole Role);
