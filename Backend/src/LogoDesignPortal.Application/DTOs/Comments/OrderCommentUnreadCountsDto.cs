namespace LogoDesignPortal.Application.DTOs.Comments;

/// <summary>
/// Unread counts for notification badges on Files, Revisions, Comments tabs.
/// </summary>
public class OrderCommentUnreadCountsDto
{
    /// <summary>Count of files not yet viewed by current user (placeholder for future).</summary>
    public int UnreadFiles { get; set; }
    /// <summary>Count of revisions not yet viewed by current user (placeholder for future).</summary>
    public int UnreadRevisions { get; set; }
    /// <summary>Count of comments not yet read by current user (Client/Designer/Admin).</summary>
    public int UnreadComments { get; set; }
}
