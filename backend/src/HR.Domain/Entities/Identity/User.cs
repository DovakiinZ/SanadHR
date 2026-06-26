using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Modules.Identity.Entities;

public class User : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }

    /// <summary>Legacy sign-in gate kept in sync with <see cref="Status"/> (Active ⇒ true).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Richer account state: Active / Suspended / Invited.</summary>
    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    // Secure, single-use token for password reset / invitation acceptance. The raw token is emailed;
    // only its SHA-256 hash is stored, with an expiry. Purpose distinguishes "Reset" from "Invite".
    public string? ResetTokenHash { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }
    public string? ResetTokenPurpose { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
