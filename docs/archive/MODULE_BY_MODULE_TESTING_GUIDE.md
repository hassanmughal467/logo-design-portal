# 🧪 Module-by-Module Testing Guide

## 📋 **Overview**

This guide provides step-by-step testing instructions for each module/feature. Test in order for the best experience.

---

## 🚀 **Setup & Prerequisites**

### **1. Start Backend**
```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```
✅ Wait for: `Now listening on: http://localhost:5000`

### **2. Create Database Migration** (First Time Only)
```bash
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef migrations add AddMessagesReviewsSettings --startup-project ../LogoDesignPortal.API
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

### **3. Start Frontend**
```bash
cd Frontend
npm start
```
✅ Wait for: `Angular Live Development Server is listening on localhost:4200`

### **4. Open Browser**
Navigate to: `http://localhost:4200`

---

## 👤 **Test Users**

### **Default SuperAdmin**
- **Email:** `superadmin@logodesign.com`
- **Password:** `SuperAdmin@123`

### **Create Test Users** (via Users page after login)

---

## 📦 **MODULE 1: Authentication & User Management**

### **1.1 Login**
**Steps:**
1. Navigate to `/login`
2. Enter SuperAdmin credentials
3. Click "Login"

**Expected:**
- ✅ Redirects to `/dashboard`
- ✅ Sidebar shows menu items
- ✅ Top navbar shows user name and role

**Test Cases:**
- [ ] Valid credentials → Success
- [ ] Invalid credentials → Error message
- [ ] Empty fields → Validation errors

---

### **1.2 Registration**
**Steps:**
1. Navigate to `/register`
2. Fill in all fields:
   - Email: `client@test.com`
   - Password: `Test123!`
   - First Name: `John`
   - Last Name: `Doe`
   - Company: `Test Corp`
   - Phone: `+1234567890`
3. Click "Register"

**Expected:**
- ✅ Success message
- ✅ Redirects to login
- ✅ Can login with new credentials

**Test Cases:**
- [ ] Valid data → Success
- [ ] Duplicate email → Error
- [ ] Weak password → Validation error
- [ ] Missing fields → Validation errors

---

### **1.3 Users Management** (SuperAdmin Only)
**Steps:**
1. Login as SuperAdmin
2. Navigate to `/users`
3. **Verify:**
   - ✅ Users list displays
   - ✅ Search works
   - ✅ Role badges show
   - ✅ Status indicators work

4. **Create User:**
   - Click "Create User"
   - Fill form:
     - Email: `designer@test.com`
     - Password: `Test123!`
     - First Name: `Jane`
     - Last Name: `Designer`
     - Role: `Designer`
   - Click "Create"

**Expected:**
- ✅ Success message
- ✅ New user appears in list
- ✅ Can login with new user

**Test Cases:**
- [ ] Create Designer → Success
- [ ] Create Admin → Success
- [ ] Create Client → Success
- [ ] Duplicate email → Error
- [ ] Invalid data → Validation errors

---

## 📊 **MODULE 2: Dashboard**

### **2.1 Admin Dashboard**
**Steps:**
1. Login as SuperAdmin/Admin
2. Navigate to `/dashboard`
3. **Verify Statistics Cards:**
   - ✅ Total Orders (clickable → goes to orders)
   - ✅ Pending Orders (clickable)
   - ✅ In Progress (clickable)
   - ✅ Completed (clickable)
   - ✅ Total Clients (clickable → goes to clients)
   - ✅ New Clients This Month (clickable)
   - ✅ Total Revenue (clickable → goes to invoices)
   - ✅ Average Delivery Time

4. **Verify Charts:**
   - ✅ **Orders by Status** (Doughnut chart)**
     - Shows status breakdown
     - Hover shows percentages
   - ✅ **Orders Trend** (Line chart)
     - Shows last 6 months
     - Smooth line with data points
   - ✅ **Revenue by Package** (Bar chart - Admin only)
     - Shows revenue by package type
     - Currency formatting in tooltips

5. **Verify Recent Orders Table:**
   - ✅ Shows latest orders
   - ✅ Status badges with colors
   - ✅ Clickable rows → navigate to order
   - ✅ "View All" button works

**Test Cases:**
- [ ] All cards display correctly
- [ ] Charts render (when data exists)
- [ ] Clickable cards navigate correctly
- [ ] Empty states show when no data

---

