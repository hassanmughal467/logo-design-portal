# Pricing and Invoicing Analysis Report

**Date:** March 10, 2025  
**Scope:** Order price storage, pricing calculation, invoice generation, and client pricing across Backend (.NET) and Frontend (Angular).

---

## 1. WHERE ORDER PRICE IS CURRENTLY STORED

### Backend — LogoOrder Entity
**File:** `Backend/src/LogoDesignPortal.Domain/Entities/LogoOrder.cs`

| Field | Type | Purpose |
|-------|------|---------|
| `Price` | `decimal` | Main order price (client-facing when no ClientPrice). Used for refund validation, BillingService totals. |
| `ClientPrice` | `decimal?` | Client-facing price from ClientLogoPricing or default. **Used for invoicing** when available. |
| `CurrencyCode` | `string?` | Currency for client price (USD, PKR, EUR, etc.) |
| `StandardPrice` | `decimal?` | Default payout price (PKR) from DesignPricing. Set when designer submits pricing. |
| `ProposedPrice` | `decimal?` | Designer's proposed payout (PKR). Set when uploading final files. |
| `ApprovedPrice` | `decimal?` | Admin-approved designer payout (PKR). Final for designer invoices. |

### Backend — Related Entities
- **Invoice** (`Invoice.cs`): `Amount`, `TaxAmount`, `TotalAmount` — totals for client invoices
- **InvoiceOrder** (`InvoiceOrder.cs`): `Amount` — per-order line item amount (from `ClientPrice ?? Price`)
- **DesignerInvoiceItem** (`DesignerInvoiceItem.cs`): `Amount` — designer payout per order (from `ApprovedPrice`)
- **ClientLogoPricing** (`ClientLogoPricing.cs`): `Price`, `CurrencyCode` — client-specific pricing by DesignCategory/DesignType
- **DesignPricing** (`DesignPricing.cs`): `DefaultPrice` — default designer payout (PKR) by DesignCategory/DesignType

### Frontend — Order Model
**File:** `Frontend/src/app/shared/models/order.model.ts`

- `price: number` — maps to backend `Price`
- `OrderResponseDto` also exposes `clientPrice`, `currencyCode`, `standardPrice`, `proposedPrice`, `approvedPrice` (via API response)

---

## 2. WHERE PRICING IS CALCULATED

### A. Client Order Pricing (Order Creation)

| Location | File | Logic |
|----------|------|-------|
| **OrderService.ApplyClientPricingAsync** | `OrderService.cs` (lines 247–285) | 1) If DesignCategory/DesignType provided: lookup `ClientLogoPricing` via `ClientLogoPricingService.GetClientPricingForOrderAsync`. If found: set `ClientPrice`, `Price`, `CurrencyCode`. 2) If not found: fallback to `DesignPricingHelper.GetDefaultPrice(designType)` (USD defaults). 3) For ComplexVector: keep request `Price`. |
| **ClientLogoPricingService.GetClientPricingForOrderAsync** | `ClientLogoPricingService.cs` (lines 134–145) | Returns `(Price, CurrencyCode)` from `ClientLogoPricing` by ClientId, DesignCategory, DesignType. |
| **DesignPricingHelper.GetDefaultPrice** | `DesignPricingHelper.cs` | Static defaults: LeftChest=350, JacketBack=700, SimpleVector=350 (PKR), ComplexVector=null. |

**Flow:** `CreateOrderAsync` / `CreateOrderWithFilesAsync` → `ApplyClientPricingAsync` → `ClientLogoPricingService` or `DesignPricingHelper`.

### B. Designer Payout Pricing

| Location | File | Logic |
|----------|------|-------|
| **DesignerPayoutService.SubmitDesignerPricingAsync** | `DesignerPayoutService.cs` (lines 52–123) | Designer submits `ProposedPrice`. `StandardPrice` from `DesignPricing` table. If `ProposedPrice != StandardPrice` or ComplexVector: `RequiresPriceApproval=true`, notify Admin. |
| **DesignerPayoutService.ApproveDesignerPriceAsync** | `DesignerPayoutService.cs` (lines 134–179) | Admin approves/modifies/rejects. Sets `ApprovedPrice`, `PriceApproved`. |
| **DesignerPayoutService.GetStandardPriceFromTableAsync** | `DesignerPayoutService.cs` (lines 125–132) | Reads `DesignPricing.DefaultPrice` by DesignCategory/DesignType. |
| **DesignerPayoutService.GetDesignPricingInfoAsync** | `DesignerPayoutService.cs` (lines 23–50) | Returns default prices for designer UI. Fallback hardcoded if table empty. |

### C. Admin Price Approval (Client-Facing)

