using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Comments;

public class CreateCommentRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>Internal comments (Admin/Designer only). Clients cannot create internal comments.</summary>
    public bool IsInternal { get; set; } = true;
}
