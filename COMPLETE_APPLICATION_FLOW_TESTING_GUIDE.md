# 🧪 Complete Application Flow Testing Guide

## 📋 **Overview**

This comprehensive guide tests the **complete end-to-end flow** of the Logo Design Portal application, covering all implemented functionality from initial setup through the full order lifecycle. Follow this guide step-by-step to verify all features work correctly.

---

## 🚀 **Prerequisites & Setup**

### **1. Start Backend Server**

```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```

✅ **Wait for:** `Now listening on: http://localhost:5000` or `https://localhost:5001`

### **2. Start Frontend Application**

```bash
cd Frontend
npm start
```

✅ **Wait for:** `Angular Live Development Server is listening on localhost:4200`

### **3. Open Application**

Navigate to: `http://localhost:4200`

---

## 👤 **Test Users Setup**

### **Default SuperAdmin**
- **Email:** `superadmin@logodesign.com`
- **Password:** `SuperAdmin@123`
- **Role:** SuperAdmin

### **Test Users to Create**
We'll create these during testing:
- **Client User:** `client@test.com`
- **Designer User:** `designer@test.com`
- **Admin User:** `admin@test.com`

---

## 📖 **Complete Testing Flow**

## **PHASE 1: Initial Setup & Authentication**

### **Test 1.1: Application Access**

**Steps:**
1. Open browser to `http://localhost:4200`
2. Verify application loads
3. Check if redirected to login page (if not authenticated)

**Expected Results:**
- ✅ Application loads without errors
- ✅ Login page displays
- ✅ No console errors

---

### **Test 1.2: User Registration (Client)**

**Steps:**
1. Navigate to `/register` or click "Register" link
2. Fill registration form:
   - **Email:** `client@test.com`
   - **Password:** `Test123!@#`
   - **First Name:** `John`
   - **Last Name:** `Doe`
   - **Company Name:** `Test Corporation`
   - **Phone Number:** `+1234567890`
3. Click "Register" button

**Expected Results:**
- ✅ Success message appears
- ✅ Redirected to login page
- ✅ User can login with new credentials

**Verification:**
- [ ] Registration form validates correctly
- [ ] Success message displays
- [ ] Redirects to login
- [ ] Can login with new credentials

---

### **Test 1.3: Login (SuperAdmin)**

**Steps:**
1. Navigate to `/login`
2. Enter credentials:
   - **Email:** `superadmin@logodesign.com`
   - **Password:** `SuperAdmin@123`
3. Click "Login"

**Expected Results:**
- ✅ Redirects to `/dashboard`
- ✅ Sidebar navigation appears
- ✅ Top navbar shows user name and role
- ✅ No authentication errors

**Verification:**
- [ ] Login successful
- [ ] Dashboard loads
- [ ] Navigation menu visible
- [ ] User info displayed in navbar

---

## **PHASE 2: User Management (SuperAdmin)**

### **Test 2.1: View Users List**

**Steps:**
1. Login as SuperAdmin
2. Navigate to `/users` (click "Users" in sidebar)
3. Verify users table displays

**Expected Results:**
- ✅ Users table shows all users
- ✅ Columns: Name, Email, Role, Status, Created Date
- ✅ Search functionality works
- ✅ Role badges display with colors
- ✅ Pagination works (if many users)

**Verification:**
- [ ] Users list displays
- [ ] Search filters correctly
- [ ] Role badges show correct colors
- [ ] Table is responsive

---

### **Test 2.2: Create Designer User**

**Steps:**
1. In Users page, click "Create User" button
2. Fill form:
   - **Email:** `designer@test.com`
   - **Password:** `Designer123!@#`
   - **First Name:** `Jane`
   - **Last Name:** `Designer`
   - **Role:** Select "Designer"
   - **Specialization:** `Logo Design, Brand Identity`
   - **Bio:** `Experienced logo designer with 5+ years`
   - **Hourly Rate:** `50.00`
   - **Availability:** `Available`
3. Click "Create" button

**Expected Results:**
- ✅ Success message appears
- ✅ New user appears in users list
- ✅ Designer profile created
- ✅ Can login with new credentials

**Verification:**
- [ ] User created successfully
- [ ] Designer profile saved
- [ ] Appears in users list
- [ ] Can login as designer

---

### **Test 2.3: Create Admin User**

**Steps:**
1. Click "Create User" again
2. Fill form:
   - **Email:** `admin@test.com`
   - **Password:** `Admin123!@#`
   - **First Name:** `Admin`
   - **Last Name:** `User`
   - **Role:** Select "Admin"
3. Click "Create"

**Expected Results:**
- ✅ Admin user created
- ✅ Appears in users list
- ✅ Can login as admin

**Verification:**
- [ ] Admin user created
- [ ] Can login with admin credentials

---

## **PHASE 3: Dashboard Testing**

### **Test 3.1: SuperAdmin Dashboard**

**Steps:**
1. Login as SuperAdmin
2. Navigate to `/dashboard`
3. Verify all dashboard components

**Expected Results:**

