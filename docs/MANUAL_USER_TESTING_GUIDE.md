# Hawk Merchandising Web Portal — Manual testing guide (non-technical)

This guide helps **business users and QA testers** walk through each area of the web portal **without writing code**. Use it to confirm that screens load, buttons work, and main workflows behave as expected.

---

## 1. Before you start

| Item | What to do |
|------|------------|
| **Browser** | Use a current version of Chrome, Edge, or Firefox. |
| **Address** | Open the portal URL your team gave you (for local development this is often `http://localhost:4200`). |
| **Accounts** | You need test logins for each **role** you will check: **Super Admin**, **Admin**, **Client**, and **Designer**. Ask your administrator for these; do not use real customer passwords. |
| **Backend** | The site needs the **API server** running. If pages stay blank or show errors, ask IT whether the backend is up. |
| **How to record results** | For each step, mark **Pass**, **Fail**, or **N/A** (not available for your role) and note what you saw if something failed. |

**Words used in this guide**

- **Click** = single left-click with the mouse (or tap on a phone/tablet).
- **Sidebar** = menu on the left with links such as Dashboard and Orders.
- **Modal / dialog** = a panel that pops up over the page; you usually close it with **Cancel**, **X**, or after **Save**.

---

## 2. Who can see what (roles)

The menu changes by role. Use the right account for each section.

| Area | Super Admin | Admin | Client | Designer |
|------|-------------|-------|--------|----------|
| Dashboard | Yes | Yes | Yes | Yes |
| Orders (all vs mine) | All orders | All orders | My orders | Assigned orders |
| Quotes | Yes | Yes | Yes (My Quotes) | No |
| Users | Yes | Yes | No | No |
| Clients | Yes | Yes | No | No |
| Projects (Logo Projects) | Yes | Yes | Yes (My Projects) | Yes (My Projects) |
| Designers | Yes | Yes | No | No |
| Messages | Yes | Yes | Yes | Yes |
| Reviews | Yes | Yes | No | No |
| Files | Yes | Yes | Yes | Yes |
| Detail Analytics | Yes | Yes | Yes | No |
| Client Intelligence | Yes | Yes | No | No |
| Invoices | Yes | Yes | Yes | No |
| Financial Overview | Yes | Yes | Yes | No |
| Designer Payout | Yes | Yes | No | Yes |
| Client Pricing | Yes | Yes | No | No |
| Designer Pricing | Yes | Yes | No | No |
| Permissions | Yes | No | No | No |
| Notifications | Yes | Yes | Yes | Yes |
| Settings | Yes | Yes | Yes | Yes |
| Gallery (full page) | No | No | Yes (via URL `/gallery` — see §15) | No |

If you open a page and are sent back to the login screen or dashboard, your role may not have access; mark **N/A** and continue with an appropriate account.

---

## 3. Signing in and signing out

### 3.1 Login — happy path

1. Open the portal address. If you are not logged in, you should see **Hawk Merchandising Web Portal** and a **Login** form.
2. Enter your **email** and **password**.
3. Click **Login**.
4. **Expected:** You reach the **Dashboard** and see your name (or user menu) in the top bar.

### 3.2 Login — wrong password

1. On the login page, enter a valid email but a **wrong** password.
2. Click **Login**.
3. **Expected:** An error message appears (red text); you stay on the login page.

### 3.3 Open login without being logged in

1. While logged out, try to open the dashboard by changing the address to end with `/dashboard` (your team can give the exact link).
2. **Expected:** You are redirected to the login page.

### 3.4 Register (if enabled)

1. On the login page, click **Register here** (or open `/auth/register`).
2. Fill in the form as instructed by your team.
3. Submit.
4. **Expected:** Success message or redirect as your process defines; if registration is disabled, you may see an error — note it.

### 3.5 Forgot password

1. On the login page, click **Forgot your password?**
2. Enter the email your team specifies for testing.
3. Submit.
4. **Expected:** Confirmation that reset instructions were sent (or your team’s expected behavior).

### 3.6 Logout

