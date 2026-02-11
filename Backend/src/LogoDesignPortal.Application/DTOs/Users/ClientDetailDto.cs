using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Application.DTOs.AuditLogs;

namespace LogoDesignPortal.Application.DTOs.Users;

public class ClientDetailDto
{
    public UserResponseDto User { get; set; } = null!;
    public ClientProfileDto? ClientProfile { get; set; }
    public List<OrderResponseDto> OrderHistory { get; set; } = new();
    public List<InvoiceResponseDto> Invoices { get; set; } = new();
    public List<FileResponseDto> Files { get; set; } = new();
    public List<AuditLogResponseDto> ActivityTimeline { get; set; } = new();
}