### **2.2 Client Dashboard**
**Steps:**
1. Login as Client
2. Navigate to `/dashboard`
3. **Verify:**
   - ✅ Client-specific stats only
   - ✅ "New Order" button visible
   - ✅ My Orders section
   - ✅ No admin charts (Revenue by Package)

**Test Cases:**
- [ ] Only client data shown
- [ ] Can create order from dashboard

---

### **2.3 Designer Dashboard**
**Steps:**
1. Login as Designer
2. Navigate to `/dashboard`
3. **Verify:**
   - ✅ Assigned orders stats
   - ✅ Order status breakdown
   - ✅ Recent assigned orders

**Test Cases:**
- [ ] Only assigned orders shown
- [ ] Can view assigned orders

---

## 📦 **MODULE 3: Orders Management**

### **3.1 View Orders List**
**Steps:**
1. Login as Admin
2. Navigate to `/orders`
3. **Verify:**
   - ✅ Orders table displays
   - ✅ Columns: Title, Status, Priority, Client, Designer, Created, Due Date, Actions
   - ✅ Search works
   - ✅ Status filter works
   - ✅ Pagination works
   - ✅ Sorting works

**Test Cases:**
- [ ] Table loads orders
- [ ] Search filters correctly
- [ ] Status filter works
- [ ] Pagination works

---

### **3.2 Create Order** (Client Only)
**Steps:**
1. Login as Client
2. Navigate to `/orders`
3. Click "Create Order"
4. Fill form (if create page exists) or use API directly

**Expected:**
- ✅ Order created
- ✅ Appears in orders list
- ✅ Status is "Pending"

---

### **3.3 Assign Designer**
**Steps:**
1. Login as Admin
2. Navigate to `/orders`
3. Find an order without designer
4. Click "Assign Designer" icon (👤+)
5. **In Dialog:**
   - Select a designer from dropdown
   - Click "Assign"

**Expected:**
- ✅ Success message
- ✅ Order shows assigned designer
- ✅ Designer can see order in their dashboard

**Test Cases:**
- [ ] Assign designer → Success
- [ ] Change designer → Success
- [ ] Cancel dialog → No changes

---

### **3.4 Upload Files**
**Steps:**
1. Navigate to `/orders`
2. Click "Upload Files" icon (📤)
3. **In Dialog:**
   - Click "Choose File"
   - Select a file (JPG, PNG, PDF, etc.)
   - Click upload

**Expected:**
- ✅ Success message
- ✅ File uploaded
- ✅ File appears in order files

**Test Cases:**
- [ ] Upload single file → Success
- [ ] Upload multiple files → Success (sequential)
- [ ] Invalid file type → Error
- [ ] File too large → Error

---

### **3.5 Change Order Status**
**Steps:**
1. Navigate to `/orders`
2. Click "Change Status" icon (✏️)
3. **In Dialog:**
   - Select new status from dropdown
   - Click "Update Status"

**Expected:**
- ✅ Success message
- ✅ Status badge updates
- ✅ Status history recorded

**Test Cases:**
- [ ] Change to InProgress → Success
- [ ] Change to Review → Success
- [ ] Change to Completed → Success
- [ ] Cancel → No changes

---

### **3.6 Generate Invoice**
**Steps:**
1. Login as Admin
2. Navigate to `/orders`
3. Find a completed order
4. Click "Generate Invoice" icon (📄)
5. **Expected:**
   - ✅ Success message
   - ✅ Invoice created
   - ✅ Redirects to invoices page

**Test Cases:**
- [ ] Generate from completed order → Success
- [ ] Invoice appears in invoices list
- [ ] Invoice has correct amount

---

## 👥 **MODULE 4: Clients Management**

### **4.1 View Clients List**
**Steps:**
1. Login as Admin
2. Navigate to `/clients`
3. **Verify:**
   - ✅ Clients table displays
   - ✅ Columns: Name, Email, Company, Total Orders, Total Spent, Created, Last Order
   - ✅ Search works
   - ✅ Sorting works
   - ✅ Clickable rows

**Test Cases:**
- [ ] Table loads clients
- [ ] Search filters correctly
- [ ] Click row → Navigates to detail

---

### **4.2 Client Detail Page**
**Steps:**
1. Navigate to `/clients`
2. Click on any client
3. Navigate to `/clients/{id}`

