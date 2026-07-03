namespace LogoDesignPortal.Application.Helpers;

/// <summary>
/// Helper for revision limits per package (Fiverr/Upwork style).
/// Basic &lt; 200: 2 revisions, Standard &lt; 500: 4 revisions, Premium 500+: unlimited.
/// </summary>
public static class RevisionLimitHelper
{
    /// <summary>
    /// Gets the revision limit for an order based on its price.
    /// Basic (&lt; 200): 2, Standard (&lt; 500): 4, Premium (≥ 500): unlimited (null).
    /// </summary>
    public static int? GetRevisionLimitFromPrice(decimal price)
    {
        if (price >= 500)
        {
            return null; // Premium/Custom: unlimited
        }

        if (price >= 200)
        {
            return 4;   // Standard: 4 revisions
        }

        return 2;                      // Basic: 2 revisions
    }

    /// <summary>
    /// Checks if the client can request another revision.
    /// Returns true if allowed, false if limit exceeded (unless AllowExtraRevisions).
    /// </summary>
    public static bool CanRequestRevision(int revisionCount, int? revisionLimit, bool allowExtraRevisions)
    {
        if (allowExtraRevisions)
        {
            return true;
        }

        if (revisionLimit == null)
        {
            return true; // Unlimited
        }

        return revisionCount < revisionLimit.Value;
    }
}