**Statistics Cards:**
- ✅ Total Orders (clickable → navigates to orders)
- ✅ Pending Orders (clickable)
- ✅ In Progress Orders (clickable)
- ✅ Completed Orders (clickable)
- ✅ Total Clients (clickable → navigates to clients)
- ✅ New Clients This Month (clickable)
- ✅ Total Revenue (currency formatted, clickable → navigates to invoices)
- ✅ Average Delivery Time (in days)

**Charts:**
- ✅ **Orders by Status** (Doughnut Chart)
  - Shows breakdown of order statuses
  - Color-coded segments
  - Hover shows percentages
- ✅ **Orders Trend** (Line Chart - Last 6 Months)
  - Shows monthly order counts
  - Smooth line with data points
  - Responsive design
- ✅ **Revenue by Package** (Bar Chart - Admin/SuperAdmin only)
  - Revenue breakdown by package type
  - Currency formatted tooltips

**Recent Orders Table:**
- ✅ Shows last 10 orders
- ✅ Columns: Title, Status, Created Date, Due Date
- ✅ Status badges with colors
- ✅ Clickable rows → navigate to order detail
- ✅ "View All" button works

**Quick Actions:**
- ✅ Navigate to Users
- ✅ Navigate to Orders
- ✅ Navigate to Designers
- ✅ Navigate to Permissions (SuperAdmin only)

**Verification:**
- [ ] All statistics cards display
- [ ] Charts render (when data exists)
- [ ] Clickable cards navigate correctly
- [ ] Recent orders table shows data
- [ ] Empty states show when no data

---

### **Test 3.2: Client Dashboard**

**Steps:**
1. Logout from SuperAdmin
2. Login as Client (`client@test.com` / `Test123!@#`)
3. Navigate to `/dashboard`

**Expected Results:**
- ✅ Client-specific statistics only
- ✅ "New Order" button visible
- ✅ My Orders section
- ✅ Order status breakdown
- ✅ No admin charts (Revenue by Package hidden)

**Verification:**
- [ ] Only client data shown
- [ ] Can create order from dashboard
- [ ] Role-based data filtering works

---

### **Test 3.3: Designer Dashboard**

**Steps:**
1. Logout from Client
2. Login as Designer (`designer@test.com` / `Designer123!@#`)
3. Navigate to `/dashboard`

**Expected Results:**
- ✅ Assigned orders statistics
- ✅ Order status breakdown
- ✅ Recent assigned orders
- ✅ No admin/client specific data

**Verification:**
- [ ] Only assigned orders shown
- [ ] Can view assigned orders
- [ ] Role-based filtering works

---

## **PHASE 4: Order Management - Complete Workflow**

### **Test 4.1: Client Creates Order**

**Steps:**
1. Login as Client (`client@test.com`)
2. Navigate to `/orders` or click "New Order" from dashboard
3. Click "Create Order" button
4. Fill order form:
   - **Title:** `Modern Tech Logo Design`
   - **Description:** `Need a modern, minimalist logo for my tech startup`
   - **Price:** `500.00`
   - **Deadline:** Select date (e.g., 30 days from today)
   - **Instructions:** `Logo should be scalable, work in black and white, and represent innovation`
   - **Required Formats:** `PNG, SVG, PDF, AI`
   - **Requirements:** `Must include icon and wordmark versions`
   - **Color Preferences:** `Blue (#0066CC) and white`
   - **Style Preferences:** `Modern, minimalist, professional`
5. Click "Create Order" or "Submit"

**Expected Results:**
- ✅ Success message appears
- ✅ Order created with status: `WaitingForAdminApproval`
- ✅ Order appears in orders list
- ✅ Order visible to admin
- ✅ Client can see order in "My Orders"

**Verification:**
- [ ] Order created successfully
- [ ] Status is "WaitingForAdminApproval"
- [ ] Order appears in client's order list
- [ ] Order visible to admin

---

### **Test 4.2: Admin Views Pending Orders**

**Steps:**
1. Logout from Client
2. Login as Admin (`admin@test.com` / `Admin123!@#`)
3. Navigate to `/orders`
4. Verify orders list

**Expected Results:**
- ✅ All orders displayed
- ✅ Orders with status "WaitingForAdminApproval" visible
- ✅ Client information visible (but masked for Admin)
- ✅ Order includes all new fields (instructions, formats, etc.)
- ✅ Search and filter work
- ✅ Status filter works

**Verification:**
- [ ] Orders list displays
- [ ] Pending orders visible
- [ ] Client info masked (no email/phone for Admin)
- [ ] Search and filters work

---

### **Test 4.3: Admin Requests Price Approval (Optional)**

**Steps:**
1. In Orders page, find the order created by client
2. Click on order or use action menu
3. If "Request Price Approval" option exists:
   - Click "Request Price Approval"
   - Enter proposed price: `750.00`
   - Add notes: `Additional complexity requires higher price`
   - Click "Request"

**Expected Results:**
- ✅ Order status changes to `PriceApprovalPending`
- ✅ Notification sent to client
- ✅ Proposed price saved

**Verification:**
- [ ] Price approval request created
- [ ] Client receives notification
- [ ] Order status updated

---

### **Test 4.4: Client Approves Price (If Price Approval Requested)**