**Verify Client Info Card:**
- ✅ Company name
- ✅ Phone number
- ✅ Total Orders
- ✅ Total Spent (currency formatted)
- ✅ Member Since
- ✅ Last Order date
- ✅ Status badge

**Test Order History Tab:**
- ✅ Shows all client orders
- ✅ Order details (ID, Title, Status, Amount, Dates)
- ✅ Click "View" → Navigates to order
- ✅ Empty state if no orders

**Test Invoices Tab:**
- ✅ Shows client invoices
- ✅ Invoice details (Number, Amount, Status, Dates)
- ✅ Click "View" → Navigates to invoice
- ✅ Empty state if no invoices

**Test Files Tab:**
- ✅ Shows client files
- ✅ File details (Name, Upload date)
- ✅ Download button works
- ✅ Empty state if no files

**Test Messages Tab:**
- ✅ Shows messages with client
- ✅ Message details (Sender, Content, Date)
- ✅ Empty state if no messages

**Test Cases:**
- [ ] All tabs load correctly
- [ ] Navigation works
- [ ] Empty states display properly
- [ ] Data displays correctly

---

## 🎨 **MODULE 5: Projects/Orders Detail**

### **5.1 View Project Detail**
**Steps:**
1. Navigate to `/orders` or `/projects`
2. Click on any order/project
3. Navigate to `/projects/{id}`

**Verify Project Info:**
- ✅ Status badge
- ✅ Revisions Left counter
- ✅ Created/Updated dates
- ✅ Description

---

### **5.2 Files & Revisions Tab**
**Steps:**
1. Open project detail
2. Go to "Files & Revisions" tab

**Test Upload Section:**
- ✅ File picker works
- ✅ Can select multiple files
- ✅ Revision notes textarea works
- ✅ Click "Upload Files"

**Expected:**
- ✅ Success message
- ✅ Files upload (one by one)
- ✅ New revision appears in list

**Test Revisions List:**
- ✅ Shows all revisions
- ✅ Version numbers
- ✅ Revision notes
- ✅ File list per revision
- ✅ Download buttons work

**Test Cases:**
- [ ] Upload single file → Success
- [ ] Upload multiple files → Success
- [ ] Add revision notes → Saved
- [ ] Download files → Works
- [ ] Empty state shows

---

### **5.3 Comments Tab**
**Steps:**
1. Open project detail
2. Go to "Comments" tab

**Test Add Comment:**
- ✅ Comment textarea works
- ✅ Type a comment
- ✅ Click "Post Comment"

**Expected:**
- ✅ Success message
- ✅ Comment appears in list
- ✅ Shows author, role, timestamp

**Test Comments List:**
- ✅ Shows all comments
- ✅ Author info (name, role)
- ✅ Timestamp
- ✅ Content displays

**Test Cases:**
- [ ] Add comment → Success
- [ ] Comments display correctly
- [ ] Empty state shows
- [ ] Timestamps format correctly

---

### **5.4 Change Status**
**Steps:**
1. Open project detail
2. Click "Change Status" button
3. **In Dialog:**
   - Select new status
   - Click "Update Status"

**Expected:**
- ✅ Success message
- ✅ Status badge updates
- ✅ Project info refreshes

---

## 💰 **MODULE 6: Invoices**

### **6.1 View Invoices List**
**Steps:**
1. Login as Admin
2. Navigate to `/invoices`
3. **Verify Statistics Cards:**
   - ✅ Total Invoices
   - ✅ Paid (with amount)
   - ✅ Unpaid (with pending amount)
   - ✅ Overdue

4. **Verify Table:**
   - ✅ Invoice Number
   - ✅ Client Name
   - ✅ Amount (currency formatted)
   - ✅ Status (Paid/Unpaid/Overdue)
   - ✅ Due Date
   - ✅ Actions (Download, Send)

**Test Cases:**
- [ ] Statistics calculate correctly
- [ ] Table displays invoices
- [ ] Status badges show correct colors
- [ ] Overdue highlighting works

---

### **6.2 Generate Invoice**
**Steps:**
1. Navigate to `/orders`
2. Find a completed order
3. Click "Generate Invoice"
4. **Expected:**
   - ✅ Invoice created
   - ✅ Appears in invoices list
   - ✅ Correct amount

---

### **6.3 Download Invoice**
**Steps:**
1. Navigate to `/invoices`
2. Click "Download" icon (📥)
3. **Expected:**
   - ✅ PDF downloads
   - ✅ Or opens in new tab

---

