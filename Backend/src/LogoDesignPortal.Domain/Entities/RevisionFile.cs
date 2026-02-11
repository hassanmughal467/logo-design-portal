using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class RevisionFile : BaseEntity
{
    public Guid RevisionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public RevisionFileType FileType { get; set; } = RevisionFileType.ReferenceImage; // Reference image, Machine photo, Output photo
    public string? Description { get; set; }

    // Navigation properties
    public OrderRevision Revision { get; set; } = null!;
}
