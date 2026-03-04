using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Files;

public class UploadFileRequestDto
{
    [Required]
    public string FileType { get; set; } = string.Empty; // Reference, Preview, Final

    public string? Description { get; set; }
}
