using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public class SendPreviewBatchRequestDto
{
    [Required]
    public Guid PreviewBatchId { get; set; }
}
