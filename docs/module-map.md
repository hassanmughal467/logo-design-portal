# Module Map

Maps every backend and frontend module to its code location, routes, and roles. Verified against `Backend/src/` and `Frontend/src/app/`. See [api.md](api.md) for the endpoint-level route map.

## Feature module overview

| Module | Backend controller(s) | Application service(s) | Frontend route(s) | Roles |
|---|---|---|---|---|
| Auth | `AuthController` | `AuthService`, `AuthCookieService` (API), `JwtTokenService` (Infra) | `/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/reset-password` | Public + all |
| Orders | `OrdersController` | `OrderService`, `RevisionService`, helpers | `/orders`, `/orders/create`, `/orders/:id`, `/orders/:orderId/upload` | Client, Designer, Admin, SuperAdmin |
| Quotes | `QuotesController` | `QuoteService` | `/quotes`, `/quotes/:id` | Client, Admin, SuperAdmin |
| Files | `FilesController` | `FileService` | `/files` | All (scoped) |
| Revisions | `RevisionsController` | `RevisionService` | inside order detail | Client, Admin, SuperAdmin, Designer (read) |
| Comments | `CommentsController` | `CommentService` | inside order detail | All (visibility-filtered) |
| Messages | `MessagesController` | `MessageService` | `/messages` | All (admin-relayed) |
| Notifications | `NotificationsController` + `NotificationHub` | `NotificationService` | `/notifications` + bell | All |
| Invoices/Billing | `InvoicesController`, `BillingController` | `InvoiceService`, `BillingService`, `InvoicePdfService` | `/invoices`, `/invoices/flexible-builder`, `/invoices/:id` | Client (own), Admin, SuperAdmin |
| Payments | `PaymentsController` | `PaymentService` | embedded in invoices (`payments/` component module) | Client, Admin, SuperAdmin |
| Designer payout | `DesignerPayoutController`, `DesignerInvoiceController` | `DesignerPayoutService` | `/financial/designer-payout`, `/financial/designer-payout/invoice/:id` | Designer (own), Admin, SuperAdmin |
| Pricing | `ClientLogoPricingController`, `DesignerLogoPricingController` | `ClientLogoPricingService`, `DesignerLogoPricingService` | `/client-pricing`, `/designer-pricing` | Admin, SuperAdmin |
| Analytics | `AnalyticsController`, `AdminClientAnalyticsController`, `FinancialController`, `ClientController` | `AnalyticsService`, `ClientAnalyticsService`, `ClientChurnAnalyticsService`, `FinancialAnalyticsService`, `ClientFinancialInsightsService` | `/analytics`, `/financial`, `/client-intelligence` | Admin, SuperAdmin (Client/Designer see scoped views) |
| Gallery | `GalleryController` | `GalleryService` | `/gallery` | Client only |
| Reviews | `ReviewsController` | `ReviewService` | `/reviews` | Client creates; all read |
| Users & roles | `UsersController`, `RolesController`, `PermissionsController` | `UserService`, `RoleService`, `PermissionService` | `/users`, `/designers`, `/clients`, `/permissions` | Admin, SuperAdmin (permissions: SuperAdmin) |
| Settings & audit | `SettingsController`, `AuditLogsController` | `SettingsService`, `AuditLogService` | `/settings` | Admin, SuperAdmin |
| System health | `SystemController` | — | — | Anonymous |

## Backend modules

### Domain (`Backend/src/LogoDesignPortal.Domain`)

- `Entities/` — 30 entities on `BaseEntity` (Id, timestamps, soft delete). Core aggregates: `User`/`Role`/`Permission`/`RolePermission`, `ClientProfile`/`DesignerProfile`, `LogoOrder` (+ `LogoFile`, `OrderRevision`, `RevisionFile`, `OrderComment`, `OrderStatusHistory`, `OrderLog`), `Quote`, `Invoice` (+ `InvoiceOrder`, `InvoiceLog`, `Payment`), `DesignerInvoice` (+ items, adjustments), pricing tables (`DesignPricing`, `ClientLogoPricing`, `DesignerLogoPricing`), `Message`, `Review`, `Notification`, `ClientGallery`, `Settings`, `AuditLog`.
- `Enums/` — `OrderStatus`, `QuoteStatus`, `InvoiceStatus`, `PriceApprovalStatus`, `BillingType`, `FileType`/`FileCategory`/`FileStatus`, `CommentType`, `NotificationType`, `DesignCategory`/`DesignType`, etc.
- `OrderStatusStateMachine.cs` — the only authority for order status transitions.

