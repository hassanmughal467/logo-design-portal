# Enterprise QA — Test Scenarios & Case Index

**Product:** Hawk Merchandising Web Portal (Angular + API)  
**Context:** Client → Admin → Designer → completion / revision loop; invoices, files, messaging, analytics.  
**Roles:** Super Admin, Admin, Client, Designer  
**Companion artifact:** Import `ENTERPRISE_QA_TEST_CASES.csv` into Excel, Google Sheets, or Jira (CSV import) for the full **235 detailed test cases** (TC-001–TC-235). Optional regenerator: `docs/_generate_qa_csv.py` (`python docs/_generate_qa_csv.py`).

---

## 1. High-level test scenarios

### 1.1 Authentication & session

- SC-A01: Valid user logs in and lands on Dashboard with correct role context.
- SC-A02: Invalid credentials are rejected with a clear, non-disclosing error.
- SC-A03: Unauthenticated deep links to protected routes redirect to login.
- SC-A04: Session expiry or token invalidation forces re-authentication gracefully.
- SC-A05: Logout clears session; back button does not expose protected data without re-login.
- SC-A06: Register flow completes or fails predictably per environment policy.
- SC-A07: Forgot-password request accepts valid email format and handles unknown email per policy.
- SC-A08: Password reset token flow (if used) validates one-time use and expiration.

### 1.2 Navigation, layout & global UX

- SC-N01: Sidebar menu reflects role; forbidden items are absent, not merely disabled.
- SC-N02: Breadcrumbs and page titles match the active module.
- SC-N03: Global search routes to Orders with query preserved for Admin/Client/Designer.
- SC-N04: Notification bell/panel shows unread count consistent with Notifications page.
- SC-N05: Mobile/tablet breakpoint: sidebar toggles; primary actions remain reachable.
- SC-N06: Long-running API calls show loaders; no duplicate submissions on double-click.

### 1.3 Dashboard

- SC-D01: Dashboard KPIs match API data for the logged-in role.
- SC-D02: Client quick actions (Create Order, Get Quote) open correct dialogs.
- SC-D03: Admin quick links navigate to Users, Orders, Designers, Permissions, Files.
- SC-D04: Client gallery preview on dashboard filters by search term.
- SC-D05: Empty states display when no orders/invoices/gallery items exist.

### 1.4 Orders — core workflow

- SC-O01: Client creates order; row appears in My Orders with correct initial status.
- SC-O02: Admin views all orders; filters and KPI cards narrow the grid.
- SC-O03: Admin approves pending order; status transitions and notifications (if any) fire.
- SC-O04: Admin assigns designer; designer sees order in assigned list only.
- SC-O05: Designer updates status to preview delivered; client sees update.
- SC-O06: Client requests revision; round count and status follow business rules.
- SC-O07: Multiple revision rounds complete without data loss on comments/files.
- SC-O08: Client approves final logo; admin can mark completed.
- SC-O09: Admin “Add Completed Order” backfills historical work without breaking billing.
- SC-O10: Order detail modal actions respect state machine (no invalid transitions).

### 1.5 Quotes

- SC-Q01: Client creates quote request from Orders or Quotes module.
- SC-Q02: Admin reviews and responds; client sees updated quote detail.
- SC-Q03: Quote list search and pagination behave under large datasets.

### 1.6 Users & RBAC

- SC-U01: Super Admin/Admin creates Designer user; row appears with correct role.
- SC-U02: Super Admin/Admin creates Client with extended profile fields validated.
- SC-U03: Super Admin opens Permissions; matrix toggles without breaking list view.
- SC-U04: Admin cannot access `/permissions` (Super Admin only).
- SC-U05: Designer cannot access Users, Clients, Reviews, Client Intelligence.

### 1.7 Clients, projects, designers

- SC-C01: Clients list search filters by name, email, company.
- SC-C02: Client detail shows linked activity consistent with API.
- SC-P01: Projects list scoped: client sees only own projects; admin sees all.
- SC-P02: Project detail opens from grid without console errors.
- SC-G01: Designers list loads; designer detail shows availability/profile fields.

### 1.8 Messages

- SC-M01: Unread badge matches server count after read action.
- SC-M02: Thread open displays chronological messages.
- SC-M03: Sending empty or oversized message handled per validation rules.

### 1.9 Reviews

- SC-R01: Reviews grid loads for Admin; Designer/Client cannot access route.
- SC-R02: Search on client name and comment text works.

### 1.10 Files

