using LogoDesignPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<ClientProfile> ClientProfiles { get; }
    DbSet<DesignerProfile> DesignerProfiles { get; }
    DbSet<LogoOrder> LogoOrders { get; }
    DbSet<LogoFile> LogoFiles { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Invoice> Invoices { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
