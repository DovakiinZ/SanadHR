namespace HR.Domain.Enums;

/// <summary>Account state of a login identity (User), distinct from the Employee HR record.
/// Active = can sign in; Suspended = disabled by an admin; Invited = created but not yet activated
/// (awaiting the user to set a password via an invitation link).</summary>
public enum UserStatus
{
    Active = 1,
    Suspended = 2,
    Invited = 3,
}
