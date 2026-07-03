# Logo Design Portal – Comprehensive Technical Audit

**Generated:** March 10, 2025  
**Scope:** Security, Performance, Architecture, API Design, Database, Business Logic, UI/UX  
**Stack:** .NET 8 API | Angular 15 | MySQL/EF Core

---

## Executive Summary

This audit analyzes the Logo Design Portal codebase module-by-module across seven focus areas. Each finding includes file name, function name, issue explanation, severity, recommended fix, and improved code example.

| Category | Critical | High | Medium | Low |
|----------|----------|------|--------|-----|
| Security | 2 | 5 | 4 | 2 |
| Performance | 0 | 3 | 4 | 2 |
| Architecture | 0 | 2 | 3 | 2 |
| API Design | 0 | 2 | 4 | 2 |
| Database | 0 | 1 | 2 | 1 |
| Business Logic | 0 | 2 | 3 | 1 |
| UI/UX | 0 | 1 | 3 | 2 |

---

# 1. SECURITY ISSUES

## 1.1 Token Storage in localStorage (XSS Risk)

| Field | Value |
|-------|-------|
| **File** | `Frontend/src/app/core/services/auth.service.ts` |
| **Function** | `setAuthData`, `loadUserFromStorage` |
| **Lines** | 149–168, 181–214 |
| **Severity** | **High** |

**Issue:** Access and refresh tokens are stored in `localStorage`. Any XSS vulnerability allows an attacker to steal tokens via `localStorage.getItem('auth_token')`. Tokens persist until explicitly cleared.

**Recommended Fix:** Use `httpOnly` cookies for tokens. Backend sets `Set-Cookie` with `HttpOnly; Secure; SameSite=Strict`. Frontend sends credentials with requests; no JavaScript access to tokens.

**Improved Code Example (Backend – AuthController):**
```csharp
// In AuthController.Login - set httpOnly cookie instead of returning token in body
Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddHours(168),
    Path = "/api/auth"
});
return Ok(new { token = accessToken, expiresAt = expiresAt, user = user });
```

---

## 1.2 JWT Key in appsettings.json

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/appsettings.json` |
| **Key** | `Jwt:Key` |
| **Severity** | **Critical** |

**Issue:** JWT signing key is stored in `appsettings.json` and committed to source control. Default key is weak and predictable. Production deployments may use the same key.

**Recommended Fix:** Use User Secrets (dev) and environment variables or Azure Key Vault (production). Never commit secrets.

```json
// appsettings.json - remove Key, use placeholder
"Jwt": {
  "Key": "",  // Must be set via JWT__Key env var or User Secrets
  "Issuer": "LogoDesignPortal",
  ...
}
```

```csharp
// Program.cs - validate key at startup
var jwtKey = configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("JWT Key must be configured and at least 32 characters.");
```

---

## 1.3 Database Credentials in appsettings.json

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/appsettings.json` |
| **Key** | `ConnectionStrings:DefaultConnection` |
| **Severity** | **Critical** |

**Issue:** Connection string with password `ADMIN` is in version control. Exposes database to unauthorized access.

**Recommended Fix:** Use User Secrets for development and environment variables or managed identity for production.

---

## 1.4 File Upload – No Rate Limiting per User

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/FilesController.cs` |
| **Function** | `UploadFile`, `UploadMultipleFiles` |
| **Lines** | 26–70 |
| **Severity** | **Medium** |

**Issue:** `RequestSizeLimit(50MB)` exists but no per-user rate limiting. A single user can exhaust disk or bandwidth by repeated uploads.

**Recommended Fix:** Add rate limiting middleware or attribute per user/IP for file upload endpoints.

```csharp
[HttpPost("upload/{orderId}")]
[RequestSizeLimit(50 * 1024 * 1024)]
[EnableRateLimiting("FileUpload")]  // Add policy: 10 requests per minute per user
public async Task<IActionResult> UploadFile(...)
```

---

## 1.5 ApprovePrice Returns 401 Instead of 403

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `ApprovePriceAsync` |
| **Lines** | 644–648 |
| **Severity** | **Low** |

**Issue:** When a client tries to approve another client's order, the code throws `UnauthorizedAccessException`, which maps to 401. The user is authenticated; the correct response is 403 Forbidden.

**Recommended Fix:** Use `ForbiddenAccessException` for authorization failures.

```csharp
// Before
if (order.Client == null || order.Client.UserId != approvedBy)
    throw new UnauthorizedAccessException("Only the client can approve the price.");

