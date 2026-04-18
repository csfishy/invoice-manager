using InvoiceManager.Domain.Users;

namespace InvoiceManager.Application.Auth;

public sealed record CurrentUserDto(
    Guid UserId,
    string Username,
    string DisplayName,
    UserRole Role);