| Location | File | Logic |
|----------|------|-------|
| **OrderService.RequestPriceApprovalAsync** | `OrderService.cs` (lines 619–660) | Admin sets `ProposedPrice`, status → `PriceApprovalPending`. Client must approve. |
| **OrderService.ApprovePriceAsync** | `OrderService.cs` (lines 662–711) | Client approves → `Price = ProposedPrice`. Client rejects → remains `PriceApprovalPending`. |

### D. Billing Queue / Eligible Order Amounts

| Location | File | Logic |
|----------|------|-------|
| **BillingService.GetBillingQueueOverviewAsync** | `BillingService.cs` (lines 22–65) | Uses `o.Price` for `TotalPendingAmount` (not `ClientPrice`). |
| **BillingService.GetEligibleOrdersForClientAsync** | `BillingService.cs` (lines 67–86) | Uses `o.Price` for `BillingEligibleOrderDto.Price` (not `ClientPrice`). |

**Note:** Invoice creation uses `ClientPrice ?? Price` (see InvoiceService below). Billing queue/eligible orders use `Price` only — potential inconsistency if `ClientPrice` differs.

---

## 3. WHERE INVOICES ARE GENERATED

### A. Client Invoices (Client-Facing)

| Location | File | Logic |
|----------|------|-------|
| **InvoiceService.CreateInvoiceAsync** | `InvoiceService.cs` (lines 27–246) | Creates `Invoice` and `InvoiceOrder` items. **Line item amount:** `order.ClientPrice ?? order.Price` (line 136). Marks orders `IsInvoiced=true`, `InvoiceId=invoice.Id`. |
| **BillingService.CreateInvoiceFromOrdersAsync** | `BillingService.cs` (lines 88–114) | Validates orders (Completed, BillingEligible, not IsInvoiced). Calls `InvoiceService.CreateInvoiceAsync`. |
| **BillingService.ProcessAutomaticInvoicingAsync** | `BillingService.cs` (lines 116–202) | **Weekly:** Mondays, invoices previous week for `BillingType.Weekly` clients. **Monthly:** 1st of month, invoices previous month for `BillingType.Monthly` clients. Uses `CreateInvoiceFromOrdersAsync`. |
| **BillingAutoInvoiceService** | `BillingAutoInvoiceService.cs` | Background service. Runs daily, calls `BillingService.ProcessAutomaticInvoicingAsync` on Mondays and 1st of month. |

### B. Designer Invoices (Designer Payout)

| Location | File | Logic |
|----------|------|-------|
| **DesignerPayoutService.GenerateDesignerInvoiceAsync** | `DesignerPayoutService.cs` (lines 208–301) | Creates `DesignerInvoice` for designer in given month. Items use `order.ApprovedPrice`. Sets `IsDesignerInvoiced=true`, `DesignerInvoiceId`, `BillingEligible=false`. |
| **DesignerPayoutService.GenerateInvoiceFromBuilderAsync** | `DesignerPayoutService.cs` (lines 347–416) | Admin selects orders and amounts. Creates designer invoice with custom amounts. Same order updates. |

### C. Invoice Creation Triggers

- **Manual:** Admin uses Billing UI → `BillingService.CreateInvoiceFromOrdersAsync` → `InvoiceService.CreateInvoiceAsync`
- **Auto:** `BillingAutoInvoiceService` (background) → `BillingService.ProcessAutomaticInvoicingAsync` for Weekly/Monthly clients

---

## 4. WHERE CLIENT PRICING EXISTS

### Backend

| Component | File | Purpose |
|-----------|------|---------|
| **ClientLogoPricing entity** | `Domain/Entities/ClientLogoPricing.cs` | ClientId, DesignCategory, DesignType, Price, CurrencyCode, IsActive |
| **ClientLogoPricingService** | `Application/Services/ClientLogoPricingService.cs` | CRUD, `GetClientPricingForOrderAsync` for order creation |
| **ClientLogoPricingController** | `API/Controllers/ClientLogoPricingController.cs` | REST API for client pricing management |
| **OrderService.ApplyClientPricingAsync** | `OrderService.cs` | Applies ClientLogoPricing when creating orders |

### Frontend

| Component | File | Purpose |
|-----------|------|---------|
| **ClientPricingManagementComponent** | `client-pricing/client-pricing-management/client-pricing-management.component.ts` | Admin UI: select client, add/edit/delete ClientLogoPricing by DesignCategory/DesignType |
| **Client pricing module** | `client-pricing/client-pricing.module.ts`, `client-pricing-routing.module.ts` | Routes and module setup |
| **Design-pricing model** | `shared/models/design-pricing.model.ts` | DesignCategory, DesignType enums for UI |