**Steps:**
1. Logout from Admin
2. Login as Client
3. Navigate to `/orders` or check notifications
4. Find order with price approval request
5. Click "Approve Price" or similar action
6. Approve the price

**Expected Results:**
- ✅ Price approved
- ✅ Order price updated to proposed price
- ✅ Status returns to `WaitingForAdminApproval`
- ✅ Admin notified

**Verification:**
- [ ] Price approved successfully
- [ ] Order price updated
- [ ] Status updated correctly

---

### **Test 4.5: Admin Approves Order**

**Steps:**
1. Logout from Client
2. Login as Admin
3. Navigate to `/orders`
4. Find the order (status: `WaitingForAdminApproval`)
5. Click "Approve Order" or change status to "InProgress"

**Expected Results:**
- ✅ Order status changes to `InProgress`
- ✅ Notification sent to client
- ✅ Status history recorded
- ✅ Order ready for designer assignment

**Verification:**
- [ ] Order approved successfully
- [ ] Status updated to "InProgress"
- [ ] Client receives notification
- [ ] Status history recorded

---

### **Test 4.6: Admin Assigns Order to Designer**

**Steps:**
1. In Orders page, find the approved order
2. Click "Assign Designer" icon/button (👤+)
3. In dialog:
   - Select designer from dropdown (`designer@test.com`)
   - Click "Assign"
4. Verify assignment

**Expected Results:**
- ✅ Success message appears
- ✅ Order shows assigned designer
- ✅ Designer can see order in their dashboard
- ✅ Designer cannot see client identity
- ✅ Status history recorded

**Verification:**
- [ ] Designer assigned successfully
- [ ] Order shows designer name
- [ ] Designer can access order
- [ ] Client info hidden from designer

---

### **Test 4.7: Designer Views Assigned Order**

**Steps:**
1. Logout from Admin
2. Login as Designer (`designer@test.com`)
3. Navigate to `/orders` or `/dashboard`
4. Verify assigned orders

**Expected Results:**
- ✅ Designer sees only assigned orders
- ✅ Client identity hidden (no name, email, phone)
- ✅ Order details visible (title, description, requirements)
- ✅ Can upload files
- ✅ Can view order details

**Verification:**
- [ ] Only assigned orders visible
- [ ] Client identity completely hidden
- [ ] Order details accessible
- [ ] Can navigate to order detail

---

### **Test 4.8: Designer Uploads Preview File**

**Steps:**
1. As Designer, navigate to order detail page
2. Go to "Files & Revisions" tab
3. Click "Upload Files" or use upload section
4. Select file (JPG, PNG, PDF, etc.)
5. Set file type: `Preview`
6. Add description: `Initial logo concept`
7. Click "Upload"

**Expected Results:**
- ✅ File uploads successfully
- ✅ File type saved as "Preview"
- ✅ File NOT visible to client (until admin approves)
- ✅ File visible to admin and designer
- ✅ Version number assigned
- ✅ Success message appears

**Verification:**
- [ ] File uploads successfully
- [ ] File type saved correctly
- [ ] File NOT visible to client
- [ ] File visible to admin/designer
- [ ] Version number assigned

---

### **Test 4.9: Admin Views All Files (Including Unapproved)**

**Steps:**
1. Logout from Designer
2. Login as Admin
3. Navigate to order detail page
4. Go to "Files & Revisions" tab

**Expected Results:**
- ✅ Admin sees ALL files (approved and unapproved)
- ✅ File details show: `IsVisibleToClient`, `IsAdminApproved`, `FileType`
- ✅ Can see designer-uploaded preview files
- ✅ File list shows all files

**Verification:**
- [ ] Admin sees all files
- [ ] Unapproved files visible to admin
- [ ] File metadata correct

---

### **Test 4.10: Client Views Files (Only Approved)**

**Steps:**
1. Logout from Admin
2. Login as Client
3. Navigate to order detail page
4. Go to "Files & Revisions" tab

**Expected Results:**
- ✅ Client sees ONLY approved/visible files
- ✅ Designer preview files NOT visible (until approved)
- ✅ Empty state or message if no approved files
- ✅ File list filtered correctly

**Verification:**
- [ ] Client sees only approved files
- [ ] Unapproved files hidden
- [ ] Empty state shows appropriately

---

### **Test 4.11: Admin Approves and Sends Files to Client**

**Steps:**
1. Logout from Client
2. Login as Admin
3. Navigate to order detail page
4. Go to "Files & Revisions" tab
5. Find unapproved preview file
6. Click "Approve File" or use action menu
7. Approve file and make visible to client
8. Or use "Send Files to Client" action

**Expected Results:**
- ✅ File approved (`IsAdminApproved` = true)
- ✅ File made visible to client (`IsVisibleToClient` = true)
- ✅ Order status changes to `PreviewDelivered`
- ✅ Notification sent to client
- ✅ Status history recorded

**Verification:**
- [ ] File approved successfully
- [ ] File becomes visible to client
- [ ] Order status updated
- [ ] Client receives notification

---

### **Test 4.12: Client Views Approved Files**