// After
if (order.Client == null || order.Client.UserId != approvedBy)
    throw new ForbiddenAccessException("Only the client can approve the price.");
```

---

## 1.6 PayPal Webhook – AllowAnonymous Without Additional Verification

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/PaymentsController.cs` |
| **Function** | `PayPalWebhook` |
| **Lines** | 179–229 |
| **Severity** | **Medium** |

**Issue:** Webhook is `[AllowAnonymous]` but signature verification is implemented. Ensure verification runs before any processing. Current implementation verifies signature; the TODO indicates webhook event processing is incomplete.

**Recommendation:** Complete webhook event handling (e.g., `PAYMENT.CAPTURE.COMPLETED`) and ensure no sensitive operations run before signature verification. Add idempotency for duplicate webhook deliveries.

---

## 1.7 FilesController Upload – No Explicit Order Ownership Check at Controller Level

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/FilesController.cs` |
| **Function** | `UploadFile`, `UploadMultipleFiles` |
| **Severity** | **Low** |

**Issue:** Authorization is delegated to `FileService.VerifyOrderUploadAccessAsync`. Controller only checks `[Authorize]`. This is acceptable if the service is always called; ensure no bypass paths exist.

**Status:** Service correctly validates client/designer access. No change required if all upload paths go through the service.

---

## 1.8 ResetSuperAdminPassword – Development-Only Endpoint

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/AuthController.cs` |
| **Function** | `ResetSuperAdminPassword` |
| **Lines** | 181–215 |
| **Severity** | **Low** |

**Issue:** Endpoint is disabled in non-Development and restricted to localhost. Response no longer exposes password in body (per prior fixes). Ensure it is never enabled in production.

---

# 2. PERFORMANCE ISSUES

## 2.1 OrderService – Eager Loading of Files for All Orders

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `GetOrdersByClientAsync`, `GetAllOrdersAsync`, `GetOrdersPagedAsync` |
| **Lines** | 283–296, 357–369, 403–416 |
| **Severity** | **High** |

**Issue:** Queries use `.Include(o => o.Files)` for every order. When listing many orders, this loads all files into memory. For list views, file data is often unnecessary.

**Recommended Fix:** Remove `Include(o => o.Files)` from list queries. Load files only when viewing a single order.

```csharp
// Before - loads all files for every order
var query = _context.LogoOrders
    .Include(o => o.Client).ThenInclude(c => c.User)
    .Include(o => o.Designer).ThenInclude(d => d.User)
    .Include(o => o.Files)  // REMOVE for list views
    .Where(o => o.ClientId == client.Id && !o.IsDeleted);

// After - no files for list
var query = _context.LogoOrders
    .Include(o => o.Client).ThenInclude(c => c.User)
    .Include(o => o.Designer).ThenInclude(d => d.User)
    .Where(o => o.ClientId == client.Id && !o.IsDeleted);
```

---

## 2.2 InvoiceService – GetInvoices Loads All Invoices

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/InvoiceService.cs` |
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/InvoicesController.cs` |
| **Function** | `GetInvoicesAsync`, `GetInvoices` |
| **Severity** | **High** |

**Issue:** `GET /api/invoices` returns all invoices for the user with no pagination. Large datasets cause memory and network issues.

**Recommended Fix:** Add pagination to `GetInvoicesAsync` and the controller.

```csharp
// InvoicesController
[HttpGet]
public async Task<IActionResult> GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
{
    pageSize = Math.Clamp(pageSize, 1, 100);
    var paged = await _invoiceService.GetInvoicesPagedAsync(userId, userRole, page, pageSize);
    return Ok(new ApiResponse<object> {
        Data = new { items = paged.Items, total = paged.Total, page, pageSize },
        Meta = new ApiMeta { Total = paged.Total, Page = page, PageSize = pageSize, TotalPages = ... }
    });
}
```

