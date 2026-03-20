namespace LogoDesignPortal.Domain.Constants;

/// <summary>
/// Fixed IDs for standard roles (seeded at startup). Matches historical migrations and scripts.
/// </summary>
public static class SeededRoleIds
{
    public static readonly Guid SuperAdmin = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Admin = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Designer = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Client = Guid.Parse("44444444-4444-4444-4444-444444444444");
}
