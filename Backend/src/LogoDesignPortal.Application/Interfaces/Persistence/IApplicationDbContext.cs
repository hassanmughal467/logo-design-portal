using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
    DbSet<DesignerInvoice> DesignerInvoices { get; }
    DbSet<DesignerInvoiceItem> DesignerInvoiceItems { get; }
    DbSet<DesignerInvoiceAdjustment> DesignerInvoiceAdjustments { get; }
    DbSet<InvoiceLog> InvoiceLogs { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Message> Messages { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Settings> Settings { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<DesignPricing> DesignPricings { get; }
    DbSet<ClientLogoPricing> ClientLogoPricings { get; }
    DbSet<DesignerLogoPricing> DesignerLogoPricings { get; }
    DbSet<Quote> Quotes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the given action inside a database transaction, wrapped in the execution strategy.
    /// Required for MySQL with retry-on-failure: user-initiated transactions must run inside CreateExecutionStrategy().
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Average completion interval (days) per designer for completed orders. Implemented with SQL on MySQL;
    /// falls back to an in-memory aggregate for SQLite / InMemory providers.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, double>> GetDesignerAverageCompletionDaysByDesignerAsync(CancellationToken cancellationToken = default);
}
