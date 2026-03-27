using System.ComponentModel.DataAnnotations;

namespace LogoDesignPortal.Application.DTOs.Orders;

public enum ClientPriceApprovalAction
{
    Approve,
    Modify,
    Reject
}

public class RespondPriceApprovalDto
{
    [Required]
    public ClientPriceApprovalAction Action { get; set; }

    /// <summary>Required when Action=Modify. Client counter-offer price.</summary>
    public decimal? CounterPrice { get; set; }

    /// <summary>Optional message from client (reason, notes, counter-offer context).</summary>
    public string? Message { get; set; }
}

