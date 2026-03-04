namespace LogoDesignPortal.Application.DTOs.Files;

public class FileResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string? FileCategory { get; set; }
    public string FileStatus { get; set; } = string.Empty;
    public bool IsFinalVersion { get; set; }
    public bool IsVisibleToClient { get; set; }
    public bool IsAdminApproved { get; set; }
    public int VersionNumber { get; set; }
    public string? Description { get; set; }
    public Guid UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
