# 🌐 Web Application Testing Guide - Complete Testing Manual

## 📋 **Table of Contents**

1. [Setup & Prerequisites](#setup--prerequisites)
2. [Quick Start Guide](#quick-start-guide)
3. [Frontend UI Testing](#frontend-ui-testing)
4. [Backend API Testing](#backend-api-testing)
5. [Complete User Workflows](#complete-user-workflows)
6. [Role-Based Testing Scenarios](#role-based-testing-scenarios)
7. [Integration Testing](#integration-testing)
8. [Troubleshooting](#troubleshooting)

---

## 🚀 **Setup & Prerequisites**

### **System Requirements**
- ✅ .NET 8.0 SDK installed
- ✅ Node.js 18+ and npm installed
- ✅ SQL Server (LocalDB or Express)
- ✅ Modern web browser (Chrome, Edge, Firefox)
- ✅ Postman or similar API testing tool (optional)

### **1. Database Setup**

```bash
# Navigate to Infrastructure project
cd Backend/src/LogoDesignPortal.Infrastructure

# Apply migrations
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

✅ **Expected:** Database created with all tables and default SuperAdmin user

### **2. Backend Setup**

```bash
# Navigate to API project
cd Backend/src/LogoDesignPortal.API

# Restore packages (if needed)
dotnet restore

# Run the application
dotnet run
```

✅ **Expected Output:**
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
Application started. Press Ctrl+C to shut down.
```

### **3. Frontend Setup**

```bash
# Navigate to Frontend directory
cd Frontend

# Install dependencies (first time only)
npm install

# Start development server
npm start
```

✅ **Expected Output:**
```
** Angular Live Development Server is listening on localhost:4200 **
Compiled successfully!
```

### **4. Access Points**

- **Frontend:** http://localhost:4200
- **Backend API:** http://localhost:5000 or https://localhost:5001
- **Swagger UI:** http://localhost:5000/swagger or https://localhost:5001/swagger

---

## ⚡ **Quick Start Guide**

### **Default Login Credentials**

**SuperAdmin:**
- Email: `superadmin@logodesign.com`
- Password: `SuperAdmin@123`

### **Quick Test Flow**

1. ✅ Open http://localhost:4200
2. ✅ Login as SuperAdmin
3. ✅ Create test users (Client, Designer, Admin)
4. ✅ Create an order as Client
5. ✅ Approve order as Admin
6. ✅ Assign to Designer
7. ✅ Upload files as Designer
8. ✅ Approve files as Admin
9. ✅ Send files to Client
10. ✅ Test complete workflow

---

## 🎨 **Frontend UI Testing**

### **1. Authentication & Authorization**

#### **1.1 Login Page** (`/auth/login`)

**Test Cases:**
- [ ] **Valid Credentials**
  - Enter: `superadmin@logodesign.com` / `SuperAdmin@123`
  - Click "Login"
  - ✅ Should redirect to dashboard
  - ✅ Should show user name in header
  - ✅ Should store JWT token

- [ ] **Invalid Credentials**
  - Enter: `wrong@email.com` / `WrongPassword`
  - Click "Login"
  - ✅ Should show error message
  - ✅ Should NOT redirect

- [ ] **Empty Fields**
  - Leave fields empty
  - Click "Login"
  - ✅ Should show validation errors
  - ✅ Submit button should be disabled

- [ ] **Remember Me**
  - Check "Remember Me"
  - Login successfully
  - Close browser
  - Reopen and navigate to app
  - ✅ Should auto-login (token in localStorage)

#### **1.2 Registration Page** (`/auth/register`)

**Test Cases:**
- [ ] **Valid Registration**
  - Fill all required fields
  - Select role: "Client"
  - Click "Register"
  - ✅ Should show success message
  - ✅ Should redirect to login

- [ ] **Password Validation**
  - Enter weak password (e.g., "123")
  - ✅ Should show password strength indicator
  - ✅ Should prevent submission

- [ ] **Email Validation**
  - Enter invalid email (e.g., "notanemail")
  - ✅ Should show validation error
  - ✅ Should highlight field in red

#### **1.3 Role-Based Navigation**

**Test Cases:**
- [ ] **Client Role**
  - Login as Client
  - ✅ Should see: Orders, Gallery, Notifications
  - ✅ Should NOT see: Users, Settings, Permissions

- [ ] **Designer Role**
  - Login as Designer
  - ✅ Should see: Assigned Orders, Files
  - ✅ Should NOT see: All Orders, Users

- [ ] **Admin Role**
  - Login as Admin
  - ✅ Should see: Orders, Users, Files, Dashboard
  - ✅ Should see admin-specific actions

- [ ] **SuperAdmin Role**
  - Login as SuperAdmin
  - ✅ Should see ALL menu items
  - ✅ Should see Permissions management

---

### **2. Dashboard Testing**

#### **2.1 Dashboard Overview** (`/dashboard`)

**Test Cases:**
- [ ] **Statistics Display**
  - Login as Admin/SuperAdmin
  - Navigate to Dashboard
  - ✅ Should show:
    - Total Orders
    - Pending Orders
    - In Progress Orders
    - Completed Orders
    - Total Clients
    - Total Revenue

- [ ] **Recent Orders Table**
  - ✅ Should display last 10 orders
  - ✅ Should be sortable by date
  - ✅ Should show order status badges
  - ✅ Clicking order should navigate to detail

- [ ] **Charts & Graphs**
  - ✅ Orders by Status chart visible
  - ✅ Orders by Month chart visible
  - ✅ Revenue by Package chart visible

- [ ] **Role-Specific Dashboard**
  - Login as Client
  - ✅ Should show only client's orders
  - ✅ Should show client-specific stats

---

### **3. Order Management - Frontend**

#### **3.1 Create Order** (`/orders/create`)

**Test Cases:**
- [ ] **Form Validation**
  - Leave required fields empty
  - Click "Create Order"
  - ✅ Should show validation errors
  - ✅ Should highlight invalid fields

- [ ] **Valid Order Creation**
  - Fill all fields:
    - Title: "Modern Logo Design"
    - Description: "Need a modern logo"
    - Price: 500.00
    - Deadline: Select future date
    - Instructions: "Logo should be scalable"
    - Required Formats: "PNG, SVG, PDF"
  - Click "Create Order"
  - ✅ Should show success message
  - ✅ Should redirect to order detail page
  - ✅ Order status should be "Waiting for Admin Approval"

- [ ] **Date Picker**
  - Click on Deadline field
  - ✅ Calendar should open
  - ✅ Past dates should be disabled
  - ✅ Select future date
  - ✅ Date should be formatted correctly

- [ ] **Price Input**
  - Enter negative number
  - ✅ Should show validation error
  - ✅ Should prevent submission

- [ ] **Cancel Button**
  - Fill form partially
  - Click "Cancel"
  - ✅ Should navigate back to orders list
  - ✅ Should NOT create order

#### **3.2 Order List** (`/orders`)

**Test Cases:**
- [ ] **Order Display**
  - Navigate to Orders page
  - ✅ Should show list of orders
  - ✅ Each order should show:
    - Title
    - Status badge (color-coded)
    - Created date
    - Price
    - Client/Designer name

- [ ] **Filtering**
  - Use status filter dropdown
  - Select "In Progress"
  - ✅ Should show only in-progress orders

- [ ] **Search**
  - Enter search term in search box
  - ✅ Should filter orders by title/description
  - ✅ Should update results in real-time

- [ ] **Sorting**
  - Click column headers
  - ✅ Should sort by that column
  - ✅ Should show sort indicator (arrow)

- [ ] **Pagination**
  - If more than 10 orders
  - ✅ Should show pagination controls
  - ✅ Clicking page number should load that page

- [ ] **Click Order**
  - Click on any order row
  - ✅ Should navigate to order detail page

#### **3.3 Order Detail** (`/orders/:id`)

**Test Cases:**
- [ ] **Order Information Display**
  - Navigate to order detail
  - ✅ Should show:
    - Order title and description
    - Status badge
    - Price information
    - Client/Designer details
    - Created/Updated dates
    - Instructions
    - Required formats

- [ ] **Tabs Navigation**
  - ✅ Should have tabs: Files, Revisions, Comments, History
  - ✅ Clicking tab should switch content
  - ✅ Active tab should be highlighted

- [ ] **Files Tab**
  - Click "Files" tab
  - ✅ Should show list of files
  - ✅ Should show file type badges (Reference, Preview, Final)
  - ✅ Should show approval status
  - ✅ Should show download buttons for visible files
  - ✅ Admin should see "Approve" buttons

- [ ] **Revisions Tab**
  - Click "Revisions" tab
  - ✅ Should show list of revisions
  - ✅ Should show revision status (Resolved/Open)
  - ✅ Should show "Request Revision" button (for clients)
  - ✅ Clicking revision should show details

- [ ] **Comments Tab**
  - Click "Comments" tab
  - ✅ Should show all comments
  - ✅ Should show comment author and timestamp
  - ✅ Internal comments should be marked
  - ✅ Should show "Add Comment" button

- [ ] **History Tab**
  - Click "History" tab
  - ✅ Should show status change history
  - ✅ Should show who made changes
  - ✅ Should show timestamps

- [ ] **Action Buttons (Role-Based)**
  - **As Admin:**
    - ✅ Should see "Approve Order" button
    - ✅ Should see "Request Price Approval" button
    - ✅ Should see "Assign Designer" button
    - ✅ Should see "Send Files to Client" button

  - **As Client:**
    - ✅ Should see "Approve Price" button (if price approval pending)
    - ✅ Should see "Request Revision" button
    - ✅ Should NOT see admin actions

  - **As Designer:**
    - ✅ Should see "Upload File" button
    - ✅ Should NOT see admin actions

#### **3.4 Price Approval Flow**

**Test Cases:**
- [ ] **Admin Requests Price Approval**
  - As Admin, open order detail
  - Click "Request Price Approval"
  - ✅ Dialog should open
  - Enter proposed price: 600.00
  - Add notes: "Additional complexity"
  - Click "Send Request"
  - ✅ Should show success message
  - ✅ Order status should change to "Price Approval Pending"
  - ✅ Client should receive notification

- [ ] **Client Approves Price**
  - Login as Client
  - Navigate to order detail
  - ✅ Should see "Approve Price" button
  - Click "Approve Price"
  - ✅ Dialog should open
  - Check "Approve" checkbox
  - Add optional notes
  - Click "Submit"
  - ✅ Should show success message
  - ✅ Order status should change to "In Progress"
  - ✅ Admin should receive notification

- [ ] **Client Rejects Price**
  - As Client, click "Approve Price"
  - Uncheck "Approve" checkbox
  - Add rejection notes
  - Click "Submit"
  - ✅ Should show message
  - ✅ Order status should remain "Price Approval Pending"
  - ✅ Admin should receive notification with notes

---

### **4. File Management - Frontend**

#### **4.1 File Upload** (`/orders/:id/upload`)

**Test Cases:**
- [ ] **File Type Selection**
  - Navigate to upload page
  - ✅ Should show dropdown for file type:
    - Reference
    - Preview
    - Final
  - ✅ Should show description for each type

- [ ] **File Selection**
  - Click "Choose File"
  - ✅ File picker should open
  - ✅ Should filter by accepted formats (JPG, PNG, PDF, etc.)
  - Select file
  - ✅ Should show file name and size
  - ✅ Should show file preview (if image)

- [ ] **Upload Process**
  - Select file type: "Preview"
  - Choose file (e.g., logo-preview.png)
  - Add description: "First draft"
  - Click "Upload"
  - ✅ Should show loading indicator
  - ✅ Should show progress (if implemented)
  - ✅ Should show success message
  - ✅ Should redirect to order detail

- [ ] **File Size Validation**
  - Try to upload file > 10MB
  - ✅ Should show error message
  - ✅ Should prevent upload

- [ ] **File Format Validation**
  - Try to upload unsupported format (e.g., .exe)
  - ✅ Should show error message
  - ✅ Should prevent upload

- [ ] **Reference Files (Client)**
  - As Client, upload Reference file
  - ✅ Should be immediately visible
  - ✅ Should NOT require approval

- [ ] **Preview/Final Files (Designer)**
  - As Designer, upload Preview file
  - ✅ Should NOT be visible to client
  - ✅ Should require admin approval
  - ✅ Admin should see "Approve" button

#### **4.2 File Approval (Admin)**

**Test Cases:**
- [ ] **View Unapproved Files**
  - As Admin, open order detail
  - Go to Files tab
  - ✅ Should see all files (including unapproved)
  - ✅ Unapproved files should be marked
  - ✅ Should show "Approve" button

- [ ] **Approve File**
  - Click "Approve" on a file
  - ✅ Dialog should open
  - Check "Approve" and "Make Visible to Client"
  - Click "Approve"
  - ✅ Should show success message
  - ✅ File should be marked as approved
  - ✅ File should be visible to client

- [ ] **Reject File**
  - Click "Approve" on a file
  - Uncheck "Approve"
  - Add rejection notes
  - Click "Submit"
  - ✅ Should show message
  - ✅ File should remain unapproved
  - ✅ Designer should receive notification

#### **4.3 File Download**

**Test Cases:**
- [ ] **Download Visible File**
  - As Client, open order detail
  - Go to Files tab
  - Click "Download" on visible file
  - ✅ Should download file
  - ✅ File name should be correct
  - ✅ File content should be intact

- [ ] **Download Permission Check**
  - As Client, try to download unapproved file
  - ✅ Should show error or hide download button
  - ✅ Should NOT allow download

---

### **5. Revision System - Frontend**

#### **5.1 Request Revision (Client)**

**Test Cases:**
- [ ] **Create Revision Request**
  - As Client, open order detail
  - Go to Revisions tab
  - Click "Request Revision"
  - ✅ Dialog should open
  - Enter instructions: "Change color to blue"
  - (Optional) Upload reference images
  - Click "Submit"
  - ✅ Should show success message
  - ✅ Revision should appear in list
  - ✅ Order status should change to "Revision Requested"
  - ✅ Designer/Admin should receive notification

- [ ] **Upload Revision Files**
  - In revision dialog
  - Click "Upload Reference Image"
  - Select image file
  - ✅ Should show file preview
  - ✅ Should allow multiple files
  - ✅ Should show file type options (Reference, Machine Photo, Output Photo)

#### **5.2 Resolve Revision (Designer/Admin)**

**Test Cases:**
- [ ] **View Revision Request**
  - As Designer, open order detail
  - Go to Revisions tab
  - ✅ Should see open revisions
  - ✅ Should show revision instructions
  - ✅ Should show attached files

- [ ] **Resolve Revision**
  - Click "Resolve Revision"
  - ✅ Should mark revision as resolved
  - ✅ Order status should update
  - ✅ Client should receive notification

---

### **6. Comments System - Frontend**

#### **6.1 Add Comment**

**Test Cases:**
- [ ] **Public Comment (Client)**
  - As Client, open order detail
  - Go to Comments tab
  - Click "Add Comment"
  - ✅ Dialog should open
  - Enter comment: "Looking forward to the design"
  - Click "Post"
  - ✅ Should show comment in list
  - ✅ Should show author name and timestamp
  - ✅ Should be visible to all roles

- [ ] **Internal Comment (Admin/Designer)**
  - As Admin, open order detail
  - Go to Comments tab
  - Click "Add Comment"
  - Check "Internal Comment" checkbox
  - Enter comment: "Designer note: Use vector format"
  - Click "Post"
  - ✅ Should show comment with "Internal" badge
  - ✅ Should NOT be visible to client
  - ✅ Should be visible to Admin/Designer

- [ ] **Comment Validation**
  - Try to post empty comment
  - ✅ Should show validation error
  - ✅ Should prevent submission

---

### **7. Notifications - Frontend**

#### **7.1 Notification Center** (`/notifications`)

**Test Cases:**
- [ ] **View Notifications**
  - Navigate to Notifications page
  - ✅ Should show list of notifications
  - ✅ Should show notification type badges
  - ✅ Unread notifications should be highlighted
  - ✅ Should show notification count in header

- [ ] **Mark as Read**
  - Click on unread notification
  - ✅ Should mark as read
  - ✅ Should remove highlight
  - ✅ Unread count should decrease

- [ ] **Mark All as Read**
  - Click "Mark All Read" button
  - ✅ All notifications should be marked as read
  - ✅ Unread count should be 0

- [ ] **Filter Notifications**
  - Click "Show Unread Only"
  - ✅ Should filter to show only unread
  - ✅ Toggle should change to "Show All"

- [ ] **Notification Types**
  - ✅ Should show different icons for:
    - Order Updates
    - Price Approvals
    - Revision Requests
    - File Approvals

- [ ] **Click Notification**
  - Click on order-related notification
  - ✅ Should navigate to order detail page

---

### **8. Gallery - Frontend**

#### **8.1 Client Gallery** (`/gallery`)

**Test Cases:**
- [ ] **View Gallery**
  - As Client, navigate to Gallery
  - ✅ Should show grid of approved files
  - ✅ Should show JPEG previews
  - ✅ Should show file information:
    - Order title
    - File name
    - Format
    - Approval date

- [ ] **Empty Gallery**
  - If no approved files
  - ✅ Should show empty state message
  - ✅ Should show helpful text

- [ ] **Preview File**
  - Click on gallery item
  - ✅ Dialog should open
  - ✅ Should show full-size preview
  - ✅ Should show file details
  - ✅ Should show download button

- [ ] **Download File**
  - Click "Download" button
  - ✅ Should download original file
  - ✅ File format should be correct (not just JPEG preview)

- [ ] **Gallery Filtering** (if implemented)
  - Filter by order
  - ✅ Should show only files from that order

---

### **9. Invoice System - Frontend**

#### **9.1 Invoice List** (`/invoices`)

**Test Cases:**
- [ ] **View Invoices**
  - Navigate to Invoices page
  - ✅ Should show list of invoices
  - ✅ Should show invoice status badges:
    - Paid (green)
    - Pending (yellow)
    - Due (orange)
    - Overdue (red)

- [ ] **Invoice Details**
  - Click on invoice
  - ✅ Should show:
    - Invoice number
    - Associated orders
    - Total amount
    - Tax amount
    - Due date
    - Payment status

- [ ] **Create Invoice** (Admin)
  - As Admin, select multiple completed orders
  - Click "Create Invoice"
  - ✅ Should create invoice
  - ✅ Should include all selected orders
  - ✅ Should calculate total amount

- [ ] **Mark as Paid** (Admin)
  - As Admin, open invoice
  - Click "Mark as Paid"
  - ✅ Should update status
  - ✅ Should record payment date

---

## 🔌 **Backend API Testing**

### **1. Authentication API**

#### **1.1 Login** `POST /api/auth/login`

**Request:**
```json
{
  "email": "superadmin@logodesign.com",
  "password": "SuperAdmin@123"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "email": "superadmin@logodesign.com",
    "role": "SuperAdmin",
    "name": "Super Admin"
  }
}
```

**Test Cases:**
- [ ] Valid credentials → 200 OK with token
- [ ] Invalid credentials → 401 Unauthorized
- [ ] Missing fields → 400 Bad Request
- [ ] Token expiration → 401 after expiry

#### **1.2 Register** `POST /api/auth/register`

**Request:**
```json
{
  "email": "client@test.com",
  "password": "Client@123",
  "name": "Test Client",
  "role": "Client"
}
```

**Test Cases:**
- [ ] Valid data → 201 Created
- [ ] Duplicate email → 400 Bad Request
- [ ] Weak password → 400 Bad Request
- [ ] Invalid role → 400 Bad Request

---

### **2. Orders API**

#### **2.1 Create Order** `POST /api/orders`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request:**
```json
{
  "title": "Modern Logo Design",
  "description": "Need a modern logo",
  "price": 500.00,
  "deadline": "2024-12-31T00:00:00Z",
  "instructions": "Logo should be scalable",
  "requiredFormats": "PNG, SVG, PDF",
  "requirements": "Must include icon version",
  "colorPreferences": "Blue and white",
  "stylePreferences": "Modern, minimalist"
}
```

**Expected Response:** `201 Created`
```json
{
  "id": "guid",
  "title": "Modern Logo Design",
  "status": "WaitingForAdminApproval",
  "price": 500.00,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Test Cases:**
- [ ] Valid order (Client) → 201 Created
- [ ] Unauthorized (no token) → 401
- [ ] Wrong role (not Client) → 403
- [ ] Missing required fields → 400
- [ ] Invalid price → 400

#### **2.2 Get Order** `GET /api/orders/{id}`

**Test Cases:**
- [ ] Owner (Client) → 200 OK with full details
- [ ] Assigned Designer → 200 OK with full details
- [ ] Admin → 200 OK with full details
- [ ] Other Client → 403 Forbidden
- [ ] Non-existent order → 404 Not Found

#### **2.3 Get All Orders** `GET /api/orders`

**Test Cases:**
- [ ] Admin/SuperAdmin → 200 OK with all orders
- [ ] Client → 403 Forbidden
- [ ] Designer → 403 Forbidden

#### **2.4 Approve Order** `POST /api/orders/{id}/approve`

**Test Cases:**
- [ ] Admin approves → 200 OK, status = "InProgress"
- [ ] Client tries to approve → 403 Forbidden
- [ ] Already approved → 400 Bad Request

#### **2.5 Request Price Approval** `POST /api/orders/{id}/request-price-approval`

**Request:**
```json
{
  "proposedPrice": 600.00,
  "notes": "Additional complexity"
}
```

**Test Cases:**
- [ ] Admin requests → 200 OK, status = "PriceApprovalPending"
- [ ] Client receives notification
- [ ] Invalid price → 400 Bad Request

#### **2.6 Approve Price** `POST /api/orders/{id}/approve-price`

**Request:**
```json
{
  "isApproved": true,
  "notes": "Accepted"
}
```

**Test Cases:**
- [ ] Client approves → 200 OK, status = "InProgress"
- [ ] Client rejects → 200 OK, status remains "PriceApprovalPending"
- [ ] Admin tries to approve → 403 Forbidden

#### **2.7 Assign Designer** `POST /api/orders/{id}/assign`

**Request:**
```json
{
  "designerId": "designer-guid"
}
```

**Test Cases:**
- [ ] Admin assigns → 200 OK, designerId set
- [ ] Designer receives notification
- [ ] Invalid designer → 400 Bad Request

---

### **3. Files API**

#### **3.1 Upload File** `POST /api/files/upload/{orderId}`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Form Data:**
- `file`: (binary)
- `fileType`: "Reference" | "Preview" | "Final"
- `description`: (optional string)

**Test Cases:**
- [ ] Client uploads Reference → 200 OK, visible immediately
- [ ] Designer uploads Preview → 200 OK, requires approval
- [ ] File too large → 400 Bad Request
- [ ] Invalid format → 400 Bad Request
- [ ] Unauthorized → 401

#### **3.2 Get Order Files** `GET /api/files/order/{orderId}`

**Test Cases:**
- [ ] Client → 200 OK, only visible files
- [ ] Designer → 200 OK, all files (own uploads)
- [ ] Admin → 200 OK, all files (including unapproved)

#### **3.3 Approve File** `PUT /api/files/{id}/approve`

**Request:**
```json
{
  "isApproved": true,
  "isVisibleToClient": true,
  "isFinalVersion": false
}
```

**Test Cases:**
- [ ] Admin approves → 200 OK
- [ ] File becomes visible to client
- [ ] Client receives notification
- [ ] Unauthorized → 403

#### **3.4 Download File** `GET /api/files/{id}/download`

**Test Cases:**
- [ ] Owner downloads → 200 OK with file
- [ ] Visible file (Client) → 200 OK
- [ ] Unapproved file (Client) → 403 Forbidden
- [ ] Non-existent file → 404 Not Found

---

### **4. Revisions API**

#### **4.1 Create Revision** `POST /api/revisions/order/{orderId}`

**Request:**
```json
{
  "instructions": "Change color to blue",
  "fileIds": ["file-guid-1", "file-guid-2"]
}
```

**Test Cases:**
- [ ] Client creates → 201 Created
- [ ] Order status → "RevisionRequested"
- [ ] Designer receives notification
- [ ] Missing instructions → 400 Bad Request

#### **4.2 Get Revisions** `GET /api/revisions/order/{orderId}`

**Test Cases:**
- [ ] All roles can view → 200 OK
- [ ] Returns list of revisions
- [ ] Includes revision files

#### **4.3 Resolve Revision** `PUT /api/revisions/{id}/resolve`

**Test Cases:**
- [ ] Designer resolves → 200 OK
- [ ] Revision marked as resolved
- [ ] Client receives notification

---

### **5. Comments API**

#### **5.1 Add Comment** `POST /api/comments/order/{orderId}`

**Request:**
```json
{
  "content": "Looking good!",
  "isInternal": false
}
```

**Test Cases:**
- [ ] Any role can comment → 201 Created
- [ ] Internal comment (Admin/Designer) → Not visible to Client
- [ ] Public comment → Visible to all
- [ ] Empty content → 400 Bad Request

#### **5.2 Get Comments** `GET /api/comments/order/{orderId}`

**Test Cases:**
- [ ] Client → 200 OK, only public comments
- [ ] Admin/Designer → 200 OK, all comments (including internal)
- [ ] Returns sorted by date

---

### **6. Notifications API**

#### **6.1 Get Notifications** `GET /api/notifications`

**Test Cases:**
- [ ] Returns user's notifications → 200 OK
- [ ] Sorted by date (newest first)
- [ ] Includes unread count

#### **6.2 Mark as Read** `PUT /api/notifications/mark-read`

**Request:**
```json
{
  "notificationIds": ["guid-1", "guid-2"]
}
```

**Test Cases:**
- [ ] Marks notifications as read → 200 OK
- [ ] Updates readAt timestamp
- [ ] Invalid IDs → 400 Bad Request

---

### **7. Gallery API**

#### **7.1 Get Gallery** `GET /api/gallery/my-gallery`

**Test Cases:**
- [ ] Client only → 200 OK
- [ ] Returns approved files only
- [ ] Includes preview paths
- [ ] Other roles → 403 Forbidden

---

## 🔄 **Complete User Workflows**

### **Workflow 1: Complete Order Lifecycle**

1. **Client Creates Order**
   - [ ] Login as Client
   - [ ] Navigate to Orders → Create Order
   - [ ] Fill form and submit
   - [ ] ✅ Order created, status: "WaitingForAdminApproval"

2. **Admin Reviews Order**
   - [ ] Login as Admin
   - [ ] Navigate to Orders
   - [ ] Find new order
   - [ ] Review details
   - [ ] ✅ Order visible in list

3. **Admin Approves Order**
   - [ ] Open order detail
   - [ ] Click "Approve Order"
   - [ ] ✅ Status changes to "InProgress"
   - [ ] ✅ Client receives notification

4. **Admin Assigns Designer**
   - [ ] Click "Assign Designer"
   - [ ] Select designer from dropdown
   - [ ] Submit
   - [ ] ✅ Designer assigned
   - [ ] ✅ Designer receives notification

5. **Designer Uploads Preview**
   - [ ] Login as Designer
   - [ ] Navigate to Assigned Orders
   - [ ] Open order
   - [ ] Upload Preview file
   - [ ] ✅ File uploaded, requires approval

6. **Admin Approves File**
   - [ ] Login as Admin
   - [ ] Open order detail
   - [ ] Go to Files tab
   - [ ] Click "Approve" on file
   - [ ] Check "Make Visible to Client"
   - [ ] Submit
   - [ ] ✅ File approved and visible

7. **Admin Sends Files to Client**
   - [ ] Click "Send Files to Client"
   - [ ] Select files
   - [ ] Submit
   - [ ] ✅ Status changes to "PreviewDelivered"
   - [ ] ✅ Client receives notification

8. **Client Reviews Files**
   - [ ] Login as Client
   - [ ] Open order detail
   - [ ] View files
   - [ ] ✅ Files visible and downloadable

9. **Client Approves Final**
   - [ ] Click "Approve Final"
   - [ ] ✅ Status changes to "FinalApproved"
   - [ ] ✅ Files moved to gallery

10. **Order Completed**
    - [ ] Admin marks as "Completed"
    - [ ] ✅ Order moves to invoice section
    - [ ] ✅ Invoice can be created

---

### **Workflow 2: Price Approval Flow**

1. **Client Creates Order with Price**
   - [ ] Create order with price: 500.00
   - [ ] ✅ Order created

2. **Admin Requests Price Approval**
   - [ ] Admin opens order
   - [ ] Click "Request Price Approval"
   - [ ] Enter proposed price: 600.00
   - [ ] Add notes
   - [ ] Submit
   - [ ] ✅ Status: "PriceApprovalPending"
   - [ ] ✅ Client notified

3. **Client Reviews Price**
   - [ ] Client opens order
   - [ ] ✅ Sees price approval request
   - [ ] ✅ Sees proposed price and notes

4. **Client Approves Price**
   - [ ] Click "Approve Price"
   - [ ] Check "Approve"
   - [ ] Submit
   - [ ] ✅ Status: "InProgress"
   - [ ] ✅ Admin notified

---

### **Workflow 3: Revision Request Flow**

1. **Client Requests Revision**
   - [ ] Client opens order with delivered files
   - [ ] Go to Revisions tab
   - [ ] Click "Request Revision"
   - [ ] Enter instructions
   - [ ] Upload reference images
   - [ ] Submit
   - [ ] ✅ Revision created
   - [ ] ✅ Status: "RevisionRequested"
   - [ ] ✅ Designer notified

2. **Designer Reviews Revision**
   - [ ] Designer opens order
   - [ ] Go to Revisions tab
   - [ ] ✅ Sees revision request
   - [ ] ✅ Sees instructions and files

3. **Designer Uploads New Files**
   - [ ] Upload new Preview files
   - [ ] ✅ Files uploaded

4. **Designer Resolves Revision**
   - [ ] Click "Resolve Revision"
   - [ ] ✅ Revision marked resolved
   - [ ] ✅ Status updated
   - [ ] ✅ Client notified

---

## 👥 **Role-Based Testing Scenarios**

### **Client Role**

**Permissions:**
- ✅ Create orders
- ✅ View own orders
- ✅ Upload reference files
- ✅ Request revisions
- ✅ Approve prices
- ✅ View approved files
- ✅ Access gallery
- ✅ Add public comments
- ❌ View all orders
- ❌ Approve files
- ❌ Assign designers
- ❌ View internal comments

**Test Scenarios:**
- [ ] Create order → Success
- [ ] View other client's order → 403 Forbidden
- [ ] Upload reference file → Success, immediately visible
- [ ] Upload preview file → 403 Forbidden (Designer only)
- [ ] View unapproved file → Hidden
- [ ] Approve price → Success
- [ ] Request revision → Success
- [ ] Access gallery → Success
- [ ] View internal comment → Hidden

---

### **Designer Role**

**Permissions:**
- ✅ View assigned orders
- ✅ Upload preview/final files
- ✅ View all files (own uploads)
- ✅ Resolve revisions
- ✅ Add internal comments
- ❌ Create orders
- ❌ View all orders
- ❌ Approve files
- ❌ Assign orders
- ❌ View other designers' files

**Test Scenarios:**
- [ ] View assigned orders → Success
- [ ] View unassigned order → 403 Forbidden
- [ ] Upload preview file → Success, requires approval
- [ ] View own uploaded files → Success
- [ ] View other designer's files → Hidden
- [ ] Resolve revision → Success
- [ ] Add internal comment → Success
- [ ] Create order → 403 Forbidden

---

### **Admin Role**

**Permissions:**
- ✅ View all orders
- ✅ Approve orders
- ✅ Assign designers
- ✅ Approve/reject files
- ✅ Request price approval
- ✅ Send files to clients
- ✅ View all comments (including internal)
- ✅ Manage users (if granted permission)
- ❌ Manage permissions
- ❌ Delete users

**Test Scenarios:**
- [ ] View all orders → Success
- [ ] Approve order → Success
- [ ] Assign designer → Success
- [ ] Approve file → Success
- [ ] View internal comments → Success
- [ ] Request price approval → Success
- [ ] Send files to client → Success
- [ ] Manage permissions → 403 Forbidden (SuperAdmin only)

---

### **SuperAdmin Role**

**Permissions:**
- ✅ All Admin permissions
- ✅ Manage permissions
- ✅ Manage all users
- ✅ Delete users
- ✅ System settings
- ✅ Full system access

**Test Scenarios:**
- [ ] All Admin tests → Success
- [ ] Manage permissions → Success
- [ ] Create/Delete users → Success
- [ ] System settings → Success

---

## 🔗 **Integration Testing**

### **1. End-to-End Order Flow**

**Test Steps:**
1. [ ] Client creates order via UI
2. [ ] Verify order in database
3. [ ] Admin sees order in UI
4. [ ] Admin approves via UI
5. [ ] Verify status change in database
6. [ ] Client receives notification
7. [ ] Designer uploads file via UI
8. [ ] Verify file in database
9. [ ] Admin approves file via UI
10. [ ] Client sees file in UI
11. [ ] Client downloads file
12. [ ] Verify download works

**Expected:**
- ✅ All steps complete without errors
- ✅ Data persists correctly
- ✅ Notifications sent
- ✅ Status changes properly
- ✅ File visibility rules enforced

---

### **2. Multi-User Concurrent Testing**

**Test Steps:**
1. [ ] Client A creates order
2. [ ] Client B creates order (simultaneously)
3. [ ] Admin views orders
4. [ ] Admin approves both orders
5. [ ] Designer A uploads file to Order 1
6. [ ] Designer B uploads file to Order 2 (simultaneously)
7. [ ] Admin approves both files

**Expected:**
- ✅ No data conflicts
- ✅ All operations succeed
- ✅ Notifications sent correctly
- ✅ Status updates correctly

---

### **3. File Upload Stress Test**

**Test Steps:**
1. [ ] Upload 10 files simultaneously
2. [ ] Upload large file (9MB)
3. [ ] Upload multiple file types
4. [ ] Verify all files saved
5. [ ] Verify file metadata correct

**Expected:**
- ✅ All uploads succeed
- ✅ Files stored correctly
- ✅ No performance degradation
- ✅ File metadata accurate

---

## 🐛 **Troubleshooting**

### **Common Issues**

#### **1. Frontend Not Loading**

**Symptoms:**
- Blank page
- Console errors
- 404 errors

**Solutions:**
- ✅ Check backend is running
- ✅ Check API base URL in environment files
- ✅ Check CORS settings in backend
- ✅ Clear browser cache
- ✅ Check browser console for errors

#### **2. Authentication Issues**

**Symptoms:**
- Login fails
- Token expired
- 401 Unauthorized

**Solutions:**
- ✅ Check token expiration time
- ✅ Verify JWT secret key
- ✅ Check refresh token logic
- ✅ Clear localStorage/sessionStorage
- ✅ Re-login

#### **3. File Upload Fails**

**Symptoms:**
- Upload button doesn't work
- File not saved
- 400 Bad Request

**Solutions:**
- ✅ Check file size limit (10MB)
- ✅ Check file format allowed
- ✅ Verify file storage path exists
- ✅ Check permissions on storage folder
- ✅ Verify multipart/form-data encoding

#### **4. Notifications Not Showing**

**Symptoms:**
- No notifications received
- Notifications not updating

**Solutions:**
- ✅ Check notification service running
- ✅ Verify database notifications table
- ✅ Check user ID matches
- ✅ Refresh page
- ✅ Check notification API endpoint

#### **5. Status Not Updating**

**Symptoms:**
- Status change doesn't reflect
- Wrong status shown

**Solutions:**
- ✅ Refresh page
- ✅ Check database directly
- ✅ Verify status change API called
- ✅ Check status enum values match
- ✅ Verify role permissions

---

## ✅ **Testing Checklist Summary**

### **Frontend UI**
- [ ] Login/Registration works
- [ ] Role-based navigation correct
- [ ] Dashboard displays correctly
- [ ] Order creation form validates
- [ ] Order list displays and filters
- [ ] Order detail shows all tabs
- [ ] File upload works
- [ ] File approval interface works
- [ ] Revision system works
- [ ] Comments system works
- [ ] Notifications display correctly
- [ ] Gallery displays correctly
- [ ] Invoice system works

### **Backend API**
- [ ] Authentication endpoints work
- [ ] Order CRUD operations work
- [ ] File upload/download works
- [ ] File approval works
- [ ] Revision endpoints work
- [ ] Comment endpoints work
- [ ] Notification endpoints work
- [ ] Gallery endpoints work
- [ ] Authorization enforced
- [ ] Validation works

### **Integration**
- [ ] Complete order workflow works
- [ ] Price approval flow works
- [ ] Revision flow works
- [ ] File visibility rules enforced
- [ ] Notifications sent correctly
- [ ] Status transitions correct
- [ ] Multi-user scenarios work

---

## 📝 **Test Data Templates**

### **Test Users**

```json
{
  "clients": [
    {"email": "client1@test.com", "password": "Client@123", "name": "Test Client 1"},
    {"email": "client2@test.com", "password": "Client@123", "name": "Test Client 2"}
  ],
  "designers": [
    {"email": "designer1@test.com", "password": "Designer@123", "name": "Test Designer 1"},
    {"email": "designer2@test.com", "password": "Designer@123", "name": "Test Designer 2"}
  ],
  "admins": [
    {"email": "admin1@test.com", "password": "Admin@123", "name": "Test Admin 1"}
  ]
}
```

### **Test Orders**

```json
{
  "order1": {
    "title": "Modern Logo Design",
    "description": "Need a modern, minimalist logo",
    "price": 500.00,
    "instructions": "Logo should be scalable",
    "requiredFormats": "PNG, SVG, PDF"
  },
  "order2": {
    "title": "Brand Identity Package",
    "description": "Complete brand identity",
    "price": 1500.00,
    "instructions": "Include logo, colors, typography",
    "requiredFormats": "AI, PDF, PNG"
  }
}
```

---

## 🎯 **Performance Testing**

### **Load Testing Scenarios**

1. **Concurrent Users**
   - [ ] 10 users creating orders simultaneously
   - [ ] 20 users viewing orders
   - [ ] 5 designers uploading files

2. **Large Data**
   - [ ] 1000 orders in database
   - [ ] 100 files per order
   - [ ] 500 notifications per user

3. **File Operations**
   - [ ] Upload 50 files simultaneously
   - [ ] Download 100 files
   - [ ] Large file (10MB) upload

---

## 📊 **Test Reporting**

### **Test Results Template**

```
Test Date: [Date]
Tester: [Name]
Environment: Development/Staging/Production

Frontend Tests: [X/Y] Passed
Backend Tests: [X/Y] Passed
Integration Tests: [X/Y] Passed

Issues Found:
1. [Issue description]
2. [Issue description]

Notes:
[Additional notes]
```

---

## 🔄 **Continuous Testing**

### **Daily Smoke Tests**

- [ ] Login works
- [ ] Create order works
- [ ] View orders works
- [ ] Upload file works
- [ ] Download file works

### **Weekly Full Tests**

- [ ] Complete order workflow
- [ ] All role permissions
- [ ] All API endpoints
- [ ] File operations
- [ ] Notification system

---

**🎉 Happy Testing!**

For issues or questions, refer to the main documentation or contact the development team.
