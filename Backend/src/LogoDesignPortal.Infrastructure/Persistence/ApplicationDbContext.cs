using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
    public DbSet<DesignerInvoice> DesignerInvoices { get; set; }
    public DbSet<DesignerInvoiceItem> DesignerInvoiceItems { get; set; }
    public DbSet<DesignerInvoiceAdjustment> DesignerInvoiceAdjustments { get; set; }
    public DbSet<InvoiceLog> InvoiceLogs { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<DesignPricing> DesignPricings { get; set; } = null!;
    public DbSet<ClientLogoPricing> ClientLogoPricings { get; set; } = null!;
    public DbSet<DesignerLogoPricing> DesignerLogoPricings { get; set; } = null!;
    public DbSet<Quote> Quotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft deletes
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LogoOrder>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LogoFile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ClientProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DesignerProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrderComment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Settings>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ClientGallery>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DesignerLogoPricing>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Quote>().HasQueryFilter(e => !e.IsDeleted);

        // Additional indexes for performance
        modelBuilder.Entity<LogoFile>().HasIndex(e => e.OrderId);
        modelBuilder.Entity<Notification>().HasIndex(e => new { e.UserId, e.IsRead });
        modelBuilder.Entity<User>().HasIndex(e => e.Email);
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

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