1. While logged in, click your **name or avatar** in the top bar to open the user menu.
2. Click **Logout**.
3. **Expected:** You return to the login page; opening `/dashboard` again should ask you to log in.

---

## 4. Layout and navigation (all roles)

1. Confirm the **sidebar** shows menu items matching your role (see §2).
2. Click **Dashboard**. **Expected:** Page title **Dashboard** and welcome text.
3. Click another item (e.g. **Orders**), then **Dashboard** again. **Expected:** Pages switch without errors.
4. If your role has it, type a short word in the **global search** (top) and run the search. **Expected:** You are taken to **Orders** with a search applied (behavior may vary slightly by role).
5. Open **Notifications** from the sidebar (or bell, if your layout uses it). **Expected:** **Notifications** page with a **Back** control and optional **Mark All Read** when there are unread items.

---

## 5. Dashboard

**Who:** All roles.

1. Open **Dashboard**.
2. **Expected:** Loading finishes; you see summary content (cards, lists, or actions) appropriate to your role.
3. **Client only:** If you see **Create New Order** and **Get a Quote**, click each once. **Expected:** A dialog opens (you can cancel after confirming it opens).
4. **Client only:** Scroll to any **gallery** or **recent orders** section. **Expected:** Sections load or show an empty state (not an endless spinner).
5. **Admin / Super Admin:** Use quick buttons (**Users**, **Orders**, **Designers**, **Permissions**, **Files**) if shown. **Expected:** Each navigates to the right page.

---

## 6. Orders

Titles: **My Orders** (Client), **Orders** (Designer), **Orders Management** (Admin / Super Admin).

### 6.1 Open the orders page

1. Click **Orders** or **My Orders** in the sidebar.
2. **Expected:** Page header matches your role; list or empty state appears after loading.

### 6.2 Summary cards and filters (when orders exist)

1. If you see **KPI / summary cards**, click one if they look clickable.
2. Use **quick filter chips** if present.
3. **Expected:** The table updates or filters without errors.

### 6.3 Create order (Client, or roles allowed to create)

1. Click **Create Order** (or **Create New Order** from the dashboard).
2. **Expected:** Dialog titled **Create New Order**.
3. Enter a **title** and **description** (use test text your team allows).
4. Submit / save.
5. **Expected:** Dialog closes; new row appears or list refreshes.

### 6.4 Get a Quote from Orders (Client)

1. Click **Get a Quote**.
2. **Expected:** Quote-related dialog or flow opens.

### 6.5 Add Completed Order (if button visible)

1. Click **Add Completed Order** when your role shows it.
2. **Expected:** Dialog opens; complete or cancel per your test data rules.

### 6.6 Open one order

1. Click a row or order title in the table.
2. **Expected:** Order **detail** dialog or page opens with information and role-specific actions (e.g. **Approve Order** for admin on pending orders).

### 6.7 Admin: approve (when applicable)

1. Open an order that should allow approval.
2. Click **Approve Order** (or equivalent).
3. **Expected:** Status updates or message confirms success.

### 6.8 Designer

1. As Designer, open **Orders**.
2. **Expected:** You only see work **assigned to you**; opening an order shows actions you are allowed (deliver preview, etc.) per your process.

---

## 7. Quotes

**Who:** Super Admin, Admin, Client.  
**Page title:** **Quotes** — *Request and manage logo quotes*.

1. Open **Quotes** or **My Quotes**.
2. **Expected:** List or empty state loads.
3. Open one quote if any exist.
4. **Expected:** Detail view **Quote: …** with subtitle *Review response and decide next action* (exact title includes the logo name).
5. Perform the actions your team cares about (accept, decline, message — whatever the UI offers) and confirm the screen updates.

---

## 8. Users

**Who:** Super Admin, Admin.  
**Page title:** **Users Management**.

1. Open **Users**.
2. **Expected:** Table of users and **Create User** if you have permission.
3. Click **Create User**.
4. **Expected:** **Create New User** dialog.
5. Fill email, first name, last name, password, choose **role** (e.g. Designer or Client). For **Client**, complete any extra fields shown (company, phone, etc.).
6. Submit.
7. **Expected:** Dialog closes; new user appears in the table (you may need to search or go to the next page).

