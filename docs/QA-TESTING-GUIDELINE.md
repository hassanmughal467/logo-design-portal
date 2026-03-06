# QA Testing Guideline — Logo Design Portal

**Version:** 1.0  
**Last Updated:** March 5, 2025  
**Purpose:** Step-by-step testing guide to identify and resolve bugs in logic, functionality, and UI across all modules.

---

## Table of Contents

1. [Testing Approach](#1-testing-approach)
2. [Prerequisites & Environment](#2-prerequisites--environment)
3. [Bug Reporting Template](#3-bug-reporting-template)
4. [Module-by-Module Testing](#4-module-by-module-testing)
5. [Resolution Workflow](#5-resolution-workflow)

---

## 1. Testing Approach

### Testing Focus Areas

| Area | What to Verify |
|------|----------------|
| **Logic** | Business rules, calculations, state transitions, role-based behavior |
| **Functionality** | CRUD operations, API integration, form submissions, workflows |
| **UI** | Layout, responsiveness, accessibility, error messages, loading states |

### Role-Based Test Accounts

Create or use test accounts for each role:

| Role | Access Level |
|------|--------------|
| **SuperAdmin** | Full access: Users, Clients, Designers, Permissions, Settings, Orders, Invoices, etc. |
| **Admin** | Users, Clients, Orders, Invoices, Settings (partial), no Permissions |
| **Designer** | Assigned orders, Files, Messages, Dashboard |
| **Client** | Own orders, Create order, Gallery, Invoices |

---

## 2. Prerequisites & Environment

- [ ] Backend API running (e.g., `http://localhost:5000` or configured API URL)
- [ ] Frontend running (e.g., `ng serve`)
- [ ] Database seeded with test data
- [ ] Browser DevTools open (Console, Network tabs)
- [ ] Test accounts for SuperAdmin, Admin, Designer, Client

---

## 3. Bug Reporting Template

When a bug is found, document it using this structure:

```markdown
**Module:** [e.g., Orders]
**Type:** Logic | Functionality | UI
**Severity:** Critical | High | Medium | Low

**Steps to Reproduce:**
1. 
2. 
3. 

**Expected Result:**

**Actual Result:**

**Screenshots/Console Errors:** (if applicable)

**Environment:** Browser, OS
```

---

## 4. Module-by-Module Testing

---

### Module 1: Authentication (`/auth`)

**Routes:** `/auth/login`, `/auth/register`, `/auth/forgot-password`, `/auth/reset-password`

#### 1.1 Login (`/auth/login`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/auth/login` | — | Page loads | Form visible, no layout shift | |
| 2 | Submit empty form | Validation blocks submit | No API call | Error messages shown | |
| 3 | Submit invalid email format | Validation catches | No API call | Inline error on email field | |
| 4 | Submit wrong password | — | API returns 401 | Clear error message | |
| 5 | Submit valid credentials | — | API returns token, redirect to dashboard | Loading state, redirect | |
| 6 | Check token storage | Token stored | — | — | |
| 7 | Navigate to `/dashboard` | AuthGuard allows | — | Dashboard loads | |
| 8 | Test legacy redirect `/login` | — | Redirects to `/auth/login` | — | |

#### 1.2 Register (`/auth/register`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/auth/register` | — | Page loads | Form visible | |
| 2 | Submit with duplicate email | — | API returns conflict | Error message | |
| 3 | Submit with weak password | Validation | No submit or API error | Password strength feedback | |
| 4 | Submit valid data | — | User created, redirect | Success, redirect to login | |
| 5 | Password mismatch | Validation | No submit | Error on confirm field | |

#### 1.3 Forgot Password (`/auth/forgot-password`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Enter valid email | — | API sends reset email | Success message | |
| 2 | Enter invalid email | — | API error or generic message | No sensitive info leaked | |
| 3 | Submit empty | Validation | No API call | Error message | |

#### 1.4 Reset Password (`/auth/reset-password`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open with valid token (from email link) | — | Form loads | Token in URL/state | |
| 2 | Open with invalid/expired token | — | Error or redirect | Clear message | |
| 3 | Submit new password | — | Password updated | Success, redirect to login | |
| 4 | Password mismatch | Validation | No submit | Error message | |

**Bugs Found (Module 1):**  
_Record bugs here before moving to next module._

---

### Module 2: Dashboard (`/dashboard`)

**Route:** `/dashboard`  
**Access:** All authenticated users

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Login as SuperAdmin, open dashboard | Stats for SuperAdmin | API returns data | Cards, charts render | |
| 2 | Login as Admin | Stats for Admin | API returns data | Correct metrics | |
| 3 | Login as Designer | Stats for Designer | Assigned orders, etc. | Role-specific widgets | |
| 4 | Login as Client | Stats for Client | Own orders, gallery | Client-specific view | |
| 5 | Click "Approve Price" quick action | — | Navigates/opens correct order | — | |
| 6 | Click "Mark Paid" quick action | — | Works or navigates | — | |
| 7 | Check charts | Data matches API | — | Charts render, no errors | |
| 8 | Check recent orders list | Correct orders by role | — | List displays | |
| 9 | Responsive layout | — | — | Works on mobile/tablet | |

**Bugs Found (Module 2):**

---

### Module 3: Users (`/users`)

**Route:** `/users`  
**Access:** SuperAdmin, Admin

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Login as Designer/Client, navigate to `/users` | RoleGuard blocks | 403 or redirect | No access | |
| 2 | Login as Admin, open `/users` | — | User list loads | Table with users | |
| 3 | Search/filter users | Filter logic | API or client filter | Results update | |
| 4 | Click "Create User" | — | Dialog/form opens | Form visible | |
| 5 | Create user with valid data | — | User created | Success, list refreshes | |
| 6 | Create user with duplicate email | — | Error returned | Error message | |
| 7 | Edit user | — | PUT succeeds | Form pre-filled, save works | |
| 8 | Deactivate user | — | User deactivated | Status updated | |
| 9 | Reactivate user | — | User reactivated | Status updated | |
| 10 | Reset user password | — | Password reset | Success message | |
| 11 | SuperAdmin: full CRUD | — | All actions work | — | |
| 12 | Admin: verify allowed actions | Role logic | No permission errors | — | |

**Bugs Found (Module 3):**

---

### Module 4: Orders (`/orders`)

**Routes:** `/orders`, `/orders/create`, `/orders/:id`, `/orders/:orderId/upload`  
**Access:** All (role-based data)

#### 4.1 Order List

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/orders` as Client | Only own orders | API returns client orders | List correct | |
| 2 | Open as Designer | Only assigned orders | API returns assigned | List correct | |
| 3 | Open as Admin/SuperAdmin | All orders | API returns all | List correct | |
| 4 | Filter by status | Filter applied | Results update | Filter works | |
| 5 | Search orders | Search logic | Results update | Search works | |
| 6 | Pagination | — | Page changes | Correct data | |

#### 4.2 Create Order (Client only)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/orders/create` as Client | — | Form loads | Form visible | |
| 2 | Submit with required fields empty | Validation | No submit | Errors shown | |
| 3 | Submit valid order | — | Order created | Redirect to detail | |
| 4 | Designer/Admin tries create | — | Redirect or no access | — | |

#### 4.3 Order Detail (`/orders/:id`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open order as Client (own) | Full view | Data loads | All sections visible | |
| 2 | Open order as Designer (assigned) | Designer view | Data loads | Correct actions | |
| 3 | Open order as Admin | Full admin view | Data loads | Assign, status, etc. | |
| 4 | Assign designer | — | POST assign succeeds | Designer updated | |
| 5 | Update status | — | PUT status succeeds | Status updated | |
| 6 | Request price approval | — | API succeeds | Client notified | |
| 7 | Approve price (as Client) | — | API succeeds | Status changes | |
| 8 | Add comment | — | Comment saved | Comment appears | |
| 9 | Request revision | — | Revision requested | Status/log updated | |
| 10 | Approve logo | — | API succeeds | Order completed | |
| 11 | Cancel order | — | Order cancelled | Status updated | |
| 12 | Archive order (Admin) | — | Order archived | Hidden from default list | |
| 13 | Refund order | — | Refund processed | Status updated | |
| 14 | Edit order (dialog) | — | PUT succeeds | Changes saved | |

#### 4.4 File Upload (`/orders/:orderId/upload`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open upload page | — | Page loads | Upload area visible | |
| 2 | Upload single file | — | File uploaded | File appears in list | |
| 3 | Upload multiple files | — | All uploaded | All appear | |
| 4 | Upload invalid type | Validation | Rejected | Error message | |
| 5 | Upload oversized file | — | Rejected | Error message | |
| 6 | Download uploaded file | — | Download works | File downloads | |

**Bugs Found (Module 4):**

---

### Module 5: Designers (`/designers`)

**Routes:** `/designers`, `/designers/:id`  
**Access:** SuperAdmin, Admin

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/designers` as Admin | — | Designer list loads | Table/cards visible | |
| 2 | Designer/Client access | RoleGuard or data | No list or redirect | — | |
| 3 | Click designer row/card | — | Navigate to detail | Detail page loads | |
| 4 | Designer detail shows orders | — | Assigned orders | Correct data | |
| 5 | Search/filter designers | — | Results update | — | |

**Bugs Found (Module 5):**

---

### Module 6: Clients (`/clients`)

**Routes:** `/clients`, `/clients/:id`  
**Access:** SuperAdmin, Admin

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/clients` | — | Client list loads | Table visible | |
| 2 | Open client detail | — | Detail loads | Orders, invoices, files, messages | |
| 3 | Tabs: Orders, Invoices, Files, Messages | — | Data loads per tab | No errors | |
| 4 | Search/filter | — | Results update | — | |

**Bugs Found (Module 6):**

---

### Module 7: Projects (`/projects`)

**Routes:** `/projects`, `/projects/:id`  
**Access:** All authenticated

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/projects` | — | Project list loads | List visible | |
| 2 | Open project detail | — | Detail loads | Files, messages, status | |
| 3 | Upload file | — | Upload works | File appears | |
| 4 | Send message | — | Message sent | Message appears | |
| 5 | Update status | — | Status updated | — | |

**Bugs Found (Module 7):**

---

### Module 8: Files (`/files`)

**Route:** `/files`  
**Access:** All authenticated

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/files` | — | File list loads | List/table visible | |
| 2 | Filter by order | — | Filtered results | Correct files | |
| 3 | Filter by user | — | Filtered results | Correct files | |
| 4 | Filter by status | — | Filtered results | — | |
| 5 | Download file | — | Download works | File downloads (correct URL) | |
| 6 | Approve file (Admin) | — | File approved | Status updated | |
| 7 | Delete file | — | File deleted | Removed from list | |

**Bugs Found (Module 8):**

---

### Module 9: Invoices (`/invoices`)

**Route:** `/invoices`  
**Access:** All authenticated

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/invoices` | — | Invoice list loads | List visible | |
| 2 | Create invoice (Admin) | — | Invoice created | Success | |
| 3 | Edit invoice | — | Changes saved | — | |
| 4 | Send invoice | — | Email sent | Success message | |
| 5 | Mark paid | — | Status updated | Paid indicator | |
| 6 | Download PDF | — | PDF downloads | Valid PDF | |
| 7 | Statistics | — | Stats load | Correct numbers | |
| 8 | Report download | — | Report PDF | Valid PDF | |

**Bugs Found (Module 9):**

---

### Module 10: Permissions (`/permissions`)

**Route:** `/permissions`  
**Access:** SuperAdmin only

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open as SuperAdmin | — | Permission list loads | Roles, permissions visible | |
| 2 | Open as Admin | RoleGuard blocks | 403 or redirect | No access | |
| 3 | Assign permission to role | — | POST succeeds | Permission assigned | |
| 4 | Revoke permission | — | DELETE succeeds | Permission removed | |
| 5 | UI reflects changes | — | — | Checkboxes/state correct | |

**Bugs Found (Module 10):**

---

### Module 11: Settings (`/settings`)

**Route:** `/settings`  
**Access:** All (role-based sections)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/settings` | — | Settings load | Tabs/sections visible | |
| 2 | Business settings | — | Save works | Success | |
| 3 | Brand settings | — | Save works | — | |
| 4 | Logo upload | — | Logo uploaded | Preview updates | |
| 5 | Invoice template | — | Save works | — | |
| 6 | Payment methods | — | Save works | — | |
| 7 | Notifications | — | Preferences saved | — | |
| 8 | Change password | — | Password updated | Success, re-login if needed | |
| 9 | Admin vs SuperAdmin | Role logic | Correct sections visible | — | |

**Bugs Found (Module 11):**

---

### Module 12: Messages (`/messages`)

**Route:** `/messages`  
**Access:** All authenticated

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/messages` | — | Message list loads | List visible | |
| 2 | Mark as read | — | Status updated | Unread indicator clears | |
| 3 | Filter/search | — | Results update | — | |

**Bugs Found (Module 12):**

---

### Module 13: Reviews (`/reviews`)

**Route:** `/reviews`  
**Access:** SuperAdmin, Admin

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/reviews` | — | Review list loads | List visible | |
| 2 | Create review (Client, from order) | — | Review created | — | |
| 3 | Filter by order | — | Results update | — | |

**Bugs Found (Module 13):**

---

### Module 14: Notifications (`/notifications`)

**Route:** `/notifications`  
**Access:** All authenticated

#### 14.1 Notification Creation Scenarios (Backend)

Test that notifications are created for the correct users when these events occur:

| # | Scenario | Actor | Recipient | Expected Notification | Pass/Fail |
|---|----------|-------|------------|------------------------|-----------|
| 1 | **Client creates order** | Client | All Admin + SuperAdmin | "New Order Submitted" – order title, waiting for approval | |
| 2 | **Admin requests price approval** | Admin | Client (order owner) | "Price Approval Required" – proposed price | |
| 3 | **Admin approves order** | Admin | Client | "Order Approved" – order in progress | |
| 4 | **Admin sends preview files** | Admin | Client | "Preview Files Available" – files ready for review | |
| 5 | **Admin changes order status** | Admin | Client | "Order Status Updated" – new status | |

#### 14.2 Top Navigation Bell Icon

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Login, check bell icon in navbar | — | Bell visible | Icon visible, no errors | |
| 2 | Unread count badge | Shows when count > 0 | Badge displays number | Correct count | |
| 3 | Click bell | Opens dropdown | Panel opens | Latest 5 notifications shown | |
| 4 | Click "View All" | Expands in place | Loads all notifications | Dropdown expands with scrollbar | |
| 5 | Click "Show Less" | Collapses | Back to 5 | Dropdown shrinks | |
| 6 | Click notification | — | Marks read, navigates to order if linked | — | |
| 7 | "Open full Notifications page" link (when expanded) | — | Navigates to `/notifications` | — | |

#### 14.3 Notifications Page (`/notifications`)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/notifications` | — | Notification list loads | List visible | |
| 2 | Unread count in header | — | Badge shows count | Correct number | |
| 3 | Mark single as read | — | Status updated | Unread indicator clears | |
| 4 | Mark all as read | — | All marked | Badge clears | |
| 5 | Toggle "Show Unread Only" | — | Filters list | Only unread shown | |
| 6 | Pagination | > 20 notifications | Pages work | Prev/Next, page info | |
| 7 | Click notification row | — | Marks read, navigates to order if linked | — | |
| 8 | Sidebar | — | "Notifications" menu item visible | pi pi-bell icon | |

#### 14.4 Dashboard Notifications Panel (Client only)

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Login as Client, open dashboard | — | Notifications panel visible | Panel with "View All" | |
| 2 | Unread badge in panel header | — | Shows count when > 0 | "X new" badge | |
| 3 | Click "View All Notifications" | — | Navigates to `/notifications` | — | |

#### 14.5 End-to-End Flow: Client Creates Order → Admin Sees Notification

| # | Step | Expected Result | Pass/Fail |
|---|------|------------------|-----------|
| 1 | Login as **Client** | — | |
| 2 | Create a new order (any title) | Order created | |
| 3 | Logout (or use second browser) | — | |
| 4 | Login as **Admin** or **SuperAdmin** | — | |
| 5 | Wait up to 30s OR click bell icon | Unread count badge appears | |
| 6 | Click bell | Dropdown shows "New Order Submitted" | |
| 7 | Click notification | Navigates to order detail | |

#### 14.6 End-to-End Flow: Admin Actions → Client Sees Notification

| # | Step | Expected Result | Pass/Fail |
|---|------|-----------------|-----------|
| 1 | Admin: Request price approval on order | — | |
| 2 | Client: Login, check bell | "Price Approval Required" | |
| 3 | Admin: Approve order | — | |
| 4 | Client: Check bell | "Order Approved" | |
| 5 | Admin: Send preview files to client | — | |
| 6 | Client: Check bell | "Preview Files Available" | |

#### 14.7 Polling & Global Count

| # | Step | Expected Result | Pass/Fail |
|---|------|-----------------|-----------|
| 1 | Admin logged in, bell shows 0 | — | |
| 2 | Client creates order (in another tab/session) | — | |
| 3 | Wait 30–60 seconds (no click) | Admin bell badge updates | |
| 4 | Mark all as read | Badge updates to 0 | |

#### 14.8 Email Notifications (Optional)

| # | Step | Expected Result | Pass/Fail |
|---|------|-----------------|-----------|
| 1 | Settings → Notifications → Enable "Email Notifications" | — | |
| 2 | Trigger event (e.g., client creates order) | Admin receives email | |
| 3 | Disable email notifications | No email sent | |

**Bugs Found (Module 14):**

---

### Module 15: Gallery (`/gallery`)

**Route:** `/gallery`  
**Access:** Client only

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Login as Client, open `/gallery` | — | Gallery loads | Approved logos visible | |
| 2 | Login as Admin, open `/gallery` | RoleGuard blocks | 403 or redirect | No access | |
| 3 | Gallery items display | — | Images load | Correct images | |
| 4 | Download/view item | — | Works | — | |

**Bugs Found (Module 15):**

---

### Module 16: Analytics (`/analytics`)

**Route:** `/analytics`  
**Access:** All authenticated

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/analytics` | — | Page loads | Charts visible | |
| 2 | Charts render | Data correct | API returns data | No console errors | |
| 3 | Date range filter | — | Data updates | Charts refresh | |

**Bugs Found (Module 16):**

---

### Module 17: Financial (`/financial`)

**Route:** `/financial`  
**Access:** All authenticated

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Open `/financial` | — | Page loads | Financial overview visible | |
| 2 | Data displays | Correct totals | API returns data | Numbers correct | |
| 3 | Filters | — | Data updates | — | |

**Bugs Found (Module 17):**

---

### Module 18: Layout & Navigation

| # | Step | Logic | Functionality | UI | Pass/Fail |
|---|------|-------|---------------|-----|-----------|
| 1 | Sidebar menu | Role-based items | Correct links per role | No 403 on click | |
| 2 | Breadcrumbs | — | Update on navigation | Correct path | |
| 3 | Global search | — | Search works | Results display | |
| 4 | User menu (logout, profile) | — | Actions work | — | |
| 5 | Responsive sidebar | — | Collapses on mobile | Hamburger menu | |
| 6 | Unauthenticated access to protected route | AuthGuard | Redirect to login | — | |

**Bugs Found (Module 18):**

---

## 5. Resolution Workflow

### Step 1: Complete One Module

1. Run all test steps for the module.
2. Mark Pass/Fail for each step.
3. Document every bug using the [Bug Reporting Template](#3-bug-reporting-template).

### Step 2: Prioritize Bugs

| Priority | Criteria |
|----------|----------|
| **Critical** | App crash, data loss, security issue, blocks core flow |
| **High** | Core feature broken, wrong calculations, wrong data |
| **Medium** | Feature works with workaround, minor logic error |
| **Low** | Cosmetic, typo, minor UX |

### Step 3: Fix Module Bugs

1. Fix Critical and High bugs first.
2. Re-run the module test steps after each fix.
3. Mark steps as Pass when fixed.
4. Move to Medium and Low when Critical/High are done.

### Step 4: Regression Check

After fixing a module:

- [ ] Re-run full module checklist.
- [ ] Spot-check related modules (e.g., Orders after Files).
- [ ] Verify no new console/API errors.

### Step 5: Move to Next Module

1. Mark module as "Tested & Resolved" when all Critical/High bugs are fixed.
2. Proceed to the next module in order.
3. At the end, run a full regression across all modules.

---

## Appendix: Quick Reference

### Suggested Testing Order

1. Auth  
2. Dashboard  
3. Users  
4. Orders (including Create, Detail, Upload)  
5. Designers  
6. Clients  
7. Projects  
8. Files  
9. Invoices  
10. Permissions  
11. Settings  
12. Messages  
13. Reviews  
14. Notifications  
15. Gallery  
16. Analytics  
17. Financial  
18. Layout & Navigation  

### Known Potential Gaps (from codebase analysis)

- Designer detail route `/designers/:id` — verify route exists and works.
- File download URL — ensure no hardcoded `localhost:5000`; use environment config.
- Audit logs — backend API exists; frontend UI may be missing.
- Payments module — verify routing and UI if separate from Invoices.

---

*End of QA Testing Guideline*
