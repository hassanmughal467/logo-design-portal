using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class Message : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    /// <summary>When true, message is in admin queue (Client/Designer → Admin). RecipientId may be null.</summary>
    public bool RequiresAdminApproval { get; set; }
    /// <summary>When true, this message was forwarded by Admin to Client or Designer.</summary>
    public bool ForwardedByAdmin { get; set; }
    /// <summary>Original sender role for relay tracking.</summary>
    public MessageSenderRole? OriginalSenderRole { get; set; }
    /// <summary>When admin forwards, links to the new forwarded message.</summary>
    public Guid? ForwardedToMessageId { get; set; }
    /// <summary>When admin rejects, marks as rejected.</summary>
    public bool IsRejected { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedBy { get; set; }

    // Navigation properties
    public User Sender { get; set; } = null!;
    public User? Recipient { get; set; }
    public LogoOrder? Order { get; set; }
}