**Steps:**
1. Logout from Admin
2. Login as Client
3. Navigate to order detail page
4. Go to "Files & Revisions" tab

**Expected Results:**
- ✅ Approved files now visible
- ✅ Can download files
- ✅ File details display
- ✅ Preview images show (if applicable)

**Verification:**
- [ ] Approved files visible
- [ ] Can download files
- [ ] File details correct

---

### **Test 4.13: Client Requests Revision**

**Steps:**
1. As Client, in order detail page
2. Go to "Files & Revisions" tab or "Revisions" section
3. Click "Request Revision" button
4. Fill revision form:
   - **Instructions:** `Please make the logo more bold and change the color to darker blue`
   - Upload reference images (optional)
5. Click "Submit Revision"

**Expected Results:**
- ✅ Revision created
- ✅ Order status changes to `RevisionRequested`
- ✅ Notification sent to admin and designer
- ✅ Status history recorded
- ✅ Revision visible in order

**Verification:**
- [ ] Revision created successfully
- [ ] Order status updated
- [ ] Notifications sent
- [ ] Revision visible in order

---

### **Test 4.14: Designer Resolves Revision**

**Steps:**
1. Logout from Client
2. Login as Designer
3. Navigate to order detail page
4. View revision request
5. Upload new files addressing revision
6. Mark revision as resolved (if option exists)

**Expected Results:**
- ✅ New files uploaded
- ✅ Revision marked as resolved
- ✅ Order status can be updated
- ✅ Client notified

**Verification:**
- [ ] Revision resolved
- [ ] New files uploaded
- [ ] Status updated appropriately

---

### **Test 4.15: Client Approves Final Files**

**Steps:**
1. Logout from Designer
2. Login as Client
3. Navigate to order detail page
4. Review final files
5. Click "Approve Final" or "Mark as Complete"

**Expected Results:**
- ✅ Order status changes to `FinalApproved` or `Completed`
- ✅ Files added to client gallery
- ✅ Invoice can be generated
- ✅ Order marked as complete

**Verification:**
- [ ] Final approval successful
- [ ] Order status updated
- [ ] Files added to gallery
- [ ] Order complete

---

## **PHASE 5: Comments & Communication**

### **Test 5.1: Add Internal Comment (Admin/Designer)**

**Steps:**
1. Login as Admin or Designer
2. Navigate to order detail page
3. Go to "Comments" tab
4. Type comment: `Client wants more vibrant colors`
5. Check "Internal" checkbox (if available)
6. Click "Post Comment"

**Expected Results:**
- ✅ Comment created
- ✅ Marked as internal
- ✅ Visible to Admin and Designer only
- ✅ Client cannot see comment

**Verification:**
- [ ] Internal comment created
- [ ] Visible to admin/designer
- [ ] Hidden from client

---

### **Test 5.2: Add External Comment (Client)**

**Steps:**
1. Logout from Admin/Designer
2. Login as Client
3. Navigate to order detail page
4. Go to "Comments" tab
5. Type comment: `Thank you for the update!`
6. Click "Post Comment"

**Expected Results:**
- ✅ Comment created
- ✅ Marked as external
- ✅ Visible to all roles
- ✅ Appears in comments list

**Verification:**
- [ ] External comment created
- [ ] Visible to all roles
- [ ] Appears in comments list

---

### **Test 5.3: View Comments (Role-Based)**

**Steps:**
1. As Client, view comments
2. As Admin, view comments
3. As Designer, view comments

**Expected Results:**
- ✅ Client sees only external comments
- ✅ Admin sees all comments (internal and external)
- ✅ Designer sees all comments
- ✅ Comments ordered by date

**Verification:**
- [ ] Client sees only external comments
- [ ] Admin sees all comments
- [ ] Designer sees all comments
- [ ] Proper ordering

---

## **PHASE 6: Clients Management**

### **Test 6.1: View Clients List**

**Steps:**
1. Login as Admin or SuperAdmin
2. Navigate to `/clients`
3. Verify clients table

**Expected Results:**
- ✅ Clients list displays
- ✅ Columns: Name, Email, Company, Total Orders, Total Spent, Created, Last Order
- ✅ Search works
- ✅ Sorting works
- ✅ Clickable rows

**Verification:**
- [ ] Clients list displays
- [ ] Search filters correctly
- [ ] Click row navigates to detail

---

### **Test 6.2: Client Detail Page**

**Steps:**
1. In Clients page, click on any client
2. Navigate to `/clients/{id}`

**Expected Results:**

**Client Info Card:**
- ✅ Company name
- ✅ Phone number
- ✅ Total Orders count
- ✅ Total Spent (currency formatted)
- ✅ Member Since date
- ✅ Last Order date
- ✅ Status badge

**Order History Tab:**
- ✅ Shows all client orders
- ✅ Order details (ID, Title, Status, Amount, Dates)
- ✅ Click "View" → Navigates to order
- ✅ Empty state if no orders

**Invoices Tab:**
- ✅ Shows client invoices (if any)
- ✅ Invoice details (Number, Amount, Status, Dates)
- ✅ Click "View" → Navigates to invoice
- ✅ Empty state if no invoices

