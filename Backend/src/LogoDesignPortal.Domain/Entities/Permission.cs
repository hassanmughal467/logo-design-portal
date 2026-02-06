namespace LogoDesignPortal.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "CreateDesignerProfile", "AssignOrders"
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g., "DesignerProfile", "Order", "User"
    public string Action { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
