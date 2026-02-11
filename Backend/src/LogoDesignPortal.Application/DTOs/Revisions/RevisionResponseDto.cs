namespace LogoDesignPortal.Application.DTOs.Revisions;

public class RevisionResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RevisionFileDto> Files { get; set; } = new();
}

public class RevisionFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