**Files Tab:**
- ✅ Shows client files
- ✅ File details (Name, Upload date)
- ✅ Download button works
- ✅ Empty state if no files

**Messages Tab:**
- ✅ Shows messages with client
- ✅ Message details (Sender, Content, Date)
- ✅ Empty state if no messages

**Verification:**
- [ ] All tabs load correctly
- [ ] Navigation works
- [ ] Empty states display properly
- [ ] Data displays correctly

---

## **PHASE 7: Invoices Management**

### **Test 7.1: View Invoices List**

**Steps:**
1. Login as Admin or SuperAdmin
2. Navigate to `/invoices`
3. Verify invoices page

**Expected Results:**

**Statistics Cards:**
- ✅ Total Invoices
- ✅ Paid (with amount)
- ✅ Unpaid (with pending amount)
- ✅ Overdue

**Table:**
- ✅ Invoice Number
- ✅ Client Name
- ✅ Amount (currency formatted)
- ✅ Status (Paid/Unpaid/Overdue)
- ✅ Due Date
- ✅ Actions (Download, Send)

**Verification:**
- [ ] Statistics calculate correctly
- [ ] Table displays invoices
- [ ] Status badges show correct colors
- [ ] Overdue highlighting works

---

### **Test 7.2: Generate Invoice from Order**

**Steps:**
1. Navigate to `/orders`
2. Find a completed order
3. Click "Generate Invoice" icon (📄)
4. Fill invoice details (if dialog appears):
   - Tax Amount: `50.00`
   - Due Date: Select date
   - Payment Method: `Bank Transfer`
   - Notes: `Payment due within 30 days`
5. Click "Generate"

**Expected Results:**
- ✅ Success message
- ✅ Invoice created
- ✅ Redirects to invoices page or shows invoice
- ✅ Invoice linked to order
- ✅ Invoice number generated

**Verification:**
- [ ] Invoice generated successfully
- [ ] Invoice appears in invoices list
- [ ] Invoice has correct amount
- [ ] Linked to order correctly

---

### **Test 7.3: Download Invoice**

**Steps:**
1. Navigate to `/invoices`
2. Find an invoice
3. Click "Download" icon (📥)

**Expected Results:**
- ✅ PDF downloads
- ✅ Or opens in new tab
- ✅ Invoice formatted correctly

**Verification:**
- [ ] Download works
- [ ] PDF format correct
- [ ] Invoice details accurate

---

### **Test 7.4: Mark Invoice as Paid**

**Steps:**
1. In Invoices page, find unpaid invoice
2. Click "Mark as Paid" or use action menu
3. Enter payment details:
   - Payment Method: `Bank Transfer`
   - Payment Date: Today
4. Click "Mark as Paid"

**Expected Results:**
- ✅ Status changes to "Paid"
- ✅ Paid date set
- ✅ Statistics update
- ✅ Invoice marked as paid

**Verification:**
- [ ] Invoice marked as paid
- [ ] Status updated
- [ ] Payment details saved
- [ ] Statistics update

---

## **PHASE 8: Settings Management**

### **Test 8.1: Business Info Settings**

**Steps:**
1. Login as Admin or SuperAdmin
2. Navigate to `/settings`
3. Go to "Business Info" tab
4. Fill form:
   - **Business Name:** `Logo Design Co.`
   - **Business Email:** `info@logodesign.com`
   - **Business Phone:** `+1 (555) 123-4567`
   - **Website:** `https://logodesign.com`
   - **Address:** `123 Main St, City, State 12345`
   - **Tax ID:** `TAX-123456`
5. Click "Save Business Info"

**Expected Results:**
- ✅ Success message
- ✅ Data saves to database
- ✅ Can reload and see saved data

**Verification:**
- [ ] Save successful
- [ ] Data persists
- [ ] Validation works

---

### **Test 8.2: Brand & Colors Settings**

**Steps:**
1. Go to "Brand & Colors" tab
2. Upload Logo:
   - Click "Choose Logo"
   - Select image file
   - Click "Upload Logo"
3. Set Colors:
   - **Primary Color:** `#6366f1`
   - **Secondary Color:** `#8b5cf6`
   - **Accent Color:** `#10b981`
4. Click "Save Brand Settings"

**Expected Results:**
- ✅ Logo uploads
- ✅ Preview shows
- ✅ Colors save
- ✅ Success message

**Verification:**
- [ ] Logo uploads successfully
- [ ] Color pickers work
- [ ] Settings save
- [ ] Data persists

---

### **Test 8.3: Invoice Template Settings**

**Steps:**
1. Go to "Invoice Template" tab
2. Fill form:
   - **Header Text:** `Thank you for your business!`
   - **Footer Text:** `Payment due within 30 days.`
   - **Terms & Conditions:** `[Your terms here]`
   - Check "Show logo on invoices"
   - Check "Show Tax ID on invoices"
3. Click "Save Invoice Template"

**Expected Results:**
- ✅ Success message
- ✅ Settings save
- ✅ Data persists

**Verification:**
- [ ] Settings save
- [ ] Data persists
- [ ] Checkboxes work

---

### **Test 8.4: Payment Methods Settings**