### **6.4 Send Invoice**
**Steps:**
1. Navigate to `/invoices`
2. Click "Send" icon (📧)
3. **Expected:**
   - ✅ Success message
   - ✅ Invoice sent (email functionality)

---

### **6.5 Mark Invoice as Paid**
**Steps:**
1. Navigate to `/invoices`
2. Find unpaid invoice
3. Use API or add UI button to mark as paid
4. **Expected:**
   - ✅ Status changes to "Paid"
   - ✅ Paid date set
   - ✅ Statistics update

---

## ⚙️ **MODULE 7: Settings**

### **7.1 Business Info Tab**
**Steps:**
1. Login as Admin
2. Navigate to `/settings`
3. Go to "Business Info" tab
4. **Fill Form:**
   - Business Name: `Logo Design Co.`
   - Business Email: `info@logodesign.com`
   - Business Phone: `+1 (555) 123-4567`
   - Website: `https://logodesign.com`
   - Address: `123 Main St, City, State 12345`
   - Tax ID: `TAX-123456`
5. Click "Save Business Info"

**Expected:**
- ✅ Success message
- ✅ Data saves to database
- ✅ Can reload and see saved data

**Test Cases:**
- [ ] Save business info → Success
- [ ] Reload page → Data persists
- [ ] Validation works
- [ ] Required fields enforced

---

### **7.2 Brand & Colors Tab**
**Steps:**
1. Go to "Brand & Colors" tab
2. **Upload Logo:**
   - Click "Choose Logo"
   - Select image file
   - Click "Upload Logo"
3. **Set Colors:**
   - Primary Color: `#6366f1`
   - Secondary Color: `#8b5cf6`
   - Accent Color: `#10b981`
4. Click "Save Brand Settings"

**Expected:**
- ✅ Logo uploads
- ✅ Preview shows
- ✅ Colors save
- ✅ Success message

**Test Cases:**
- [ ] Upload logo → Success
- [ ] Color pickers work
- [ ] Save settings → Success
- [ ] Data persists

---

### **7.3 Invoice Template Tab**
**Steps:**
1. Go to "Invoice Template" tab
2. **Fill Form:**
   - Header Text: `Thank you for your business!`
   - Footer Text: `Payment due within 30 days.`
   - Terms & Conditions: `[Your terms here]`
   - Check "Show logo on invoices"
   - Check "Show Tax ID on invoices"
3. Click "Save Invoice Template"

**Expected:**
- ✅ Success message
- ✅ Settings save
- ✅ Data persists

---

### **7.4 Payment Methods Tab**
**Steps:**
1. Go to "Payment Methods" tab
2. Click "Add Payment Method"
3. **Fill Dialog:**
   - Name: `PayPal`
   - Type: `PayPal`
   - Account Details: `payments@logodesign.com`
   - Check "Active"
4. Click "Save"

**Expected:**
- ✅ Payment method added
- ✅ Appears in list
- ✅ Can edit/delete

**Test Cases:**
- [ ] Add payment method → Success
- [ ] Edit payment method → Success
- [ ] Delete payment method → Success
- [ ] Multiple methods → All display

---

### **7.5 Notifications Tab**
**Steps:**
1. Go to "Notifications" tab
2. **Toggle Switches:**
   - Email Notifications: ON
   - Order Notifications: ON
   - Payment Notifications: ON
   - Client Message Notifications: ON
   - Review Notifications: ON
3. Click "Save Notification Preferences"

**Expected:**
- ✅ Success message
- ✅ Preferences save
- ✅ Data persists

---

## 💬 **MODULE 8: Messages**

### **8.1 View Messages List**
**Steps:**
1. Navigate to `/messages`
2. **Verify:**
   - ✅ Messages list displays
   - ✅ Shows sender, content, date
   - ✅ Unread indicators
   - ✅ Empty state if no messages

**Test Cases:**
- [ ] Messages load
- [ ] Empty state shows
- [ ] Unread badges work

---

### **8.2 Send Message**
**Steps:**
1. Navigate to project detail
2. Go to Comments tab (messages are comments)
3. Type message
4. Click "Post Comment"

**Expected:**
- ✅ Message sent
- ✅ Appears in messages list
- ✅ Recipient can see it

---

### **8.3 Mark as Read**
**Steps:**
1. Navigate to `/messages`
2. Click on unread message
3. **Expected:**
   - ✅ Message marked as read
   - ✅ Unread indicator removed

