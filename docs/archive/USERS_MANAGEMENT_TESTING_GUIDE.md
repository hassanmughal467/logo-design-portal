# 🧪 Users Management Testing Guide (Section 1.3)

## 📋 **Test Overview**
Testing the Users Management module as **SuperAdmin** to verify:
- ✅ Users list displays correctly
- ✅ Search functionality works
- ✅ Role badges display properly
- ✅ Status indicators work correctly

---

## 🚀 **Prerequisites**

1. **Backend is running** on `http://localhost:5000`
2. **Frontend is running** on `http://localhost:4200`
3. **You are logged in as SuperAdmin:**
   - Email: `superadmin@logodesign.com`
   - Password: `SuperAdmin@123`

---

## 📝 **Step-by-Step Testing Instructions**

### **Step 1: Navigate to Users Page**

1. After logging in as SuperAdmin, navigate to the Users page:
   - **Option A:** Click on "Users" in the sidebar menu
   - **Option B:** Type `/users` in the browser address bar
   - **Expected URL:** `http://localhost:4200/users`

2. **Verify Page Loads:**
   - ✅ Page title shows "Users Management"
   - ✅ Subtitle shows "Manage system users and their roles"
   - ✅ "Create User" button is visible (top right)

---

### **Step 2: Verify Users List Displays** ✅

**What to Check:**
1. **Table Structure:**
   - ✅ Table header shows columns: Checkbox, Name, Email, Role, Status, Created, Actions
   - ✅ At least one user row is visible (should see SuperAdmin user)
   - ✅ If no users exist, empty state message shows: "No users found"

2. **Table Features:**
   - ✅ Pagination controls appear at bottom (if more than 10 users)
   - ✅ Rows per page dropdown shows: 10, 25, 50
   - ✅ Table is responsive (check on mobile/tablet view)

3. **User Data Display:**
   - ✅ Name column shows: First Name + Last Name with user icon
   - ✅ Email column shows: Full email address
   - ✅ Created column shows: Formatted date (e.g., "Jan 15, 2024")
   - ✅ All columns are sortable (click column headers to test)

**Expected Result:** Users table displays with all columns and data visible.

---

### **Step 3: Test Search Functionality** 🔍

**Test Case 1: Search by Name**
1. Locate the search box at the top of the table (has search icon)
2. Type a user's first name (e.g., if user is "John Doe", type "John")
3. **Expected:** Table filters to show only matching users
4. Clear search and type last name (e.g., "Doe")
5. **Expected:** Table filters to show only matching users

**Test Case 2: Search by Email**
1. Type a user's email address (e.g., "superadmin@logodesign.com")
2. **Expected:** Table shows only that user
3. Type partial email (e.g., "superadmin")
4. **Expected:** Table shows all users with "superadmin" in email

**Test Case 3: Search by Role**
1. Type "Admin" in search box
2. **Expected:** Table shows only users with "Admin" in their role name
3. Type "Designer"
4. **Expected:** Table shows only Designer users

**Test Case 4: Clear Search**
1. Delete all text from search box
2. **Expected:** All users appear again

**Expected Result:** Search filters the table in real-time across Name, Email, and Role fields.

---

### **Step 4: Verify Role Badges** 🏷️

**What to Check:**
1. **Role Badge Display:**
   - ✅ Each user has a role badge in the "Role" column
   - ✅ Badges are rounded/pill-shaped
   - ✅ Badges show the role name (SuperAdmin, Admin, Designer, Client)

2. **Role Badge Colors:**
   - ✅ **SuperAdmin** → Red badge (danger severity)
   - ✅ **Admin** → Blue badge (info severity)
   - ✅ **Designer** → Gray badge (secondary severity)
   - ✅ **Client** → Green badge (success severity)

3. **Visual Verification:**
   - Look at the SuperAdmin user row → Should have red badge
   - If other users exist, verify their role badges match their roles

**Expected Result:** All role badges display with correct colors and text.

---

### **Step 5: Verify Status Indicators** 🟢🔴

**What to Check:**
1. **Status Badge Display:**
   - ✅ Each user has a status badge in the "Status" column
   - ✅ Badges are rounded/pill-shaped
   - ✅ Badges show either "Active" or "Inactive"

2. **Status Badge Colors:**
   - ✅ **Active** users → Green badge (success severity)
   - ✅ **Inactive** users → Red badge (danger severity)