### API Endpoints (Client Logo Pricing)

- `GET /api/client-logo-pricing/client/{clientId}` — list pricings for client
- `POST /api/client-logo-pricing` — create
- `PUT /api/client-logo-pricing/{id}` — update
- `DELETE /api/client-logo-pricing/{id}` — delete (soft)

---

## 5. MODULE INVENTORY

### Backend (.NET)

| Module | Path | Role |
|--------|------|------|
| **OrdersController** | `API/Controllers/OrdersController.cs` | Order CRUD, assign, status, price approval, cancel, archive, refund |
| **OrderService** | `Application/Services/OrderService.cs` | Order logic, `ApplyClientPricingAsync`, price approval |
| **ClientLogoPricingService** | `Application/Services/ClientLogoPricingService.cs` | Client-specific pricing lookup |
| **DesignerPayoutService** | `Application/Services/DesignerPayoutService.cs` | Designer pricing, designer invoices |
| **InvoiceService** | `Application/Services/InvoiceService.cs` | Client invoice CRUD, uses `ClientPrice ?? Price` |
| **BillingService** | `Application/Services/BillingService.cs` | Billing queue, auto-invoicing, uses `Price` for totals |
| **DesignPricing** | `Domain/Entities/DesignPricing.cs` | Default designer payout (PKR) by category/type |
| **ClientLogoPricing** | `Domain/Entities/ClientLogoPricing.cs` | Client-specific pricing by category/type |
| **LogoOrder** | `Domain/Entities/LogoOrder.cs` | Price, ClientPrice, StandardPrice, ProposedPrice, ApprovedPrice |
| **DesignPricingHelper** | `Application/Helpers/DesignPricingHelper.cs` | Static default prices (fallback) |

### Frontend (Angular)

| Module | Path | Role |
|--------|------|------|
| **order-create** | `orders/order-create/order-create.component.ts` | Form: title, description, price (hidden), designCategory, designType. Sends to `POST orders` or `POST orders/with-files`. Price set by backend. |
| **order-detail** | `orders/order-detail/order-detail.component.ts` | Displays order, price, designer price approval, refund. Uses `order.price`, `order.proposedPrice`, etc. |
| **client-detail** | `clients/client-detail/client-detail.component.ts` | Client analytics, orders, invoices. Uses `(order as any).price` for spend calculations. |
| **invoice-list** | `invoices/invoice-list/invoice-list.component.ts` | Invoice list, generate from billing queue, pay. Uses `BillingService`. |
| **BillingService** | `core/services/billing.service.ts` | `getBillingQueue()`, `getEligibleOrders()`, `createInvoiceFromOrders()` |
| **client-pricing-management** | `client-pricing/client-pricing-management/` | Admin CRUD for ClientLogoPricing |

---

## 6. OBSERVATIONS AND EDGE CASES

1. **BillingEligible and CompletedDate**  
   - Set only in `RevisionService.ApproveLogo` when Admin/SuperAdmin approves (direct to Completed).  
   - **Not set** when Admin marks `ClientApproved` → `Completed` via `UpdateOrderStatus`. Orders completed via "Mark as Completed" may not become billing eligible.

2. **Price vs ClientPrice**  
   - Invoice items use `ClientPrice ?? Price`.  
   - Billing queue and `BillingEligibleOrderDto` use `Price` only. If `ClientPrice` differs (e.g. multi-currency), queue totals may not match invoice amounts.

3. **DesignPricing vs DesignPricingHelper**  
   - `DesignPricing` table is primary for designer defaults.  
   - `DesignPricingHelper` is fallback when table empty or for client order creation (DesignType-based defaults in USD).

4. **order-create price field**  
   - Form has `price: [0]` (hidden). Backend ignores it when `ApplyClientPricingAsync` runs; client cannot override pricing.

---

## 7. DATA FLOW SUMMARY

```
Order Creation:
  Client submits (designCategory, designType) 
    → OrderService.ApplyClientPricingAsync 
    → ClientLogoPricingService OR DesignPricingHelper 
    → LogoOrder.Price, ClientPrice, CurrencyCode

Designer Payout:
  Designer submits ProposedPrice 
    → DesignerPayoutService 
    → DesignPricing (StandardPrice), ApprovedPrice (admin)
    → DesignerInvoiceItem.Amount = ApprovedPrice

Client Invoice:
  Admin selects orders 
    → InvoiceService.CreateInvoiceAsync 
    → InvoiceOrder.Amount = ClientPrice ?? Price 
    → LogoOrder.IsInvoiced, InvoiceId
```

---

*End of report. No code changes were made.*
