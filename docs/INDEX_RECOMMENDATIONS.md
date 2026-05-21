# Index Recommendations (Week 3)

## Shipped in `AddWeek3QueryPerformanceIndexes`

| Table | Index | Use case |
|-------|-------|----------|
| `LogoOrders` | `CompletedDate` | Billing queue date filters |
| `LogoOrders` | `Deadline` | Overdue analytics |
| `LogoOrders` | `(Status, BillingEligible, IsInvoiced)` | Invoice generation / billing |
| `LogoFiles` | `(OrderId, IsDeleted)` | `ApplyFileCountsAsync`, file lists |
| `LogoFiles` | `(OrderId, IsVisibleToClient, FileType)` | Client file authorization |
| `Invoices` | `DueDate` | Overdue maintenance |
| `Invoices` | `(Status, DueDate)` | Overdue batch queries |

## Already present (do not duplicate)

- `LogoOrders`: `(Status, UpdatedAt)`, `(ClientId, BillingEligible, IsInvoiced)`, status/created analytics composites
- `Invoices`: `Status`, `CreatedAt`, `(ClientId, Status)`
- Row versions on hot entities (`ScalabilityRowVersionAndIndexes`)

## Future (evaluate after staging EXPLAIN)

| Table | Index | Driver |
|-------|-------|--------|
| `Messages` | `(OrderId, CreatedAt)` | Order message threads |
| `OrderRevisions` | `(OrderId, IsDeleted, CreatedAt)` | Latest revision, analytics |
| `OrderComments` | `(OrderId, IsDeleted, CreatedAt)` | Comment threads |

## Maintenance

- Consolidate `LogoFile.OrderId` index from `ApplicationDbContext.OnModelCreating` into `LogoFileConfiguration` (source of truth).
- Review unused indexes annually via MySQL `sys.schema_unused_indexes`.