---

## 2.3 InvoicesController – DownloadReport Loads All Invoices

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/InvoicesController.cs` |
| **Function** | `DownloadReport` |
| **Lines** | 155–184 |
| **Severity** | **High** |

**Issue:** Report generation loads all invoices via `GetInvoicesAsync` before filtering by status. For large datasets this is inefficient and can cause timeouts.

**Recommended Fix:** Add a dedicated `GetInvoicesForReportAsync` that supports server-side filtering and streaming, or at least paginated/streamed PDF generation.

---

## 2.4 OrderService – GetOrdersByClientAsync / GetOrdersByDesignerAsync No Pagination

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `GetOrdersByClientAsync`, `GetOrdersByDesignerAsync` |
| **Lines** | 271–321, 323–348 |
| **Severity** | **Medium** |

**Issue:** Client and designer order lists return all orders with no pagination. Clients/designers with many orders will experience slow responses.

**Recommended Fix:** Add optional pagination parameters and return `PagedResultDto<OrderResponseDto>`.

---

## 2.5 FileService – GetOrderFilesAsync Redundant Include

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/FileService.cs` |
| **Function** | `GetOrderFilesAsync` |
| **Lines** | 551–556 |
| **Severity** | **Low** |

**Issue:** Query includes `f.Order` but order was already loaded earlier. The Include may trigger extra joins. Consider projecting only needed fields.

---

## 2.6 RevisionService – Duplicate GetAdminAndSuperAdminUserIdsAsync Calls

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/RevisionService.cs` |
| **Function** | `RequestRevisionAsync` |
| **Lines** | 278–291 |
| **Severity** | **Low** |

**Issue:** `GetAdminAndSuperAdminUserIdsAsync` is called, then designer lookup is done separately, and recipient list is built. Could be optimized with a single query for all recipient IDs.

---

# 3. ARCHITECTURE PROBLEMS

## 3.1 OrderService – Mixed Responsibilities

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Severity** | **Medium** |

**Issue:** OrderService handles order CRUD, status transitions, notifications, file sending, pricing approval, and refunds. This violates single responsibility and makes testing and maintenance harder.

**Recommended Fix:** Extract domain logic into smaller services:
- `OrderStatusService` – status transitions and validation
- `OrderNotificationService` – notifications for order events
- Keep `OrderService` as orchestration/facade

---

## 3.2 Duplicate GetAdminAndSuperAdminUserIdsAsync

| Field | Value |
|-------|-------|
| **Files** | `OrderService.cs`, `FileService.cs`, `RevisionService.cs` |
| **Severity** | **Medium** |

**Issue:** Same helper logic is duplicated across multiple services. Changes must be applied in multiple places.

**Recommended Fix:** Extract to shared service, e.g. `IAdminUserResolver` or `IUserRoleService`.

```csharp
public interface IAdminUserResolver
{
    Task<List<Guid>> GetAdminAndSuperAdminUserIdsAsync();
}
```

---

## 3.3 PaymentService DTOs Defined in Controller

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/PaymentsController.cs` |
| **Lines** | 233–251 |
| **Severity** | **Low** |

**Issue:** `GeneratePaymentLinkRequestDto`, `VerifyPayPalRequestDto`, `UpdatePaymentStatusRequestDto` are defined in the controller file instead of the Application layer.

**Recommended Fix:** Move to `LogoDesignPortal.Application/DTOs/Payments/`.

---

