using InvoiceManager.Application.Auth;
using InvoiceManager.Domain.Users;

namespace InvoiceManager.Infrastructure.Auth;

public interface IJwtTokenService
{
    AuthResponseDto CreateToken(AppUser user);
}
