# 🚀 Quick Start - Testing Guide

## ⚡ **3-Step Setup**

> **Note:** `Frontend/src/environments/environment.ts` uses `apiUrl: 'http://localhost:5000'` for local backend. If you use IIS Express (port 61311), update it.

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

## 🏃 **Run Both (Two Terminals)**

**Terminal 1 - Backend:**
```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```
Wait for: `Now listening on: http://localhost:5000`

**Terminal 2 - Frontend:**
```bash
cd Frontend
npm start
```
Wait for: `localhost:4200`

Then open: **http://localhost:4200/login**

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

### **8. Test Notifications**
- **Admin sees new order:** Client creates order → Admin logs in → Bell shows "New Order Submitted"
- **Client sees updates:** Admin approves order / sends files → Client checks bell
- **Bell dropdown:** Click bell → View All (expands) → Show Less (collapses)
- **Notifications page:** `/notifications` → Mark read, Mark all read, pagination

### **9. Test Real-Time Notifications (SignalR)**
- **Two browsers:** Login as Admin in Browser A, login as Client (or different user) in Browser B
- **Trigger notification:** In Browser B, create an order or perform an action that notifies the Admin
- **Instant delivery:** Browser A should show a toast immediately (no page refresh) and the bell badge should update
- **Fallback:** If SignalR fails (check console for "SignalR connection failed"), polling runs every 60 seconds

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

## ⚠️ **Troubleshooting**

| Issue | Fix |
|-------|-----|
| API calls fail (CORS/404) | Ensure backend is running on port 5000; check `environment.ts` apiUrl |
| SignalR "connection failed" | Backend must be running; check browser console; polling fallback (60s) still works |
| MySQL connection error | Update `appsettings.json` ConnectionStrings; ensure MySQL is running |

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
