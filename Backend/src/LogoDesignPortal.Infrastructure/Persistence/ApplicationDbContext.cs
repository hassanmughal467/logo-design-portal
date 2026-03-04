using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<ClientProfile> ClientProfiles { get; set; }
    public DbSet<DesignerProfile> DesignerProfiles { get; set; }
    public DbSet<LogoOrder> LogoOrders { get; set; }
    public DbSet<LogoFile> LogoFiles { get; set; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
    public DbSet<OrderLog> OrderLogs { get; set; }
    public DbSet<OrderRevision> OrderRevisions { get; set; }
    public DbSet<RevisionFile> RevisionFiles { get; set; }
    public DbSet<OrderComment> OrderComments { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ClientGallery> ClientGalleries { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceOrder> InvoiceOrders { get; set; }
    public DbSet<InvoiceLog> InvoiceLogs { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