**Steps:**
1. Go to "Payment Methods" tab
2. Click "Add Payment Method"
3. Fill dialog:
   - **Name:** `PayPal`
   - **Type:** `PayPal`
   - **Account Details:** `payments@logodesign.com`
   - Check "Active"
4. Click "Save"

**Expected Results:**
- ✅ Payment method added
- ✅ Appears in list
- ✅ Can edit/delete

**Verification:**
- [ ] Payment method added
- [ ] Can edit
- [ ] Can delete
- [ ] Multiple methods display

---

### **Test 8.5: Notifications Settings**

**Steps:**
1. Go to "Notifications" tab
2. Toggle switches:
   - Email Notifications: ON
   - Order Notifications: ON
   - Payment Notifications: ON
   - Client Message Notifications: ON
   - Review Notifications: ON
3. Click "Save Notification Preferences"

**Expected Results:**
- ✅ Success message
- ✅ Preferences save
- ✅ Data persists

**Verification:**
- [ ] Toggles work
- [ ] Settings save
- [ ] Data persists

---

## **PHASE 9: Notifications System**

### **Test 9.1: View Notifications**

**Steps:**
1. Login as any user
2. Click notification bell icon in top navbar
3. View notifications dropdown

**Expected Results:**
- ✅ Notifications list displays
- ✅ Unread count badge shows
- ✅ Notifications ordered by date (newest first)
- ✅ Notification details visible

**Verification:**
- [ ] Notifications load
- [ ] Unread count accurate
- [ ] Proper ordering
- [ ] Details visible

---

### **Test 9.2: Mark Notification as Read**

**Steps:**
1. Click on unread notification
2. Or click "Mark as Read" action

**Expected Results:**
- ✅ Notification marked as read
- ✅ Unread count decreases
- ✅ Read indicator updates

**Verification:**
- [ ] Notification marked as read
- [ ] Count updates
- [ ] Indicator changes

---

### **Test 9.3: Mark All as Read**

**Steps:**
1. In notifications dropdown, click "Mark All as Read"

**Expected Results:**
- ✅ All notifications marked as read
- ✅ Unread count becomes 0
- ✅ All indicators update

**Verification:**
- [ ] All marked as read
- [ ] Count resets
- [ ] Indicators update

---

## **PHASE 10: Gallery System**

### **Test 10.1: View Client Gallery**

**Steps:**
1. Login as Client
2. Navigate to `/gallery` (if route exists) or view in order detail
3. View gallery items

**Expected Results:**
- ✅ All approved files from completed orders
- ✅ Gallery items include: order title, preview, format, approved date
- ✅ Items ordered by approval date (newest first)
- ✅ Download links work

**Verification:**
- [ ] Gallery items listed
- [ ] All details visible
- [ ] Proper ordering
- [ ] Preview images load
- [ ] Download works

---

### **Test 10.2: View Gallery Item Details**

**Steps:**
1. Click on gallery item
2. View details

**Expected Results:**
- ✅ Full gallery item details
- ✅ Order information
- ✅ File download link
- ✅ Format information

**Verification:**
- [ ] Item details correct
- [ ] Download link works
- [ ] All metadata visible

---

## **PHASE 11: Files Management**

### **Test 11.1: View Files List**

**Steps:**
1. Login as Admin or SuperAdmin
2. Navigate to `/files`
3. View files list

**Expected Results:**
- ✅ Files list displays
- ✅ Shows file name, order, upload date
- ✅ Download buttons work
- ✅ Delete buttons work

**Verification:**
- [ ] Files load
- [ ] Download works
- [ ] Delete works (with confirmation)
- [ ] Empty state shows

---

### **Test 11.2: Download File**

**Steps:**
1. In Files page, click "Download" button
2. Verify download

**Expected Results:**
- ✅ File downloads
- ✅ Correct file type
- ✅ File intact

**Verification:**
- [ ] Download works
- [ ] File correct
- [ ] No errors

---

### **Test 11.3: Delete File**

**Steps:**
1. In Files page, click "Delete" button
2. Confirm deletion

**Expected Results:**
- ✅ Confirmation dialog appears
- ✅ File deleted (soft delete)
- ✅ File removed from list
- ✅ Success message

**Verification:**
- [ ] Confirmation works
- [ ] File deleted
- [ ] List updates
- [ ] Success message

---

## **PHASE 12: Reviews System**

### **Test 12.1: View Reviews List**

**Steps:**
1. Navigate to `/reviews`
2. View reviews list

**Expected Results:**
- ✅ Reviews list displays
- ✅ Shows client, rating, comment, date
- ✅ Star ratings display
- ✅ Empty state if no reviews

**Verification:**
- [ ] Reviews load
- [ ] Ratings display correctly
- [ ] Empty state shows

---

### **Test 12.2: Create Review (Client)**

**Steps:**
1. Login as Client
2. Navigate to completed order
3. Add review (if UI exists):
   - **Rating:** `5` (1-5 stars)
   - **Comment:** `Great work!`
4. Submit

**Expected Results:**
- ✅ Review created
- ✅ Appears in reviews list
- ✅ Shows on order/project

