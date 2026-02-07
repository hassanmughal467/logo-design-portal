namespace LogoDesignPortal.Domain.Entities;

public class Message : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid SenderId { get; set; }
    public Guid? RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public User Sender { get; set; } = null!;
    public User? Recipient { get; set; }
    public LogoOrder? Order { get; set; }
}
