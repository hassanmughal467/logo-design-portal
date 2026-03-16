# Logo Design Portal – Internal Analytics Data Map

## 1. Order Entity (LogoOrder)

**Location:** `Backend/src/LogoDesignPortal.Domain/Entities/LogoOrder.cs`  
**Table:** `LogoOrders`

| Property | Type | Analytics Use |
|----------|------|---------------|
| Id | Guid | Order identification |
| ClientId | Guid | Client analytics, revenue by client |
| DesignerId | Guid? | Designer performance, workload |
| Status | OrderStatus | Status distribution, funnel, workflow |
| Price | decimal | Revenue, AOV |
| CreatedAt | DateTime | Order trends, completion time |
| UpdatedAt | DateTime? | Completion timestamp when Status=Completed |
| Deadline | DateTime? | Overdue detection |

## 2. Order Statuses

| Value | Analytics Category |
|-------|-------------------|
| WaitingForAdminApproval, Pending | Awaiting Admin Review |
| InProgress, RevisionRequested | In Progress |
| PreviewDelivered | Awaiting Client Approval |
| Completed | Completed |
| Cancelled, CancelledByUser, CancelledByAdmin | Cancelled |

## 3. Key Metrics Sources

| Metric | Source |
|--------|--------|
| Order timestamps | LogoOrder.CreatedAt, UpdatedAt |
| Completion time | UpdatedAt ?? CreatedAt when Status=Completed |
| Designer IDs | LogoOrder.DesignerId |
| Client IDs | LogoOrder.ClientId |
| Revision count | OrderRevisions.Count per OrderId |
| Revenue | Sum(LogoOrder.Price) where Status=Completed |
| File uploads | LogoFiles.CreatedAt |
| Notifications | Notifications.CreatedAt, Type |

## 4. Supporting Entities

- **OrderRevision** – Revision count per order
- **LogoFile** – File upload analytics
- **Notification** – Notification activity by type
- **OrderStatusHistory** – Status transitions, approvals
- **OrderLog** – Activity tracking (FileUploaded, etc.)