**Verification:**
- [ ] Review created
- [ ] Appears in list
- [ ] Rating displays

---

## **PHASE 13: Complete End-to-End Scenario**

### **Scenario: Full Order Lifecycle**

**Test the complete flow from order creation to completion:**

1. **Setup:**
   - [ ] Create test users (Client, Designer, Admin)
   - [ ] Login as each user to verify access

2. **Order Creation:**
   - [ ] Client creates order
   - [ ] Order status: `WaitingForAdminApproval`
   - [ ] Admin sees order in list

3. **Price Approval (Optional):**
   - [ ] Admin requests price approval
   - [ ] Client receives notification
   - [ ] Client approves price
   - [ ] Order price updated

4. **Order Approval:**
   - [ ] Admin approves order
   - [ ] Order status: `InProgress`
   - [ ] Client notified

5. **Designer Assignment:**
   - [ ] Admin assigns to designer
   - [ ] Designer receives notification
   - [ ] Designer can see order (no client identity)

6. **File Upload:**
   - [ ] Designer uploads preview file
   - [ ] File NOT visible to client
   - [ ] Admin sees file

7. **File Approval:**
   - [ ] Admin approves file
   - [ ] Admin sends file to client
   - [ ] Order status: `PreviewDelivered`
   - [ ] Client receives notification
   - [ ] Client can see file

8. **Revision Request:**
   - [ ] Client requests revision
   - [ ] Order status: `RevisionRequested`
   - [ ] Admin and designer notified
   - [ ] Revision visible in order

9. **Revision Resolution:**
   - [ ] Designer uploads new files
   - [ ] Admin approves and sends
   - [ ] Client approves final
   - [ ] Order status: `FinalApproved`

10. **Completion:**
    - [ ] Order status: `Completed`
    - [ ] Files added to client gallery
    - [ ] Invoice generated
    - [ ] All notifications sent

**Verification:**
- [ ] All status transitions work
- [ ] Status history recorded
- [ ] Notifications sent at each step
- [ ] File visibility rules enforced
- [ ] Role-based access works
- [ ] Complete workflow successful

---

## **PHASE 14: Role-Based Access Control Verification**

### **Test 14.1: Client Access**

**Verify Client can:**
- [ ] Create orders
- [ ] View own orders only
- [ ] See only approved files
- [ ] Request revisions
- [ ] Approve/reject prices
- [ ] View own gallery
- [ ] View own invoices
- [ ] See only external comments
- [ ] View own notifications

**Verify Client cannot:**
- [ ] View other clients' orders
- [ ] See unapproved files
- [ ] See internal comments
- [ ] Assign orders
- [ ] Approve files
- [ ] View all orders
- [ ] Access admin features

---

### **Test 14.2: Designer Access**

**Verify Designer can:**
- [ ] View assigned orders only
- [ ] Upload files (Preview, Final)
- [ ] View all files for assigned orders
- [ ] See internal comments
- [ ] Resolve revisions
- [ ] Update order status (limited)

**Verify Designer cannot:**
- [ ] See client identity (name, email, phone)
- [ ] View unassigned orders
- [ ] Approve files
- [ ] Assign orders
- [ ] See client gallery
- [ ] Access admin features

---

### **Test 14.3: Admin Access**

**Verify Admin can:**
- [ ] View all orders
- [ ] Approve orders
- [ ] Assign orders to designers
- [ ] Request price approval
- [ ] Approve files
- [ ] Send files to clients
- [ ] View all files (approved and unapproved)
- [ ] See internal comments
- [ ] View all invoices
- [ ] See masked client info (no email/phone)

**Verify Admin cannot:**
- [ ] See client personal data (email/phone masked)
- [ ] Delete orders (soft delete only)
- [ ] Access SuperAdmin features

---

### **Test 14.4: SuperAdmin Access**

**Verify SuperAdmin can:**
- [ ] Everything Admin can do
- [ ] Full client information access
- [ ] User management
- [ ] System settings
- [ ] All permissions

---

## **PHASE 15: Error Handling & Edge Cases**

### **Test 15.1: Invalid Login**

**Steps:**
1. Try login with wrong credentials
2. Try login with empty fields

**Expected Results:**
- ✅ Error message displays
- ✅ Form validation works
- ✅ No crashes

**Verification:**
- [ ] Error handling works
- [ ] User-friendly messages
- [ ] No crashes

---

### **Test 15.2: Unauthorized Access**

**Steps:**
1. Try accessing admin routes as client
2. Try accessing other users' orders

**Expected Results:**
- ✅ Access denied
- ✅ Redirected appropriately
- ✅ Error message shows

**Verification:**
- [ ] Access control works
- [ ] Redirects correctly
- [ ] Error messages appropriate

---

### **Test 15.3: File Upload Errors**

**Steps:**
1. Try uploading invalid file type
2. Try uploading file too large
3. Try uploading without selecting file

**Expected Results:**
- ✅ Validation errors display
- ✅ File rejected
- ✅ User-friendly error messages

**Verification:**
- [ ] Validation works
- [ ] Error messages clear
- [ ] No crashes

---

### **Test 15.4: Network Errors**

