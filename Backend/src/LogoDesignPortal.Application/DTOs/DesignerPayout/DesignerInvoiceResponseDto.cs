namespace LogoDesignPortal.Application.DTOs.DesignerPayout;

public class DesignerInvoiceResponseDto
{
    public Guid Id { get; set; }
    public Guid DesignerId { get; set; }
    public string DesignerName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
    public List<DesignerInvoiceItemDto> Items { get; set; } = new();
    public List<DesignerInvoiceAdjustmentDto> Adjustments { get; set; } = new();
}

public class DesignerInvoiceItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderTitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    /// <summary>Design category from order, for display in invoice detail.</summary>
    public string? DesignCategory { get; set; }
    /// <summary>Design type from order, for display in invoice detail.</summary>
    public string? DesignType { get; set; }
}

public class DesignerInvoiceAdjustmentDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
