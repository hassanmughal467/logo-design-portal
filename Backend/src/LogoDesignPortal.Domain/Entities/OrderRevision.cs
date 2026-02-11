namespace LogoDesignPortal.Domain.Entities;

public class OrderRevision : BaseEntity
{
    public Guid OrderId { get; set; }
    public string Instructions { get; set; } = string.Empty; // Text instructions from client
    public Guid RequestedBy { get; set; } // Client who requested revision
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
    public ICollection<RevisionFile> Files { get; set; } = new List<RevisionFile>(); // Reference images, machine photos, etc.
}
