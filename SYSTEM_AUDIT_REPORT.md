# Logo Design Portal – Full System Audit Report

**Date:** March 11, 2025  
**Purpose:** Pre-production stability audit  
**Scope:** Backend (.NET API), Frontend (Angular), Database (EF Core)

---

## 1. System Architecture Overview

### 1.1 Backend Structure

| Layer | Components |
|-------|------------|
| **API** | 23 Controllers, JWT Auth, SignalR, Exception Middleware |
| **Application** | 24 Services, DTOs, AutoMapper, Helpers (OrderLockingHelper, OrderStatusStateMachine, RevisionLimitHelper) |
| **Domain** | Entities (LogoOrder, Invoice, DesignerInvoice, ClientProfile, etc.), Enums |
| **Infrastructure** | EF Core, MySQL, Migrations, Soft Delete Filters |

### 1.2 Frontend Structure

| Layer | Components |
|-------|------------|
| **Modules** | Auth, Dashboard, Orders, Invoices, Financial, Analytics, Clients, Designers, etc. |
| **Core** | ApiService, AuthService, BillingService, Error Interceptor, Token Interceptor |
| **Shared** | Order model, Comment model, Design-pricing model |

### 1.3 Key Data Flow

```
Order Created (Client) → Admin Assigns Designer → Designer Uploads Files →
Preview Delivered → Client Approves/Revisions → Admin Marks Completed →
BillingEligible=true → Client Invoice Created → Designer Invoice Created
```

---

## 2. Module-by-Module Workflow Analysis

### 2.1 Authentication & Authorization

| Aspect | Details |
|--------|---------|
| **Workflow** | Login → JWT + Refresh Token; Role-based (SuperAdmin, Admin, Client, Designer); Permission-based for some endpoints |
| **Backend** | AuthController, AuthService, JwtTokenService |
| **Validation** | Password reset, change password, forgot password |
| **Issues** | None critical identified |

---

### 2.2 Client Management

| Aspect | Details |
|--------|---------|
| **Workflow** | ClientProfile auto-created on first order; ClientLogoPricing per client/design-type |
| **Backend** | ClientLogoPricingService, ClientLogoPricingController |
| **Validation** | **MISSING:** No validation for negative `Price` in ClientLogoPricing Create/Update |
| **Issues** | ClientLogoPricing can store negative prices → order creation would apply invalid pricing |

---

### 2.3 Orders Module

| Aspect | Details |
|--------|---------|
| **Workflow** | Create → WaitingForAdminApproval → (PriceApprovalPending) → InProgress → PreviewDelivered → RevisionRequested/ClientApproved → Completed |
| **Backend** | OrderService, OrdersController |
| **Status Machine** | OrderStatusStateMachine enforces valid transitions |
| **Locking** | OrderLockingHelper locks Completed, Cancelled, Refunded orders |
| **Issues** | See Section 3 (Identified Bugs) |

---

### 2.4 File Upload System

| Aspect | Details |
|--------|---------|
| **Workflow** | Reference/Preview/Final uploads; Designer submits pricing with Final upload |
| **Backend** | FileService, FilesController |
| **Validation** | File type, size, magic bytes; DesignCategory/DesignType/ProposedPrice required for Final |
| **Issues** | None critical |

---

### 2.5 Designer Workflow

| Aspect | Details |
|--------|---------|
| **Workflow** | Assign → Upload Preview → Send to Client → Client Approve/Revision → Admin Approve Logo (Completed) |
| **Backend** | OrderService, RevisionService, FileService |
| **Issues** | None critical |

---

### 2.6 Revision System

| Aspect | Details |
|--------|---------|
| **Workflow** | Client requests revision (PreviewDelivered only); RevisionLimit enforced |
| **Backflow** | RevisionRequested → Designer uploads → PreviewDelivered |
| **Backend** | RevisionService |
| **Issues** | None critical |

---

### 2.7 Pricing System

