# 🧪 Complete Testing Guide - Frontend & Backend Alignment

## 📋 **Overview**

This guide covers testing all frontend features with the current backend implementation. Some features will work fully, while others will show empty states gracefully until backend endpoints are implemented.

---

## ✅ **Backend Endpoints Available**

### **Authentication** ✅
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Client registration
- `POST /api/auth/refresh-token` - Refresh token

### **Orders** ✅
- `GET /api/orders` - Get all orders (Admin/SuperAdmin)
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/my-orders` - Get client's orders
- `GET /api/orders/assigned-orders` - Get designer's assigned orders
- `POST /api/orders` - Create order (Client)
- `POST /api/orders/{id}/assign` - Assign order to designer
- `PUT /api/orders/{id}/status` - Update order status

### **Files** ✅
- `POST /api/files/upload/{orderId}` - Upload file
- `GET /api/files/{id}/download` - Download file
- `GET /api/files/order/{orderId}` - Get order files
- `DELETE /api/files/{id}` - Delete file

### **Users** ✅
- `GET /api/users` - Get all users (Admin/SuperAdmin)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user (SuperAdmin)

### **Permissions** ✅
- `GET /api/permissions` - Get permissions
- `POST /api/permissions/assign` - Assign permission
- `DELETE /api/permissions/revoke` - Revoke permission

---

## ⚠️ **Backend Endpoints Missing (Frontend Handles Gracefully)**

These features will show empty states or use mock data until backend is implemented:

- ❌ **Settings API** (`/api/settings/*`) - Settings page will save locally
- ❌ **Invoices API** (`/api/invoices/*`) - Invoice list will show empty state
- ❌ **Messages API** (`/api/messages/*`) - Messages will show empty state
- ❌ **Dashboard API** (`/api/dashboard`) - Dashboard calculates from orders/users

---

## 🚀 **Quick Start Testing**

### **Step 1: Start Backend**
```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```
✅ Wait for: `Now listening on: http://localhost:5000`

### **Step 2: Start Frontend**
```bash
cd Frontend
npm start
```
✅ Wait for: `Angular Live Development Server is listening on localhost:4200`

### **Step 3: Open Browser**
Navigate to: `http://localhost:4200`

---

## 🔐 **Test Users**

### **Default SuperAdmin**
- **Email:** `superadmin@logodesign.com`
- **Password:** `SuperAdmin@123`
- **Access:** Full system access

### **Create Test Client**
1. Go to `/register`
2. Register with:
   - Email: `client@test.com`
   - Password: `Test123!`
   - First Name: `John`
   - Last Name: `Doe`
   - Company: `Test Corp`

### **Create Test Designer (via Users Page)**
1. Login as SuperAdmin
2. Go to `/users`
3. Click "Create User"
4. Create Designer:
   - Email: `designer@test.com`
   - Password: `Test123!`
   - Role: `Designer`

---

## 📝 **Feature-by-Feature Testing**

### **1. Dashboard** ✅ **FULLY FUNCTIONAL**

**Test as SuperAdmin/Admin:**
1. Login as SuperAdmin
2. Navigate to `/dashboard`
3. **Verify:**
   - ✅ Statistics cards show (Total Orders, Clients, Revenue, etc.)
   - ✅ **Orders by Status** chart (Doughnut chart)
   - ✅ **Orders Trend** chart (Line chart - last 6 months)
   - ✅ **Revenue by Package** chart (Bar chart - Admin only)
   - ✅ Recent Orders table
   - ✅ Quick Actions section

**Test as Client:**
1. Login as Client
2. Navigate to `/dashboard`
3. **Verify:**
   - ✅ Client-specific stats (My Orders)
   - ✅ Order status breakdown
   - ✅ Recent orders
   - ✅ "New Order" button visible

**Test as Designer:**
1. Login as Designer
2. Navigate to `/dashboard`
3. **Verify:**
   - ✅ Assigned orders stats
   - ✅ Order status breakdown
   - ✅ Recent assigned orders

**Expected Behavior:**
- Charts appear when orders exist
- Empty states show when no data
- All cards are clickable and navigate correctly

---

### **2. Settings Page** ⚠️ **UI WORKS, BACKEND PENDING**

**Test Steps:**
1. Login as any user
2. Navigate to `/settings` (or click Settings in sidebar)
3. **Test Each Tab:**

#### **Business Info Tab:**
- ✅ Form fields display correctly
- ✅ Can enter business details
- ⚠️ Save will attempt API call (will fail gracefully)
- ✅ Form validation works

#### **Brand & Colors Tab:**
- ✅ Logo upload UI works
- ✅ Color pickers work
- ✅ Can select colors
- ⚠️ Upload will attempt API call (will fail gracefully)

#### **Invoice Template Tab:**
- ✅ All form fields work
- ✅ Checkboxes work
- ⚠️ Save will attempt API call (will fail gracefully)

#### **Payment Methods Tab:**
- ✅ Add Payment Method dialog works
- ✅ Form validation works
- ✅ Can add/edit/delete (locally)
- ⚠️ Save will attempt API call (will fail gracefully)

#### **Notifications Tab:**
- ✅ All toggle switches work
- ✅ Can change preferences
- ⚠️ Save will attempt API call (will fail gracefully)

**Expected Behavior:**
- All UI elements work
- Error messages show when API calls fail
- Data persists in component (until page refresh)

---

### **3. Orders Management** ✅ **FULLY FUNCTIONAL**

**Test Steps:**
1. Login as SuperAdmin/Admin
2. Navigate to `/orders`
3. **Verify Table:**
   - ✅ Orders list displays
   - ✅ Search works
   - ✅ Status filter works
   - ✅ Pagination works

4. **Test Action Buttons:**

#### **Assign Designer:**
- ✅ Click "Assign Designer" icon
- ✅ Dialog opens
- ✅ Designer dropdown populates
- ✅ Can select designer
- ✅ Click "Assign" - **Backend call works!**
- ✅ Success message shows
- ✅ Table refreshes

#### **Upload Files:**
- ✅ Click "Upload Files" icon
- ✅ Dialog opens
- ✅ File picker works
- ✅ Can select files
- ✅ Click upload - **Backend call works!**
- ✅ Success message shows

#### **Change Status:**
- ✅ Click "Change Status" icon
- ✅ Dialog opens
- ✅ Status dropdown works
- ✅ Can select new status
- ✅ Click "Update Status" - **Backend call works!**
- ✅ Success message shows
- ✅ Table refreshes

#### **Generate Invoice:**
- ✅ Click "Generate Invoice" icon (Admin only)
- ⚠️ Will attempt API call (will fail gracefully - invoice endpoint not implemented)

**Test as Client:**
1. Login as Client
2. Navigate to `/orders`
3. **Verify:**
   - ✅ Only sees own orders
   - ✅ "Create Order" button visible
   - ✅ Can view own orders

**Test as Designer:**
1. Login as Designer
2. Navigate to `/orders`
3. **Verify:**
   - ✅ Only sees assigned orders
   - ✅ Can upload files
   - ✅ Can change status

---

### **4. Clients Management** ✅ **FULLY FUNCTIONAL**

**Test Steps:**
1. Login as SuperAdmin/Admin
2. Navigate to `/clients`
3. **Verify:**
   - ✅ Clients list displays
   - ✅ Search works
   - ✅ Shows: Name, Email, Company, Total Orders, Total Spent
   - ✅ Click on client row → Navigates to client detail

#### **Client Detail Page:**
1. Click on any client
2. Navigate to `/clients/{id}`
3. **Verify Client Info Card:**
   - ✅ Shows client details
   - ✅ Total Orders, Total Spent
   - ✅ Member Since, Last Order

4. **Test Tabs:**

##### **Order History Tab:**
- ✅ Shows all client orders
- ✅ Order details display
- ✅ Click "View" → Navigates to order

##### **Invoices Tab:**
- ⚠️ Shows empty state (invoices endpoint not implemented)
- ✅ Empty state message displays

##### **Files Tab:**
- ✅ Shows client files (if any uploaded)
- ✅ Download button works
- ✅ File list displays

##### **Messages Tab:**
- ⚠️ Shows empty state (messages endpoint not implemented)
- ✅ Empty state message displays

---

### **5. Projects/Orders Detail** ✅ **FULLY FUNCTIONAL**

**Test Steps:**
1. Navigate to `/orders` or `/projects`
2. Click on any order/project
3. Navigate to `/projects/{id}` or `/orders/{id}`

**Verify Project Info:**
- ✅ Status badge displays
- ✅ Revisions Left counter
- ✅ Created/Updated dates
- ✅ Description (if available)

**Test Files & Revisions Tab:**
- ✅ Upload section displays
- ✅ File picker works
- ✅ Can enter revision notes
- ✅ Click "Upload Files" - **Backend call works!**
- ✅ Revisions list updates
- ✅ Can download files

**Test Comments Tab:**
- ✅ Comment form displays
- ✅ Can type comments
- ⚠️ Submit will attempt API call (will fail gracefully)
- ✅ Comments display (if any exist)

**Test Change Status:**
- ✅ Button visible
- ✅ Dialog opens
- ✅ Can select status
- ✅ Click "Update Status" - **Backend call works!**

---

### **6. Invoices** ⚠️ **UI WORKS, BACKEND PENDING**

**Test Steps:**
1. Login as SuperAdmin/Admin
2. Navigate to `/invoices`
3. **Verify Statistics Cards:**
   - ✅ Total Invoices card
   - ✅ Paid card (with amount)
   - ✅ Unpaid card (with pending amount)
   - ✅ Overdue card
   - ⚠️ Will show 0s (no invoice data yet)

4. **Verify Table:**
   - ⚠️ Shows empty state (invoices endpoint not implemented)
   - ✅ Empty state message displays
   - ✅ "Generate Invoice" button visible

**Expected Behavior:**
- UI fully functional
- Statistics calculate from empty array (shows 0s)
- Table shows helpful empty state

---

### **7. Users Management** ✅ **FULLY FUNCTIONAL**

**Test Steps:**
1. Login as SuperAdmin
2. Navigate to `/users`
3. **Verify:**
   - ✅ Users list displays
   - ✅ Search works
   - ✅ Role badges display
   - ✅ Status indicators work

4. **Test Create User:**
   - ✅ Click "Create User"
   - ✅ Dialog opens
   - ✅ Form validation works
   - ✅ Can select role
   - ✅ Click "Create" - **Backend call works!**
   - ✅ Success message shows
   - ✅ Table refreshes

5. **Test Actions:**
   - ✅ View button visible
   - ✅ Edit button visible
   - ⚠️ Edit functionality not implemented yet

---

### **8. Files Management** ✅ **FULLY FUNCTIONAL**

**Test Steps:**
1. Navigate to `/files`
2. **Verify:**
   - ✅ Files list displays (if any uploaded)
   - ✅ Can download files
   - ✅ Can delete files
   - ✅ File details show

**Expected Behavior:**
- Files uploaded via Orders/Projects appear here
- Download works
- Delete works (with confirmation)

---

### **9. Messages** ⚠️ **UI WORKS, BACKEND PENDING**

**Test Steps:**
1. Navigate to `/messages`
2. **Verify:**
   - ⚠️ Shows empty state (messages endpoint not implemented)
   - ✅ Empty state message displays
   - ✅ UI structure ready

---

### **10. Reviews** ⚠️ **UI WORKS, BACKEND PENDING**

**Test Steps:**
1. Navigate to `/reviews`
2. **Verify:**
   - ⚠️ Shows empty state (reviews endpoint not implemented)
   - ✅ Empty state message displays
   - ✅ UI structure ready

---

## 🔍 **Testing Checklist**

### **Core Features (Fully Working)**
- [ ] Dashboard - All charts and stats
- [ ] Orders - List, create, assign, change status
- [ ] Files - Upload, download, delete
- [ ] Users - List, create
- [ ] Clients - List, detail view
- [ ] Projects - Detail view, file upload

### **UI Features (Works, Backend Pending)**
- [ ] Settings - All tabs and forms
- [ ] Invoices - Statistics cards, table structure
- [ ] Messages - Empty state
- [ ] Reviews - Empty state

### **Role-Based Access**
- [ ] SuperAdmin - Full access
- [ ] Admin - Most features (no permissions management)
- [ ] Client - Own orders only
- [ ] Designer - Assigned orders only

---

## 🐛 **Known Limitations**

1. **Settings API** - Not implemented, data doesn't persist
2. **Invoices API** - Not implemented, shows empty state
3. **Messages API** - Not implemented, shows empty state
4. **Reviews API** - Not implemented, shows empty state
5. **Dashboard API** - Calculates from existing endpoints (works!)

---

## 🎯 **Quick Test Scenarios**

### **Scenario 1: Complete Order Workflow**
1. Login as Client
2. Create an order
3. Login as Admin
4. View order in `/orders`
5. Assign to Designer
6. Upload files
7. Change status to "InProgress"
8. View in Dashboard charts

### **Scenario 2: Client Management**
1. Login as Admin
2. View `/clients`
3. Click on client
4. View Order History tab
5. View Files tab
6. Check statistics

### **Scenario 3: Settings Configuration**
1. Login as Admin
2. Go to `/settings`
3. Fill Business Info
4. Upload logo
5. Set brand colors
6. Add payment method
7. Configure notifications

---

## 📊 **Expected Results Summary**

| Feature | Backend Status | Frontend Status | Test Result |
|---------|---------------|-----------------|-------------|
| Dashboard | ✅ Calculates | ✅ Works | ✅ PASS |
| Orders | ✅ Full API | ✅ Works | ✅ PASS |
| Files | ✅ Full API | ✅ Works | ✅ PASS |
| Users | ✅ Full API | ✅ Works | ✅ PASS |
| Clients | ✅ Uses Users API | ✅ Works | ✅ PASS |
| Projects | ✅ Uses Orders API | ✅ Works | ✅ PASS |
| Settings | ❌ Not Implemented | ✅ UI Works | ⚠️ UI Only |
| Invoices | ❌ Not Implemented | ✅ UI Works | ⚠️ UI Only |
| Messages | ❌ Not Implemented | ✅ UI Works | ⚠️ UI Only |
| Reviews | ❌ Not Implemented | ✅ UI Works | ⚠️ UI Only |

---

## 🚨 **Troubleshooting**

### **Charts Not Showing:**
- **Cause:** No orders in database
- **Solution:** Create some orders first

### **Empty States Everywhere:**
- **Cause:** No data in database
- **Solution:** 
  1. Create test users
  2. Create test orders
  3. Upload some files

### **API Errors in Console:**
- **Expected:** Settings, Invoices, Messages will show errors
- **Action:** These are handled gracefully, UI still works

### **Can't Login:**
- **Check:** Backend is running on port 5000
- **Check:** Database is initialized
- **Try:** Default SuperAdmin credentials

---

## ✅ **Success Criteria**

Your testing is successful if:
- ✅ Dashboard shows charts (when data exists)
- ✅ Orders can be created, assigned, and status changed
- ✅ Files can be uploaded and downloaded
- ✅ Users can be created
- ✅ Client detail page shows tabs
- ✅ Project detail page shows revisions and comments
- ✅ Settings page forms work (even if save fails)
- ✅ All navigation works
- ✅ Role-based access works correctly

---

## 🎉 **You're Ready to Test!**

Start with the Quick Start steps above and work through each feature. The frontend is designed to handle missing backend endpoints gracefully, so you can test the full UI even without all APIs implemented.

**Happy Testing!** 🚀