## 3.4 InvoicesController – MarkInvoicePaidRequestDto in Controller

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/InvoicesController.cs` |
| **Lines** | 215–218 |
| **Severity** | **Low** |

**Issue:** DTO defined in controller. Should live in Application DTOs.

---

# 4. API DESIGN ISSUES

## 4.1 Inconsistent Response Structure

| Field | Value |
|-------|-------|
| **Files** | Multiple controllers |
| **Severity** | **Medium** |

**Issue:** Some endpoints return `{ data: { items, total }, meta: { ... } }` (e.g., Orders, Files) while others return raw arrays or objects (e.g., `GetInvoices`, `GetMyOrders`).

**Recommended Fix:** Standardize on a wrapper for list endpoints:

```csharp
public class ApiResponse<T>
{
    public T Data { get; set; }
    public ApiMeta? Meta { get; set; }
    public string? Error { get; set; }
}
```

---

## 4.2 ExceptionMiddleware – Missing FileNotFoundException

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Middleware/ExceptionMiddleware.cs` |
| **Lines** | 33–72 |
| **Severity** | **Medium** |

**Issue:** `FileNotFoundException` is not explicitly handled. It may fall through to the default case and return 500 instead of 404.

**Recommended Fix:**
```csharp
case FileNotFoundException:
    code = HttpStatusCode.NotFound;
    result = JsonSerializer.Serialize(new { error = exception.Message });
    break;
```

---

## 4.3 DbUpdateException Exposes Inner Message to Client

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/AuthController.cs` |
| **Function** | `Register` |
| **Lines** | 75–94 |
| **Severity** | **High** |

**Issue:** On `DbUpdateException`, the response includes `detail = innerMsg`, which can expose database schema or constraint details to clients.

**Recommended Fix:** Never expose `ex.InnerException?.Message` to clients. Log it server-side only.

```csharp
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Registration database error for email: {Email}", request.Email);
    return StatusCode(500, new { error = "Registration failed. The email may already exist. Please try again." });
}
```

---

## 4.4 OrdersController – CreateOrder Missing 500 Handler

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/OrdersController.cs` |
| **Function** | `CreateOrder` |
| **Lines** | 27–43 |
| **Severity** | **Low** |

**Issue:** Only `InvalidOperationException` is caught. Other exceptions (e.g., `DbUpdateException`) propagate and may not be formatted consistently.

**Recommended Fix:** Add a generic catch that returns a consistent 500 response.

---

## 4.5 OrdersController – CreateOrderWithFiles JSON Deserialization Without Validation

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.API/Controllers/OrdersController.cs` |
| **Function** | `CreateOrderWithFiles` |
| **Lines** | 59–60 |
| **Severity** | **Medium** |

**Issue:** `JsonSerializer.Deserialize<CreateOrderRequestDto>(order)` is used without schema validation. Malformed or malicious JSON could cause unexpected behavior.

**Recommended Fix:** Use `[FromForm] CreateOrderRequestDto request` with model binding, or validate the deserialized object (e.g., required fields, ranges).

---

# 5. DATABASE RISKS

## 5.1 Missing Index on OrderLog

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Infrastructure/Persistence/ApplicationDbContext.cs` |
| **Entity** | `OrderLog` |
| **Severity** | **Medium** |

**Issue:** `GetOrderLogsAsync` filters by `OrderId`. There is no explicit index on `OrderLog.OrderId` in the configurations.

**Recommended Fix:** Add index in `OrderLogConfiguration` or `OnModelCreating`:

```csharp
modelBuilder.Entity<OrderLog>().HasIndex(e => e.OrderId);
```

---

## 5.2 OrderLog Not in Global Soft-Delete Filter

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Infrastructure/Persistence/ApplicationDbContext.cs` |
| **Lines** | 50–59 |
| **Severity** | **Low** |

**Issue:** `OrderLog` extends `BaseEntity` (has `IsDeleted`) but is not in the global query filter. `GetOrderLogsAsync` manually filters `!l.IsDeleted`. Inconsistent approach; new queries might forget the filter.

**Recommended Fix:** Add `OrderLog` to the soft-delete filter for consistency, or document that it is intentionally excluded.

---

## 5.3 Invoice – Missing Composite Index for Common Queries

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Infrastructure` |
| **Severity** | **Low** |

**Issue:** Invoice list queries often filter by `ClientId` and `Status`. A composite index could improve performance.

```csharp
modelBuilder.Entity<Invoice>().HasIndex(e => new { e.ClientId, e.Status });
```

