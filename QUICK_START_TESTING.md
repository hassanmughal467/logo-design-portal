# 🚀 Quick Start - Testing Guide

## ⚡ **3-Step Setup**

### **Step 1: Database Migration** (First Time Only)
```bash
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef migrations add AddMessagesReviewsSettings --startup-project ../LogoDesignPortal.API
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

### **Step 2: Start Backend**
```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```
✅ Wait for: `Now listening on: http://localhost:5000`

### **Step 3: Start Frontend**
```bash
cd Frontend
npm start
```
✅ Wait for: `Angular Live Development Server is listening on localhost:4200`

---

## 🎯 **Quick Test Flow**

### **1. Login**
- URL: `http://localhost:4200/login`
- Email: `superadmin@logodesign.com`
- Password: `SuperAdmin@123`

### **2. Test Dashboard**
- ✅ Check statistics cards
- ✅ Check charts (if orders exist)
- ✅ Check recent orders table

### **3. Test Settings**
- Navigate to `/settings`
- Fill Business Info → Save
- Upload Logo → Save
- Set Colors → Save
- Add Payment Method → Save
- Toggle Notifications → Save

### **4. Test Orders**
- Navigate to `/orders`
- Assign Designer → Test
- Upload Files → Test
- Change Status → Test
- Generate Invoice → Test

### **5. Test Invoices**
- Navigate to `/invoices`
- ✅ Statistics cards show
- ✅ Invoice list displays
- Generate Invoice → Test
- Download Invoice → Test

### **6. Test Clients**
- Navigate to `/clients`
- Click on client
- ✅ All tabs work (Orders, Invoices, Files, Messages)

### **7. Test Projects**
- Navigate to `/projects` or `/orders`
- Click on project/order
- ✅ Upload files → Test
- ✅ Add comments → Test
- ✅ Change status → Test

---

## 📋 **Module Testing Order**

Follow this order for systematic testing:

1. **Authentication** → Login/Register
2. **Dashboard** → Verify all stats and charts
3. **Settings** → Test all 5 tabs
4. **Orders** → Test all actions
5. **Clients** → Test list and detail
6. **Projects** → Test detail and actions
7. **Invoices** → Test generate and manage
8. **Messages** → Test send and view
9. **Reviews** → Test create and view
10. **Files** → Test upload and download

---

## ✅ **Success Indicators**

- ✅ No console errors
- ✅ All API calls succeed
- ✅ Data persists after save
- ✅ Navigation works smoothly
- ✅ Role-based access works
- ✅ Empty states show appropriately

---

**For detailed testing, see:** `MODULE_BY_MODULE_TESTING_GUIDE.md`
