namespace LogoDesignPortal.Domain.Entities;

public class LogoFile : BaseEntity
{
    public Guid OrderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsFinalVersion { get; set; } = false;
    public string? Description { get; set; }
    public Guid UploadedBy { get; set; }

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
}