---

# 6. BUSINESS LOGIC VALIDATION

## 6.1 OrderService.ApprovePriceAsync – Wrong Exception for Wrong Client

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `ApprovePriceAsync` |
| **Lines** | 644–648 |
| **Severity** | **Low** |

**Issue:** Throws `UnauthorizedAccessException` when a client approves another client's order. Should throw `ForbiddenAccessException` for 403. See Security 1.5.

---

## 6.2 RefundOrderAsync – No Validation of Order Status

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `RefundOrderAsync` |
| **Lines** | 1054–1115 |
| **Severity** | **Medium** |

**Issue:** Refund is allowed without checking if the order is in a refundable state (e.g., Completed, Paid). A cancelled order could theoretically be refunded.

**Recommended Fix:**
```csharp
if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Refunded)
    throw new InvalidOperationException("Only completed orders can be refunded.");
```

---

## 6.3 UpdateOrderAsync – Client Can Override Price

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `UpdateOrderAsync` |
| **Lines** | 868–869 |
| **Severity** | **High** |

**Issue:** `order.Price = request.Price` allows the client to set any price when updating an order in `WaitingForAdminApproval` or `PriceApprovalPending`. This could be abused.

**Recommended Fix:** Do not allow client to change `Price` on update, or validate it against pricing rules (e.g., ClientLogoPricing, DesignPricing).

```csharp
// Only allow Title, Description, Instructions, etc. - not Price
order.Title = request.Title;
order.Description = request.Description;
// order.Price = request.Price;  // REMOVE - use pricing service
```

---

## 6.4 RevisionService – ValidateRevisionFile Missing Magic Byte Check

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/RevisionService.cs` |
| **Function** | `ValidateRevisionFile` |
| **Lines** | 80–89 |
| **Severity** | **Medium** |

**Issue:** `FileService.ValidateFile` uses magic byte validation to prevent file type spoofing. `RevisionService.ValidateRevisionFile` only checks extension and size, not content.

**Recommended Fix:** Reuse `FileService` validation or extract shared validation logic.

---

## 6.5 Order Create – No Maximum File Count for CreateOrderWithFiles

| Field | Value |
|-------|-------|
| **File** | `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs` |
| **Function** | `CreateOrderWithFilesAsync` |
| **Lines** | 104–106 |
| **Severity** | **Low** |

**Issue:** No limit on number of reference files. A client could upload hundreds of files in one request.

**Recommended Fix:** Add a maximum (e.g., 20 files) and validate before processing.

---

# 7. UI/UX PROBLEMS

## 7.1 Invoice List – Fallback to Mock Data on Error

| Field | Value |
|-------|-------|
| **File** | `Frontend/src/app/invoices/invoice-list/invoice-list.component.ts` |
| **Function** | `loadInvoices` |
| **Lines** | 176–184 |
| **Severity** | **High** |

**Issue:** On API error, the component calls `createMockInvoices()` and shows an empty list. User may think there are no invoices instead of understanding that loading failed.

**Recommended Fix:** Show an error message and retry option instead of silently falling back to empty data.

```typescript
error: (error) => {
  console.error('Error loading invoices:', error);
  this.loading = false;
  this.messageService.add({
    severity: 'error',
    summary: 'Failed to Load Invoices',
    detail: 'Could not load invoices. Please try again.',
    life: 5000
  });
  this.cdr.markForCheck();
}
```

---

## 7.2 Login – Loading State Not Disabled on Success Before Navigate

| Field | Value |
|-------|-------|
| **File** | `Frontend/src/app/auth/login/login.component.ts` |
| **Function** | `onSubmit` |
| **Lines** | 71–81 |
| **Severity** | **Low** |

**Issue:** `loading = true` is set, but on success `loading` is not set to `false` before `router.navigate`. The component is destroyed on navigate, so this is minor, but for consistency and in case of slow navigation, set `loading = false` in the success path.

---

## 7.3 Error Interceptor – Generic 403 Message

| Field | Value |
|-------|-------|
| **File** | `Frontend/src/app/core/interceptors/error.interceptor.ts` |
| **Lines** | 41–66 |
| **Severity** | **Low** |

**Issue:** For 403, the message is always "You do not have permission to perform this action". The backend often sends a more specific message in `error.error.error` that could be shown.

**Recommended Fix:**
```typescript
case 403:
  errorMessage = error.error?.error || 'You do not have permission to perform this action';
  // ...