---

## 9. Clients

**Who:** Super Admin, Admin.  
**Page title:** **Clients Management**.

1. Open **Clients**.
2. **Expected:** Table with search; data loads or empty state.
3. Use **Search clients** and type part of a name or email.
4. **Expected:** Table filters.
5. Open a client row if available. **Expected:** Detail view with related information.

---

## 10. Logo Projects

**Who:** All roles that have **Projects** or **My Projects** in the menu.  
**Page title:** **Logo Projects**.

1. Open **Projects** / **My Projects**.
2. **Expected:** Table with search; loading completes.
3. Open one project. **Expected:** **Project** detail shows without errors.

---

## 11. Designers

**Who:** Super Admin, Admin.  
**Page title:** **Designers**.

1. Open **Designers**.
2. **Expected:** List loads; search works if present.
3. Open one designer. **Expected:** Detail page with profile information.

---

## 12. Messages

**Who:** Super Admin, Admin, Client, Designer.  
**Page title:** **Messages**.

1. Open **Messages**.
2. **Expected:** Table and search; unread badge in header if you have unread items.
3. Open a message thread or row. **Expected:** Content displays; sending a reply (if available) follows your team’s test script.

---

## 13. Reviews and feedback

**Who:** Super Admin, Admin.  
**Page title:** **Reviews & Feedback**.

1. Open **Reviews**.
2. **Expected:** Table with search for clients/comments.
3. Open a review if present. **Expected:** Details display correctly.

---

## 14. Files

**Who:** All roles with **Files** in the menu.  
**Page title:** **Files Management**.

1. Open **Files**.
2. **Expected:** Subtitle matches your role (upload/view vs oversight for Super Admin).
3. Click **Grid View** / **Table View** if the button is shown.
4. **Expected:** Layout switches.
5. **Super Admin:** Use the search box if shown; **Expected:** Results update.
6. Upload or download only as instructed by your team (to avoid affecting production data).

---

## 15. Gallery (Clients)

**Who:** Client only. The full-page gallery may not appear in the sidebar; clients often see gallery items on the **Dashboard** as well.

1. As Client, open: `…/gallery` (your full site URL + `/gallery`).
2. **Expected:** **My Gallery** heading and *All your approved final files*; grid or *No files in gallery yet*.
3. If items exist: click an item; use **preview** and **download** if shown. **Expected:** Preview or download works.

---

## 16. Detail Analytics

**Who:** Super Admin, Admin, Client.

1. Open **Detail Analytics**.
2. **Expected:** **Detail Analytics** title; **Client** sees usage sections (e.g. Usage Overview, order counts); **Admin** sees broader analytics. Wait for spinners to finish.
3. Scroll through sections. **Expected:** No blank broken areas; charts or numbers appear or show zero.

---

## 17. Client Intelligence

**Who:** Super Admin, Admin.  
**Page title:** **Client Intelligence**.

1. Open **Client Intelligence**.
2. Click tab **Overview**. **Expected:** Dashboard-style panels load.
3. Click tab **Churn Prediction**. **Expected:** Churn-related content loads.
4. **Expected:** Switching tabs does not freeze the page.

---

## 18. Invoices

**Who:** Super Admin, Admin, Client.  
**Page title:** **Invoices Management**.

1. Open **Invoices**.
2. **Expected:** List or empty state; subtitle *Track payments and manage invoices*.
3. Open an invoice if listed. **Expected:** Detail view is readable.
4. If your team uses it, find **Flexible Invoice Builder** (may be linked from invoices area — *Generate invoice by date range or custom order selection*). Walk through opening the screen and canceling if you must not create real invoices.

---

## 19. Financial Overview

**Who:** Super Admin, Admin, Client.

1. Open **Financial Overview** (under **Financial** in the menu).
2. **Expected:** **Financial Overview** page loads after any loading indicator.

---

## 20. Designer Payout

**Who:** Super Admin, Admin (manage payouts); Designer (view own payouts).

