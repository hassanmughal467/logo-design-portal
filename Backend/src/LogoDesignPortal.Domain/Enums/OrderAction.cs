namespace LogoDesignPortal.Domain.Enums;

public enum OrderAction
{
    Created = 1,
    StatusChanged = 2,
    Cancelled = 3,
    Refunded = 4,
    Archived = 5,
    Unarchived = 6,
    PriceUpdated = 7,
    DesignerAssigned = 8,
    FileUploaded = 9,
    CommentAdded = 10
}