### Application (`Backend/src/LogoDesignPortal.Application`)

29 services (all interface-backed). Grouped:

| Group | Services |
|---|---|
| Orders & delivery | `OrderService`, `RevisionService`, `FileService`, `GalleryService` |
| Quotes | `QuoteService` |
| Billing & payments | `InvoiceService`, `BillingService`, `PaymentService`, `InvoicePdfService`, `DesignerPayoutService` |
| Pricing | `ClientLogoPricingService`, `DesignerLogoPricingService` |
| Analytics | `AnalyticsService`, `ClientAnalyticsService`, `ClientChurnAnalyticsService`, `FinancialAnalyticsService`, `ClientFinancialInsightsService` |
| Communication | `CommentService`, `MessageService`, `NotificationService`, `ReviewService` |
| Identity & access | `AuthService`, `UserService`, `RoleService`, `PermissionService`, `ClientProfileEnsureService` |
| Cross-cutting | `SettingsService`, `AuditLogService`, `NullFileUploadScanHook` |

Key helpers (`Helpers/`): `OrderStatusTransitionHelper`, `OrderLockingHelper`, `UploadSecurityHelper`, `RevisionLimitHelper`, `ClientCurrencyHelper`, `PaymentInvoiceAccessHelper`, `StorageKeyHelper`, `FileStorageOperations`, `DesignPricingHelper`, `InvoiceUpdateChangeLabels`, notification format/redirect/aggregation helpers, `TextInputSanitizer`.

### Infrastructure (`Backend/src/LogoDesignPortal.Infrastructure`)

- `Persistence/` — `ApplicationDbContext`, 30 entity configurations, `SlowQueryLoggingInterceptor`
- `Migrations/` — 48 EF migrations
- `Authentication/` — `JwtTokenService`, `JwtSigningKeyRotationHelper`
- `Currency/` — `CurrencyService` (+ DI extension)
- `Storage/` — `LocalFileStorageService`, `R2FileStorageService`, `StorageServiceCollectionExtensions`, `MigrateLocalFilesToR2` (manual utility)
- Email sending (SMTP), Redis wiring, `DependencyInjection.cs` (provider selection MySQL/SQLite)

### API (`Backend/src/LogoDesignPortal.API`)

- `Controllers/` — 27 controllers (see [api.md](api.md))
- `Middleware/` — CORS preflight, security headers, correlation id, slow request, exception, CSRF, rate limiting, input sanitization (disabled)
- `Infrastructure/` — Hangfire filters, distributed rate limiters
- `Hubs/` — `NotificationHub`
- `Services/` — `AuthCookieService`, `FileStorageInitializer`, `OrphanFileCleanupService`, `BillingAutoInvoiceService`, SignalR realtime senders
- `Attributes/` — `RequirePermissionAttribute`
- `Health/` — custom health checks
- `Hosting/` — `ScalabilityServiceRegistration` (Redis, Hangfire, rate limiter, recurring jobs)

## Frontend modules (`Frontend/src/app/`)

All feature modules are lazy-loaded under a `MainLayoutComponent` shell guarded by `AuthGuard`; role restrictions via `RoleGuard` reading `route.data['roles']`. Auth pages use `AuthLayoutComponent`.