---

## ⭐ **MODULE 9: Reviews**

### **9.1 View Reviews List**
**Steps:**
1. Navigate to `/reviews`
2. **Verify:**
   - ✅ Reviews list displays
   - ✅ Shows client, rating, comment, date
   - ✅ Star ratings display
   - ✅ Empty state if no reviews

**Test Cases:**
- [ ] Reviews load
- [ ] Ratings display correctly
- [ ] Empty state shows

---

### **9.2 Create Review** (Client Only)
**Steps:**
1. Login as Client
2. Navigate to completed order
3. Add review (if UI exists) or use API
4. **Fill:**
   - Rating: `5` (1-5 stars)
   - Comment: `Great work!`
5. Submit

**Expected:**
- ✅ Review created
- ✅ Appears in reviews list
- ✅ Shows on order/project

---

## 📁 **MODULE 10: Files Management**

### **10.1 View Files List**
**Steps:**
1. Navigate to `/files`
2. **Verify:**
   - ✅ Files list displays
   - ✅ Shows file name, order, upload date
   - ✅ Download buttons work
   - ✅ Delete buttons work

**Test Cases:**
- [ ] Files load
- [ ] Download works
- [ ] Delete works (with confirmation)
- [ ] Empty state shows

---

## 🔍 **MODULE 11: Search & Navigation**

### **11.1 Global Search**
**Steps:**
1. Use search bar in top navbar
2. Type search term
3. Press Enter

**Expected:**
- ✅ Navigates to relevant page
- ✅ Search filters applied
- ✅ Results show

**Test Cases:**
- [ ] Search orders → Works
- [ ] Search clients → Works
- [ ] Search invoices → Works

---

### **11.2 Sidebar Navigation**
**Steps:**
1. Click sidebar items
2. **Verify:**
   - ✅ All menu items work
   - ✅ Role-based items show/hide correctly
   - ✅ Active state highlights
   - ✅ Collapsible works

**Test Cases:**
- [ ] All links navigate correctly
- [ ] Role-based visibility works
- [ ] Active state works

---

### **11.3 Notifications**
**Steps:**
1. Click notification bell
2. **Expected:**
   - ✅ Notification dropdown opens
   - ✅ Shows notifications (when implemented)
   - ✅ Badge shows count

---

## ✅ **Complete Testing Checklist**

### **Authentication & Users**
- [ ] Login works
- [ ] Registration works
- [ ] Create user works
- [ ] User list displays
- [ ] Role-based access works

### **Dashboard**
- [ ] Statistics cards display
- [ ] Charts render
- [ ] Recent orders show
- [ ] Role-based data works

### **Orders**
- [ ] List displays
- [ ] Create order works
- [ ] Assign designer works
- [ ] Upload files works
- [ ] Change status works
- [ ] Generate invoice works

### **Clients**
- [ ] List displays
- [ ] Detail page works
- [ ] All tabs load
- [ ] Navigation works

### **Projects**
- [ ] Detail page works
- [ ] File upload works
- [ ] Comments work
- [ ] Status change works

### **Invoices**
- [ ] Statistics cards work
- [ ] List displays
- [ ] Generate works
- [ ] Download works
- [ ] Send works

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

### **Files**
- [ ] List displays
- [ ] Download works
- [ ] Delete works

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

### **Can't Login**
- **Check:** Backend on port 5000
- **Check:** Database exists
- **Try:** Default SuperAdmin credentials

---

## 🎯 **Testing Priority**

### **Must Test (Critical)**
1. ✅ Login/Registration
2. ✅ Dashboard
3. ✅ Orders (all actions)
4. ✅ Files upload/download
5. ✅ Settings save

### **Should Test (Important)**
6. ✅ Client detail page
7. ✅ Project detail page
8. ✅ Invoices
9. ✅ Users management

### **Nice to Test (Optional)**
10. ✅ Messages
11. ✅ Reviews
12. ✅ Search functionality

---

## 📊 **Success Criteria**

Your testing is successful if:
- ✅ All core features work end-to-end
- ✅ Data persists after save
- ✅ Role-based access works correctly
- ✅ Navigation is smooth
- ✅ Empty states show appropriately
- ✅ Error handling works
- ✅ Forms validate correctly

---

## 🎉 **You're Ready!**

Start with Module 1 and work through each module systematically. Take notes of any issues you find.

**Happy Testing!** 🚀