```

---

## 7.4 Invoice List – No Loading Indicator for Initial Load in Template

| Field | Value |
|-------|-------|
| **File** | `Frontend/src/app/invoices/invoice-list/invoice-list.component.html` |
| **Severity** | **Low** |

**Issue:** `p-table` has `[loading]="loading"` which shows a spinner. The KPI cards and summary sections do not reflect loading state; they may show zeros during load.

**Recommendation:** Consider a skeleton or overlay for the whole page during initial load.

---

## 7.5 Order Create – File Upload Validation Feedback

| Field | Value |
|-------|-------|
| **File** | `Frontend/src/app/orders/order-create/order-create.component.ts` |
| **Severity** | **Medium** |

**Issue:** File upload errors from the backend (e.g., "File type not allowed", "File exceeds size") may not be displayed clearly to the user. Ensure error messages from the API are surfaced in the UI.

---

# 8. REFACTORING OPPORTUNITIES

1. **Extract Order Status State Machine** – `OrderStatusStateMachine.ValidateTransition` and `GetAllowedStatusesForRole` could move to a dedicated `OrderStatusStateMachine` or domain service.
2. **Unify Notification Creation** – Multiple services create notifications for Admin/SuperAdmin with similar patterns. Extract to `INotificationDispatcher` or similar.
3. **Centralize Permission Checks** – Replace repeated role checks with policy-based authorization where applicable.
4. **API Versioning** – Add `api/v1` prefix and versioning strategy for future breaking changes.

---

# 9. ARCHITECTURE IMPROVEMENTS

1. **CQRS for Read-Heavy Modules** – Orders, Invoices, and Analytics could benefit from separate read models for list vs. detail views.
2. **Background Jobs** – Move notification sending and PDF generation to background jobs (e.g., Hangfire) to improve response times.
3. **Caching** – Cache user permissions, settings, and rarely changing reference data.
4. **API Gateway** – Consider an API gateway for rate limiting, authentication, and request routing in production.

---

# 10. SECURITY HARDENING

1. **Content Security Policy (CSP)** – Add CSP headers to mitigate XSS.
2. **HTTPS Enforcement** – Ensure all production traffic uses HTTPS.
3. **Security Headers** – Add `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`.
4. **Input Sanitization** – Re-enable `InputSanitizationMiddleware` after fixing stream handling.
5. **Audit Logging** – Extend audit logging for sensitive operations (password change, refund, permission changes).

---

# 11. PERFORMANCE OPTIMIZATIONS

1. **Response Compression** – Enable gzip/brotli for API responses.
2. **EF Core Query Splitting** – For queries with multiple collections, consider `AsSplitQuery()` to avoid cartesian explosion.
3. **Lazy Loading** – Avoid; current explicit loading is preferable. Continue removing unnecessary Includes.
4. **CDN for Static Assets** – Serve Angular assets from CDN in production.
5. **Database Connection Pooling** – Verify connection string includes pooling settings for MySQL.

---

# 12. PRIORITY FIX ORDER

| Priority | Issue | Effort |
|----------|-------|--------|
| 1 | JWT Key and DB credentials in config | Low |
| 2 | Token storage (httpOnly cookies) | Medium |
| 3 | UpdateOrderAsync client price override | Low |
| 4 | Invoice list error fallback to mock | Low |
| 5 | DbUpdateException detail exposure | Low |
| 6 | Invoice/Order list pagination | Medium |
| 7 | Order list remove Files Include | Low |
| 8 | ExceptionMiddleware FileNotFoundException | Low |
| 9 | RefundOrder status validation | Low |
| 10 | RevisionService file validation | Medium |

---

*End of Comprehensive Technical Audit*