| Folder | Route | RoleGuard roles |
|---|---|---|
| `auth/` | `/auth/*` | public |
| `dashboard/` | `/dashboard` | any authenticated |
| `users/` | `/users` | SuperAdmin, Admin |
| `orders/` (+ `order-detail`) | `/orders`, `/orders/create`, `/orders/:id`, `/orders/:orderId/upload` | any authenticated (API scopes data) |
| `quotes/` | `/quotes`, `/quotes/:id` | SuperAdmin, Admin, Client |
| `designers/` | `/designers` | SuperAdmin, Admin |
| `permissions/` | `/permissions` | SuperAdmin |
| `files/` | `/files` | any authenticated |
| `clients/` | `/clients`, `/clients/:id` | SuperAdmin, Admin |
| `client-pricing/` | `/client-pricing` | SuperAdmin, Admin |
| `designer-pricing/` | `/designer-pricing` | SuperAdmin, Admin |
| `projects/` | `/projects`, `/projects/:id` | any authenticated |
| `invoices/` | `/invoices`, `/invoices/flexible-builder`, `/invoices/:id` | SuperAdmin, Admin, Client |
| `analytics/` | `/analytics` | SuperAdmin, Admin, Client |
| `financial/` | `/financial`, `/financial/designer-payout`, `/financial/designer-payout/invoice/:id` | SuperAdmin, Admin, Client, Designer |
| `client-intelligence/` | `/client-intelligence` | SuperAdmin, Admin |
| `messages/` | `/messages` | any authenticated |
| `reviews/` | `/reviews` | any authenticated |
| `settings/` | `/settings` | any authenticated (API restricts writes) |
| `notifications/` | `/notifications` | any authenticated |
| `gallery/` | `/gallery` | Client |
| `payments/` | (no route — embedded components) | — |
| `layout/`, `shared/`, `core/` | infrastructure | — |

### Core (`src/app/core/`)

- **Services**: `ApiService` (HTTP wrapper on `{apiUrl}/api`), `AuthService`, `NotificationService` (with 60 s polling fallback), `RealtimeNotificationService` (SignalR client, auto-reconnect, rejoins user group), `RealtimeStatusService`, `PermissionsService`, `LoadingService`, `LoggerService`, `DashboardService`, `SharedListDataService`, plus analytics/billing/payment API services.
- **Interceptors** (registered order): `CookieCredentialsInterceptor` → `CsrfInterceptor` → `TokenInterceptor` (bearer + 401 refresh when not cookie auth) → `HttpRequestTrackerInterceptor` → `HttpLoadingErrorInterceptor`.
- **Guards**: `AuthGuard`, `RoleGuard` (`PermissionGuard` exists but is unused in routing — TODO: wire or remove).

### Shared (`src/app/shared/`)

`HasPermissionDirective`, pipes (`RoleNamePipe`, `RelativeTimePipe`), models, order-locking/status-display utils, and a UI component kit (PageHeader, KpiCard, StatusBadge, DataTableWrapper, ModalDialog, skeletons, buttons).

### Environments (`src/environments/`)

`AppEnvironment` contract: `production`, `staging`, `apiUrl`, `apiVersion`, optional `signalRUrl`, `useCookieAuth`, `enableDebugLogging`.

| Build config | File | apiUrl | Cookie auth |
|---|---|---|---|
| development (default) | `environment.development.ts` | `https://localhost:44398` | yes |
| staging | `environment.staging.ts` | `https://staging-api.hawkmerchandising.com` | yes |
| production | `environment.production.ts` | `https://api.hawkmerchandising.com` | yes |
| testing / e2e | `environment.testing.ts` | `https://localhost:44398` | **no** (bearer for test automation) |

`environment.network.ts` is a stale placeholder not wired into `angular.json` — TODO: delete or update.

## Test modules

| Location | Contents |
|---|---|
| `Backend/src/LogoDesignPortal.Domain.Tests` | 3 test classes — state machine and domain rules |
| `Backend/src/LogoDesignPortal.Application.Tests` | 45 test classes — services, storage, currency, auth helpers |
| `Backend/src/LogoDesignPortal.API.IntegrationTests` | 38 test classes — controllers, security (CSRF, CORS, Hangfire auth, cookies), config validation |
| `Frontend/src/**/*.spec.ts` | Karma/Jasmine unit tests |
| `Frontend/e2e/tests/` | 47 Playwright specs: auth, security (RBAC, IDOR, uploads), workflows, smoke, UAT, resilience, visual, elite serial |
| `load-tests/k6/` | auth spike, concurrent orders/uploads, dashboard read, invoice spike |