**Steps:**
1. Stop backend server
2. Try performing actions in frontend

**Expected Results:**
- ✅ Error messages display
- ✅ Graceful degradation
- ✅ No crashes

**Verification:**
- [ ] Error handling works
- [ ] User informed
- [ ] Application stable

---

## ✅ **Complete Testing Checklist**

### **Authentication & Users**
- [ ] Login works
- [ ] Registration works
- [ ] Create user works
- [ ] User list displays
- [ ] Role-based access works
- [ ] Password reset works

### **Dashboard**
- [ ] Statistics cards display
- [ ] Charts render (when data exists)
- [ ] Recent orders show
- [ ] Role-based data works
- [ ] Clickable cards navigate

### **Orders**
- [ ] List displays
- [ ] Create order works
- [ ] Assign designer works
- [ ] Upload files works
- [ ] Change status works
- [ ] Price approval works
- [ ] Revision system works
- [ ] Generate invoice works
- [ ] Status flow complete

### **Files**
- [ ] Upload works
- [ ] Download works
- [ ] Delete works
- [ ] File visibility rules enforced
- [ ] File approval works
- [ ] Version tracking works

### **Clients**
- [ ] List displays
- [ ] Detail page works
- [ ] All tabs load
- [ ] Navigation works
- [ ] Statistics calculate

### **Projects/Orders Detail**
- [ ] Detail page works
- [ ] File upload works
- [ ] Comments work
- [ ] Status change works
- [ ] Revisions work

### **Invoices**
- [ ] Statistics cards work
- [ ] List displays
- [ ] Generate works
- [ ] Download works
- [ ] Mark as paid works

### **Settings**
- [ ] All tabs work
- [ ] Business info saves
- [ ] Logo uploads
- [ ] Colors save
- [ ] Payment methods work
- [ ] Notifications save

### **Messages**
- [ ] List displays
- [ ] Send works
- [ ] Mark read works

### **Reviews**
- [ ] List displays
- [ ] Create works
- [ ] Ratings display

### **Notifications**
- [ ] List displays
- [ ] Unread count works
- [ ] Mark as read works
- [ ] Mark all as read works

### **Gallery**
- [ ] Gallery displays
- [ ] Items show correctly
- [ ] Download works

### **Comments**
- [ ] Internal comments work
- [ ] External comments work
- [ ] Role-based visibility works

### **Security & Permissions**
- [ ] Client data masking works
- [ ] File visibility rules enforced
- [ ] Internal comments hidden from clients
- [ ] Role-based API access works

---

## 🐛 **Common Issues & Solutions**

### **Charts Not Showing**
- **Cause:** No orders in database
- **Solution:** Create some orders first

### **Empty States Everywhere**
- **Cause:** No data
- **Solution:** Create test users, orders, files

### **API Errors**
- **Check:** Backend is running
- **Check:** Database is initialized
- **Check:** CORS is configured
- **Check:** API endpoints exist

### **Can't Login**
- **Check:** Backend on port 5000
- **Check:** Database exists
- **Try:** Default SuperAdmin credentials

### **Files Not Visible to Client**
- **Check:** `IsVisibleToClient` flag
- **Check:** `IsAdminApproved` flag
- **Solution:** Admin must approve and send files

### **Notifications Not Appearing**
- **Check:** User ID matches
- **Check:** Notification service is called
- **Check:** Database has notifications

---

## 📊 **Testing Summary Report Template**

After completing all tests, fill this summary:

### **Overall Status:**
- [ ] ✅ All Critical Features Working
- [ ] ⚠️ Some Features Need Fixes
- [ ] ❌ Major Issues Found

### **Features Tested:**
- Authentication: [ ] Pass [ ] Fail
- User Management: [ ] Pass [ ] Fail
- Dashboard: [ ] Pass [ ] Fail
- Orders: [ ] Pass [ ] Fail
- Files: [ ] Pass [ ] Fail
- Clients: [ ] Pass [ ] Fail
- Invoices: [ ] Pass [ ] Fail
- Settings: [ ] Pass [ ] Fail
- Notifications: [ ] Pass [ ] Fail
- Gallery: [ ] Pass [ ] Fail
- Comments: [ ] Pass [ ] Fail
- Reviews: [ ] Pass [ ] Fail

### **Issues Found:**
1. [Issue description]
2. [Issue description]
3. [Issue description]

### **Recommendations:**
1. [Recommendation]
2. [Recommendation]
3. [Recommendation]

---

## 🎯 **Success Criteria**

Your testing is successful if:
- ✅ All core features work end-to-end
- ✅ Data persists after save
- ✅ Role-based access works correctly
- ✅ Navigation is smooth
- ✅ Empty states show appropriately
- ✅ Error handling works
- ✅ Forms validate correctly
- ✅ File visibility rules enforced
- ✅ Notifications work
- ✅ Complete order lifecycle works

---

## 🎉 **Testing Complete!**

Congratulations! You've tested the complete application flow. Document any issues found and verify all features work as expected.

**Happy Testing!** 🚀

---

**Last Updated:** 2024
**Version:** 1.0
**Status:** Complete Application Flow Testing Guide
