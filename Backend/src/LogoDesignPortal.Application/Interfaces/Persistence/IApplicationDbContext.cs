using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<ClientProfile> ClientProfiles { get; }
    DbSet<DesignerProfile> DesignerProfiles { get; }
    DbSet<LogoOrder> LogoOrders { get; }
    DbSet<LogoFile> LogoFiles { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<OrderLog> OrderLogs { get; }
    DbSet<OrderRevision> OrderRevisions { get; }
    DbSet<RevisionFile> RevisionFiles { get; }
    DbSet<OrderComment> OrderComments { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<ClientGallery> ClientGalleries { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceOrder> InvoiceOrders { get; }
    DbSet<InvoiceLog> InvoiceLogs { get; }
    DbSet<Message> Messages { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Settings> Settings { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
