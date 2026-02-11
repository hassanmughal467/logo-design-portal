using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Comments;

public class CreateCommentRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = true; // Internal comments (Admin/Designer only)
}