3. **Visual Verification:**
   - Check SuperAdmin user → Should show green "Active" badge
   - If any inactive users exist, they should show red "Inactive" badge

**Expected Result:** Status indicators correctly show Active (green) or Inactive (red) for each user.

---

## ✅ **Testing Checklist**

Use this checklist to track your testing progress:

### **Basic Display**
- [ ] Users page loads successfully
- [ ] Page title and subtitle are visible
- [ ] "Create User" button is visible
- [ ] Users table displays with all columns
- [ ] At least one user (SuperAdmin) is visible in the table

### **Search Functionality**
- [ ] Search box is visible and functional
- [ ] Search by first name works
- [ ] Search by last name works
- [ ] Search by email works
- [ ] Search by role works
- [ ] Clearing search shows all users again

### **Role Badges**
- [ ] All users have role badges
- [ ] SuperAdmin badge is red
- [ ] Admin badge is blue (if exists)
- [ ] Designer badge is gray (if exists)
- [ ] Client badge is green (if exists)
- [ ] Badge text matches user's role

### **Status Indicators**
- [ ] All users have status badges
- [ ] Active users show green "Active" badge
- [ ] Inactive users show red "Inactive" badge (if any)
- [ ] Status badge text matches user's actual status

### **Table Features**
- [ ] Pagination works (if more than 10 users)
- [ ] Sorting works (click column headers)
- [ ] Rows per page dropdown works
- [ ] Table is responsive

---

## 🐛 **Common Issues & Solutions**

### **Issue: Users list is empty**
- **Check:** Are there users in the database?
- **Solution:** The SuperAdmin user should always exist. If not, check backend logs.

### **Issue: Search doesn't work**
- **Check:** Is the search box visible?
- **Check:** Are you typing in the correct search box (should be at top of table)?
- **Solution:** Refresh the page and try again.

### **Issue: Role badges not showing colors**
- **Check:** Are PrimeNG styles loaded?
- **Solution:** Check browser console for CSS errors.

### **Issue: Status always shows "Active"**
- **Check:** Are users actually active in the database?
- **Solution:** This might be expected if all users are active.

### **Issue: Can't see "Create User" button**
- **Check:** Are you logged in as SuperAdmin?
- **Solution:** Only SuperAdmin can see this button. Verify your role.

---

## 📸 **Visual Verification Points**

When testing, visually verify:

1. **Page Header:**
   ```
   Users Management
   Manage system users and their roles        [Create User]
   ```

2. **Table Header:**
   ```
   [Search box with icon]
   [☑] Name | Email | Role | Status | Created | Actions
   ```

3. **User Row Example:**
   ```
   [☑] 👤 John Doe | john@example.com | [Admin] | [Active] | Jan 15, 2024 | [👁️] [✏️] [🔑]
   ```

4. **Role Badge Colors:**
   - SuperAdmin: 🔴 Red
   - Admin: 🔵 Blue
   - Designer: ⚪ Gray
   - Client: 🟢 Green

5. **Status Badge Colors:**
   - Active: 🟢 Green
   - Inactive: 🔴 Red

---

## 🎯 **Success Criteria**

Your testing is successful if:
- ✅ Users list displays with all users visible
- ✅ Search filters users correctly in real-time
- ✅ Role badges show correct colors for each role
- ✅ Status indicators show correct colors (green for Active, red for Inactive)
- ✅ All table features work (pagination, sorting, etc.)

---

## 📝 **Test Results Template**

Fill this out as you test:

```
Date: ___________
Tester: ___________

✅ Users List Displays: [ ] Pass [ ] Fail
   Notes: _________________________________

✅ Search Works: [ ] Pass [ ] Fail
   Notes: _________________________________

✅ Role Badges Show: [ ] Pass [ ] Fail
   Notes: _________________________________

✅ Status Indicators Work: [ ] Pass [ ] Fail
   Notes: _________________________________

Issues Found:
1. _________________________________
2. _________________________________
3. _________________________________

Overall Result: [ ] All Tests Pass [ ] Some Tests Fail
```

---

## 🚀 **Next Steps**

After completing this testing:
1. Document any issues found
2. Proceed to test "Create User" functionality (Step 4 in the main guide)
3. Test with different user roles (Admin, Designer, Client)

---

**Happy Testing!** 🎉