| Aspect | Details |
|--------|---------|
| **Client Pricing** | ClientLogoPricing (per client, DesignCategory, DesignType) → ClientBasePrice, ClientChargePrice |
| **Designer Pricing** | DesignPricing (default PKR) → DesignerProposedPrice → DesignerApprovedPrice |
| **Price Fields** | Price (legacy), ClientPrice (deprecated), ClientBasePrice, ClientChargePrice, DesignerProposedPrice, DesignerApprovedPrice |
| **Issues** | **ProposedPrice overload:** Used for both CLIENT price approval and DESIGNER price; can overwrite each other (see Bug #2) |

---

### 2.8 Client Pricing (ClientLogoPricing)

| Aspect | Details |
|--------|---------|
| **Workflow** | SuperAdmin sets per-client pricing; applied at order creation |
| **Backend** | ClientLogoPricingService |
| **Validation** | **MISSING:** Negative price, null currency |
| **Issues** | See Bug #8 |

---

### 2.9 Designer Pricing (DesignerPayoutService)

| Aspect | Details |
|--------|---------|
| **Workflow** | Designer submits → Auto-approve if matches StandardPrice; else Admin approves |
| **Backend** | DesignerPayoutService |
| **Eligibility** | Status=Completed, PriceApproved, ApprovedPrice>0, !IsDesignerInvoiced |
| **Issues** | See Bug #1, #4 |

---

### 2.10 Billing Queue

| Aspect | Details |
|--------|---------|
| **Workflow** | Orders with Status=Completed, BillingEligible=true, IsInvoiced=false |
| **Backend** | BillingService, BillingController |
| **Issues** | **CRITICAL:** DesignerPayoutService sets BillingEligible=false when creating designer invoice → removes order from client billing queue (Bug #1) |

---

### 2.11 Invoice Generation (Client)

| Aspect | Details |
|--------|---------|
| **Workflow** | BillingService.CreateInvoiceFromOrdersAsync → InvoiceService.CreateInvoiceAsync |
| **Price Source** | ClientChargePrice > 0 ? ClientChargePrice : (ClientPrice ?? Price) |
| **Editable Price** | CreateInvoiceRequestDto.Orders allows admin to override price per line |
| **Issues** | No validation that editable price is reasonable; could create invoice with arbitrary amount |

---

### 2.12 Designer Invoice Generation

| Aspect | Details |
|--------|---------|
| **Workflow** | GenerateDesignerInvoiceAsync (by period) or GenerateInvoiceFromBuilderAsync (manual selection) |
| **Price Source** | DesignerApprovedPrice ?? ApprovedPrice (auto-generated); request.Amount (builder) |
| **Issues** | **Builder allows arbitrary Amount:** Admin can pass amount ≠ DesignerApprovedPrice (Bug #4) |

---

### 2.13 Notifications

| Aspect | Details |
|--------|---------|
| **Workflow** | NotificationService, SignalR RealtimeNotificationService |
| **Issues** | None critical |

---

### 2.14 Order Status Workflow

| Aspect | Details |
|--------|---------|
| **Transitions** | OrderStatusStateMachine enforces valid paths |
| **Completed Paths** | UpdateOrderStatusAsync(Completed), RevisionService.ApproveLogoAsync (Admin) |
| **BillingEligible** | Set in OrderService (UpdateOrderStatusAsync) and RevisionService (ApproveLogoAsync) |
| **Issues** | Consistent; no path skips required states |

---

### 2.15 Refunds / Cancel Orders

| Aspect | Details |
|--------|---------|
| **Refund** | Only Completed orders; RefundAmount ≤ ClientChargePrice |
| **Cancel** | Client: WaitingForAdminApproval/PriceApprovalPending only; Admin: any |
| **Issues** | None critical |

---

### 2.16 Admin Operations

| Aspect | Details |
|--------|---------|
| **Authorization** | RequirePermissionAttribute, Authorize(Roles) |
| **Endpoints** | Financial, Analytics, ClientAnalytics, Billing, DesignerPayout, Invoices |
| **Issues** | None critical |

---

## 3. Identified Bugs

### Bug #1 (CRITICAL) – BillingEligible Incorrectly Set to False on Designer Invoice

**Location:** `DesignerPayoutService.GenerateInvoiceFromBuilderAsync` (line 584)

**Issue:** When creating a designer invoice, the code sets `order.BillingEligible = false`. `BillingEligible` controls **client** invoicing, not designer invoicing. This removes completed orders from the client billing queue before they are client-invoiced.

**Impact:** Clients with Weekly/Monthly billing may never receive invoices for completed orders if the designer is invoiced first.

**Fix:** Remove the line `order.BillingEligible = false` from `GenerateInvoiceFromBuilderAsync`. Do not modify `BillingEligible` when creating designer invoices.

---

### Bug #2 (HIGH) – ProposedPrice Field Overload (Client vs Designer)

**Location:** `LogoOrder.ProposedPrice` used in both:
- `OrderService.RequestPriceApprovalAsync` (client price)
- `DesignerPayoutService` (designer payout)

**Issue:** Both flows write to `ProposedPrice`. If designer submits pricing first (`ProposedPrice` = designer payout in PKR), then admin calls `RequestPriceApprovalAsync` (client price in USD), the client approval flow uses `order.ProposedPrice ?? order.Price`. If designer submitted first, `ProposedPrice` could be designer payout (wrong currency/amount) when client approves.

**Impact:** Client could be charged designer payout amount (e.g., 5000 PKR) instead of client price (e.g., 100 USD).

**Fix:** Introduce `ClientProposedPrice` for client approval flow; keep `ProposedPrice`/`DesignerProposedPrice` for designer only. Or ensure strict ordering: client price approval must complete before designer pricing.

---

### Bug #3 (MEDIUM) – ApprovePriceAsync Uses ProposedPrice for Client Charge

**Location:** `OrderService.ApprovePriceAsync` (line 656)

**Issue:** `var approvedAmount = order.ProposedPrice ?? order.Price;` – If `ProposedPrice` was overwritten by designer flow, wrong amount is applied.

**Fix:** Use a dedicated client-proposed field (see Bug #2) or validate that `ProposedPrice` is in expected currency/range before applying.

---

### Bug #4 (MEDIUM) – Designer Invoice Builder Accepts Arbitrary Amounts

**Location:** `DesignerPayoutService.GenerateInvoiceFromBuilderAsync`

**Issue:** `request.Orders` contains `Amount` per order. The service uses `amount` from the request without validating it equals `DesignerApprovedPrice`. Admin could accidentally overpay or underpay designers.

**Impact:** Billing mistakes, audit trail mismatch.

**Fix:** Validate `amount` equals `order.DesignerApprovedPrice` (or allow explicit override with a separate flag and audit log).

---

### Bug #5 (LOW) – RevisionService.ApproveLogo Uses ProposedPrice for Notification

**Location:** `RevisionService.ApproveLogoAsync` (line 382)

**Issue:** Notification says "Designer proposed PKR {order.ProposedPrice}". If `ProposedPrice` was set by client approval flow, message could show wrong value.

**Fix:** Use `order.DesignerProposedPrice ?? order.ProposedPrice` for designer-specific notifications.

---

### Bug #6 (LOW) – InvoiceResponseDto.ClientId Semantics

**Location:** `InvoiceService.MapToInvoiceResponseDto`

**Issue:** `ClientId = invoice.Client.UserId` – returns User ID, not ClientProfile ID. Billing queue and other APIs use ClientProfile.Id. Naming can cause confusion.

**Fix:** Document clearly or add `ClientProfileId` if needed for consistency.

---

### Bug #7 (LOW) – Frontend Order.clientId

**Location:** `Frontend order.model.ts`

**Issue:** `Order` interface has `clientId: string` but `OrderResponseDto` does not expose `ClientId`. Frontend may expect it from `Client.Id` (ClientProfile.Id). Potential mismatch.

**Fix:** Ensure frontend uses `client?.id` (ClientProfile.Id) where needed; document mapping.

---

### Bug #8 (MEDIUM) – No Negative Price Validation in ClientLogoPricing

**Location:** `ClientLogoPricingService.CreateAsync`, `UpdateAsync`

**Issue:** No validation that `request.Price >= 0`. Negative prices would flow into order creation.

**Fix:** Add `if (request.Price < 0) throw new InvalidOperationException("Price cannot be negative.");`

---

## 4. Potential Production Risks

| Risk | Severity | Description |
|------|----------|-------------|
| Client never invoiced | **Critical** | Bug #1: Designer invoice creation removes orders from client billing queue |
| Wrong client charge | **High** | Bug #2/3: ProposedPrice confusion can charge wrong amount |
| Designer over/underpay | **Medium** | Bug #4: Builder allows arbitrary amounts |
| Negative pricing | **Medium** | Bug #8: ClientLogoPricing accepts negative prices |
| Currency mismatch | **Low** | Client (USD) vs Designer (PKR) in same ProposedPrice field |

---

## 5. Data Inconsistency Risks

| Risk | Description |
|------|-------------|
| InvoiceOrder.Amount vs Order.ClientChargePrice | CreateInvoiceRequestDto.Orders allows override; no reconciliation check |
| DesignerInvoiceItem.Amount vs Order.DesignerApprovedPrice | Builder uses request amount; can diverge |
| BillingEligible vs IsDesignerInvoiced | BillingEligible incorrectly set false on designer invoice; breaks client billing |
| HasInvoice vs InvoiceOrders | OrderService checks InvoiceOrders for HasInvoice; consistent |

---

## 6. API Mismatch Issues

| Backend | Frontend | Issue |
|---------|----------|-------|
| OrderResponseDto | Order | No `clientId` in DTO; frontend may expect it |
| InvoiceResponseDto.ClientId | Invoice.clientId | Backend returns UserId; frontend may expect ClientProfile.Id |
| OrderStatus (enum int) | OrderStatus (string) | Backend returns string via ToString(); should match |
| BillingType (enum) | billingType (number) | Frontend uses 1–4; backend enum values should align |

---

## 7. Performance Concerns

| Concern | Location | Recommendation |
|---------|----------|----------------|
| N+1 on order lists | OrderService.GetOrdersByClientAsync | Mitigated by ApplyFileCountsAsync batch load |
| Include chains | OrderService multiple Includes | Consider split queries for large result sets |
| Invoice status update | GetInvoicesAsync calls UpdateInvoiceStatusIfNeededAsync per invoice | Could batch overdue updates |
| Missing index | LogoOrders | Index on (ClientId, BillingEligible, IsInvoiced) exists |

---

## 8. Security Issues

| Issue | Severity | Description |
|-------|----------|-------------|
| Client modifying price | Mitigated | UpdateOrderAsync does not update Price |
| Designer modifying client data | Mitigated | Designer cannot access client identity |
| Admin endpoints | OK | Authorize(Roles) and RequirePermission used |
| SuperAdmin bypass | By design | SuperAdmin bypasses permission checks |

---

## 9. Order Lifecycle Consistency

**Validated flow:**

1. Order Created → WaitingForAdminApproval  
2. Designer Assigned → InProgress (if was WaitingForAdminApproval)  
3. Files Uploaded → (no status change; PreviewDelivered when sent)  
4. Send to Client → PreviewDelivered  
5. Client Approve → ClientApproved  
6. Admin Mark Completed → Completed, BillingEligible=true, CompletedDate set  
7. BillingEligible → Orders appear in billing queue  
8. Invoice Created → IsInvoiced=true  

**No path skips required states.** State machine enforces transitions.

---

## 10. Pricing Consistency Summary

| Source | Field | Used For |
|--------|-------|----------|
| ClientLogoPricing | Price | ClientBasePrice at order creation |
| LogoOrder | ClientBasePrice | Never changes |
| LogoOrder | ClientChargePrice | Client invoicing (primary) |
| LogoOrder | ClientPrice, Price | Fallbacks; legacy |
| LogoOrder | DesignerApprovedPrice | Designer invoicing |
| InvoiceService | ClientChargePrice > 0 ? ClientChargePrice : (ClientPrice ?? Price) | Invoice amount |
| BillingService | Same fallback | Queue total |
| DesignerPayoutService | DesignerApprovedPrice ?? ApprovedPrice | Designer invoice (auto); request.Amount (builder) |

**Mismatch risk:** ProposedPrice overload (Bug #2); Builder amount (Bug #4).

---

## 11. Recommended Fixes (Do Not Implement Yet)

1. **Bug #1:** Remove `order.BillingEligible = false` from `DesignerPayoutService.GenerateInvoiceFromBuilderAsync`.
2. **Bug #2/3:** Introduce `ClientProposedPrice` or enforce ordering so client price approval completes before designer pricing.
3. **Bug #4:** Validate builder `Amount` against `DesignerApprovedPrice` or require explicit override with audit.
4. **Bug #5:** Use `DesignerProposedPrice` for designer notifications in RevisionService.
5. **Bug #8:** Add negative price validation in ClientLogoPricingService.
6. **Documentation:** Clarify ClientId (UserId vs ClientProfile.Id) in API contracts.
7. **Data migration:** If Bug #1 has already occurred, run script to set `BillingEligible=true` for orders where `IsInvoiced=false` and `Status=Completed` and `IsDesignerInvoiced=true`.

---

## 12. Summary

| Category | Count |
|----------|-------|
| Critical Bugs | 1 |
| High Bugs | 1 |
| Medium Bugs | 4 |
| Low Bugs | 3 |
| Security Issues | 0 critical |
| Performance Concerns | Minor |

**Recommendation:** Address Bug #1 before production. Address Bugs #2, #3, #4 before handling real client/designer payments. Address Bug #8 as a quick validation improvement.