1. Open **Designer Payout**.
2. **Expected:** **Designer Payout** title; subtitle differs for admin vs designer.
3. **Admin:** Check summary cards (e.g. pending designers, eligible orders) if data exists.
4. **Designer:** Confirm you only see your own payout information.

---

## 21. Client Logo Pricing

**Who:** Super Admin, Admin.  
**Screen title:** **Client Logo Pricing**.

1. Open **Client Pricing**.
2. **Expected:** **Select Client** dropdown and pricing grid or form.
3. Select a test client (ask your team which to use). **Expected:** Rules load or show empty.
4. Change a value only if your test plan allows; otherwise stop after read-only checks.

---

## 22. Designer Logo Pricing

**Who:** Super Admin, Admin.  
**Screen title:** **Designer Logo Pricing**.

1. Open **Designer Pricing**.
2. **Expected:** **Select Designer** dropdown.
3. Select a test designer. **Expected:** Pricing table loads.
4. Edit only per test policy.

---

## 23. Permissions (Super Admin only)

**Page title:** **Permissions Management**.

1. Log in as **Super Admin**.
2. Open **Permissions**.
3. **Expected:** Table of permissions with search.
4. Click **Permission Matrix** (or **Hide Matrix** after it opens). **Expected:** Matrix view toggles.

---

## 24. Settings

**Who:** All roles with access.

1. Open **Settings**.
2. **Expected:** **Settings** with tabs such as **Business Info** (and others your build includes).
3. Open each tab. **Expected:** Forms load without errors.
4. Change a non-critical field in a **test** environment only; click save if available. **Expected:** Success message or updated value.

---

## 25. End-to-end order flow (multi-person checklist)

Use separate browsers or private windows for **Client**, **Admin**, and **Designer** so sessions do not overwrite each other. Adjust steps to match your business rules.

| Step | Actor | Action | Expected |
|------|--------|--------|----------|
| 1 | Client | Create a new order with title + description | Order appears in **My Orders** |
| 2 | Admin | Open **Orders Management**, open the new order, **Approve** | Status moves forward |
| 3 | Admin | Assign a designer (if your UI has this in the order detail) | Designer sees the order under **Orders** |
| 4 | Designer | Open order, deliver preview / update status per your workflow | Client can see update |
| 5 | Client | Request revision (if applicable) | Order shows revision state |
| 6 | Designer | Deliver updated preview | Client sees new version |
| 7 | Client | Approve final logo (if button exists) | Order progresses |
| 8 | Admin | Mark order **Completed** (if that step is admin-only) | Status **Completed** |
| 9 | Admin | Create invoice for that order (Invoices / builder) | Invoice exists and links to order |

If any step is not available in the UI, note the step number and ask the product owner whether the workflow changed.

---

## 26. Quick pass/fail checklist (by module)

Copy this table into your test report and fill it in.

| Module | Role used | Pass / Fail / N/A | Notes |
|--------|-----------|-------------------|-------|
| Login / Logout | | | |
| Register | | | |
| Forgot password | | | |
| Dashboard | | | |
| Orders | | | |
| Quotes | | | |
| Users | | | |
| Clients | | | |
| Logo Projects | | | |
| Designers | | | |
| Messages | | | |
| Reviews | | | |
| Files | | | |
| Gallery (Client) | | | |
| Detail Analytics | | | |
| Client Intelligence | | | |
| Invoices | | | |
| Financial Overview | | | |
| Designer Payout | | | |
| Client Pricing | | | |
| Designer Pricing | | | |
| Permissions | | | |
| Notifications | | | |
| Settings | | | |

---

## 27. When something goes wrong

- **Spinning loader forever:** Note the page name and time; ask IT if the API is slow or down.
- **“Unauthorized” or blank after login:** Wrong role for that URL, or session expired — try logout and login again.
- **Red error text on forms:** Read the message; often a required field is missing or format is wrong (email, phone).
- **404 or wrong page:** Confirm the full URL with your team; bookmarks may be outdated.

---

*Document version: aligned with the Web Portal frontend routes and menus. For API-only behavior, see technical test plans under `docs/` (e.g. E2E plans).*
