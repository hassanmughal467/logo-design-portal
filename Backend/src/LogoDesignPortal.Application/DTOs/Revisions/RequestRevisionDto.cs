using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LogoDesignPortal.Application.DTOs.Revisions;

public class RequestRevisionDto
{
    [Required]
    [MinLength(10)]
    [MaxLength(5000)]
    public string Instructions { get; set; } = string.Empty;
    
    public IFormFile[]? Files { get; set; }
}
