namespace LogoDesignPortal.Domain.Entities;

public class ClientGallery : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid OrderId { get; set; }
    public Guid FileId { get; set; } // Reference to LogoFile
    public string PreviewImagePath { get; set; } = string.Empty; // JPEG preview
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // Final format download path
    public string ContentType { get; set; } = string.Empty;
    public string? Format { get; set; } // PNG, SVG, PDF, etc.
    public DateTime ApprovedAt { get; set; } // When client approved

    // Navigation properties
    public ClientProfile Client { get; set; } = null!;
    public LogoOrder Order { get; set; } = null!;
    public LogoFile File { get; set; } = null!;
}
