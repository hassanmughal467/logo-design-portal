using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Domain.Entities;

public class LogoFile : BaseEntity
{
    public Guid OrderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileType FileType { get; set; } = FileType.Reference; // Reference, Preview, Final
    public FileCategory? FileCategory { get; set; } // Source, Print, Web, Embroidery
    public FileStatus FileStatus { get; set; } = FileStatus.Draft; // Draft, Final, Approved
    public bool IsFinalVersion { get; set; } = false;
    public bool IsVisibleToClient { get; set; } = false; // Admin must approve before client sees
    public bool IsAdminApproved { get; set; } = false; // Admin approval flag
    public int VersionNumber { get; set; } = 1;
    public string? Description { get; set; }
    public Guid UploadedBy { get; set; }
    public Guid? ApprovedBy { get; set; } // Admin who approved
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Groups preview files from the same designer upload session.
    /// Admin must send entire batch - no partial preview delivery.
    /// </summary>
    public Guid? PreviewBatchId { get; set; }

    // Navigation properties
    public LogoOrder Order { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
}
