namespace LogoDesignPortal.Application.DTOs.Gallery;

public class GalleryItemResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderTitle { get; set; } = string.Empty;
    public string PreviewImagePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? Format { get; set; }
    public DateTime ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