- SC-F01: Upload valid file type to correct order/logo context.
- SC-F02: Reject or warn on disallowed extension / virus scan failure (if integrated).
- SC-F03: Super Admin search-first file discovery returns cross-tenant results only as allowed.
- SC-F04: Grid vs table view toggles without losing selection context.

### 1.11 Gallery (Client)

- SC-GY01: `/gallery` loads only for Client; other roles denied or redirected.
- SC-GY02: Preview and download use authorized URLs (no direct unauthenticated file access).

### 1.12 Analytics & client intelligence

- SC-AN01: Detail Analytics sections render for Client vs Admin without cross-leakage.
- SC-AN02: Client Intelligence Overview vs Churn tabs switch without stale data bleed.
- SC-AN03: Zero-data analytics shows empty charts/tables, not infinite spinners.

### 1.13 Invoices & financial

- SC-I01: Invoice list filters and opens detail for Super Admin/Admin/Client.
- SC-I02: Flexible Invoice Builder: date range vs custom order selection produces draft.
- SC-I03: Financial Overview aggregates match underlying orders/invoices sample set.
- SC-I04: Designer Payout: Admin KPIs vs Designer self-service view isolation.
- SC-I05: Client Pricing: selecting client loads rules; save validates required fields.
- SC-I06: Designer Pricing: PKR fields validate numeric bounds.

### 1.14 Notifications & settings

- SC-NT01: Mark all read updates unread count everywhere.
- SC-NT02: Notification deep link opens related entity if implemented.
- SC-ST01: Settings tabs save Business Info and reflect on reload.
- SC-ST02: Invalid email/phone in settings shows field-level errors.

### 1.15 Security, performance, edge

- SC-S01: Direct URL to `/users` as Client returns 403 or redirect, not partial HTML leak.
- SC-S02: API errors from tampered JWT do not expose stack traces in UI.
- SC-P01: Orders table with 500+ rows: pagination and sort remain responsive.
- SC-P02: Concurrent edit on same order: last-write-wins or conflict message is explicit.
- SC-E01: Maximum-length description on order create: truncate vs reject documented behavior.

---

## 2. Detailed test cases — usage

| Column | Description |
|--------|-------------|
| Test Case ID | TC-001 … TC-235 |
| Module | Functional area |
| Title | One-line intent |
| Preconditions | Accounts, data, environment |
| Test Steps | Numbered steps separated by ` \| ` in CSV |
| Expected Result | Observable pass criteria |
| Priority | High / Medium / Low |
| Type | Functional / Negative / Edge / Security / Performance / UI |

**Import:** Open `ENTERPRISE_QA_TEST_CASES.csv` in Excel (Data → From Text/CSV) or Jira CSV import.  
**Traceability:** Map scenarios SC-* to related TC-* by Module and Title keywords.

---

## 3. Coverage matrix (summary)

| Category | Approx. TC count | ID ranges |
|----------|------------------|-----------|
| Authentication & session | 19 | TC-001–TC-018, TC-228 |
| Navigation & global UX | 13 | TC-019–TC-030, TC-234 |
| Dashboard | 15 | TC-031–TC-044, TC-227 |
| Orders | 54 | TC-045–TC-096, TC-221, TC-235 |
| Quotes | 11 | TC-097–TC-106, TC-222 |
| Users & Permissions | 17 | TC-107–TC-122, TC-223 |
| Clients & Projects & Designers | 19 | TC-123–TC-140, TC-232 |
| Messages & Reviews | 16 | TC-141–TC-154, TC-226, TC-233 |
| Files & Gallery | 17 | TC-155–TC-170, TC-224 |
| Analytics & Client Intelligence | 13 | TC-171–TC-182, TC-229 |
| Invoices & Financial & Pricing | 24 | TC-183–TC-204, TC-225, TC-230 |
| Notifications & Settings | 11 | TC-205–TC-214, TC-231 |
| Security & RBAC & perf & UI | 6 | TC-215–TC-220 |

**Total: 235 test cases** (exceeds minimum 200).

## 4. Jira / Xray / TestRail quick mapping

| CSV column | Typical Jira CSV field |
|------------|-------------------------|
| Test Case ID | Summary prefix or custom field `Test Case ID` |
| Module | Component or Label |
| Title | Summary (body) |
| Preconditions | Preconditions / Description (custom) |
| Test Steps | Steps (numbered) or Zephyr step field |
| Expected Result | Expected Result |
| Priority | Priority |
| Type | Labels e.g. `functional`, `negative` |

Use **UTF-8** encoding when importing the CSV.

---

*Maintained for client delivery / regression. Align execution priority with release scope: smoke = High only, full regression = all.*
