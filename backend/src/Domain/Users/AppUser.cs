namespace InvoiceManager.Domain.Users;

public sealed class AppUser
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAtUtc { get; set; }
}
