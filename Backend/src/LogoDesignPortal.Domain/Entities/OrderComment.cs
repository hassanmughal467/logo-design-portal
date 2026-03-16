using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class OrderComment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public bool IsInternal { get; set; } = true; // Internal comments (Admin/Designer only), not visible to clients

    /// <summary>Type of comment for visibility and mediation control.</summary>
    public CommentType CommentType { get; set; } = CommentType.General;

    /// <summary>When true, Client can see this comment. Designer comments default false; Admin can approve.</summary>
    public bool VisibleToClient { get; set; }

    /// <summary>Client has read this comment.</summary>
    public bool IsReadByClient { get; set; }

    /// <summary>Designer has read this comment.</summary>
    public bool IsReadByDesigner { get; set; }

    /// <summary>Admin has read this comment.</summary>
    public bool IsReadByAdmin { get; set; }

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
