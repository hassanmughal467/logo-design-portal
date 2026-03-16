namespace LogoDesignPortal.Domain.Enums;

/// <summary>
/// Type of order comment for visibility and mediation control.
/// </summary>
public enum CommentType
{
    /// <summary>General comment visible to all parties (Client, Admin, Designer).</summary>
    General = 1,
    /// <summary>Internal comment visible only to Admin and Designer.</summary>
    Internal = 2,
    /// <summary>Designer feedback; requires Admin approval to be visible to Client.</summary>
    DesignerFeedback = 3,
    /// <summary>Admin relay/mediation message to Client.</summary>
    AdminRelay = 4,
}
