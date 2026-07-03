# 🚀 Complete Run & Test Guide - Logo Design Portal

## 📋 Prerequisites Checklist

Before starting, ensure you have:
- ✅ .NET SDK 6.0+ installed (`dotnet --version`)
- ✅ Node.js 18+ installed (`node --version`)
- ✅ npm installed (`npm --version`)
- ✅ SQL Server LocalDB or SQL Server Express running
- ✅ Two terminal windows ready

---

## 🎯 STEP 1: Start Backend Server

### Terminal 1 - Backend

```bash
cd Backend/src/LogoDesignPortal.API
dotnet restore
dotnet run
```

**✅ Wait for this message:**
```
Now listening on: http://localhost:5000
```

**✅ Verify Backend is Running:**
- Open browser: `http://localhost:5000/swagger`
- You should see Swagger UI with all API endpoints
- ✅ **Backend is ready!**

**⚠️ Keep this terminal open!**

---

## 🎯 STEP 2: Start Frontend Server

### Terminal 2 - Frontend (NEW WINDOW)

```bash
cd Frontend
npm install  # Only if node_modules doesn't exist
npm start
```

**✅ Wait for this message:**
```
✓ Compiled successfully.
** Angular Live Development Server is listening on localhost:4200 **
```

**✅ Verify Frontend is Running:**
- Open browser: `http://localhost:4200`
- You should see the login page
- ✅ **Frontend is ready!**

**⚠️ Keep this terminal open!**

---

## 🧪 STEP 3: Complete Testing Flow

### Test 1: Login with SuperAdmin

1. **Open Browser:** `http://localhost:4200`
2. **You should see:** Login page with purple gradient background
3. **Enter Credentials:**
   - Email: `superadmin@logodesign.com`
   - Password: `SuperAdmin@123`
4. **Click "Login"**

**✅ Expected Results:**
- ✅ Success message appears
- ✅ Redirected to Dashboard (`/dashboard`)
- ✅ Sidebar navigation visible
- ✅ User menu in top-right corner
- ✅ Dashboard shows stats cards

**✅ Verify Dashboard:**
- Check sidebar has menu items
- Check top-right shows user email/name
- Check dashboard content loads

---

### Test 2: Register New Client

1. **Click "Register"** (or go to `/register`)
2. **Fill Registration Form:**
   - Email: `client1@test.com`
   - Password: `Password123!`
   - Confirm Password: `Password123!`
   - First Name: `John`
   - Last Name: `Doe`
   - Company Name: `Acme Corporation`
   - Phone Number: `+1234567890`
3. **Click "Register"**

**✅ Expected Results:**
- ✅ Success message: "Registration successful. Please login."
- ✅ Redirected to Login page
- ✅ Can login with new credentials

**✅ Test Login with New Client:**
- Login with: `client1@test.com` / `Password123!`
- Should redirect to Dashboard
- Should see Client-specific menu items

---

### Test 3: Test Navigation & Sidebar

**While logged in as SuperAdmin:**

1. **Check Sidebar Menu Items:**
   - ✅ Dashboard
   - ✅ Users (SuperAdmin only)
   - ✅ Orders
   - ✅ Files
   - ✅ Designers
   - ✅ Permissions (SuperAdmin only)

2. **Click Each Menu Item:**
   - ✅ Dashboard → Shows dashboard
   - ✅ Users → Shows users list (if implemented)
   - ✅ Orders → Shows orders list
   - ✅ Files → Shows files list
   - ✅ Designers → Shows designers list
   - ✅ Permissions → Shows permissions (if implemented)

3. **Check User Menu (Top-Right):**
   - ✅ Click user icon/name
   - ✅ Should show dropdown with:
     - Profile (if implemented)
     - Logout

---

### Test 4: Test Logout

1. **Click User Menu** (top-right)
2. **Click "Logout"**

**✅ Expected Results:**
- ✅ Redirected to Login page
- ✅ Token removed from storage
- ✅ Cannot access protected routes

**✅ Verify Token Removed:**
- Open Browser DevTools (F12)
- Go to Application → Local Storage
- Check that token is removed

---

### Test 5: Test API Endpoints via Swagger

1. **Open Swagger UI:** `http://localhost:5000/swagger`

2. **Test Authentication Endpoints:**

   **a) Register Client:**
   - Endpoint: `POST /api/auth/register`
   - Click "Try it out"
   - Use this body:
     ```json
     {
       "email": "swagger-client@test.com",
       "password": "Password123!",
       "firstName": "Swagger",
       "lastName": "Test",
       "companyName": "Swagger Corp",
       "phoneNumber": "+1234567890"
     }
     ```
   - Click "Execute"
   - ✅ Should return 201 with token

   **b) Login:**
   - Endpoint: `POST /api/auth/login`
   - Click "Try it out"
   - Use this body:
     ```json
     {
       "email": "superadmin@logodesign.com",
       "password": "SuperAdmin@123"
     }
     ```
   - Click "Execute"
   - ✅ Should return 200 with token
   - **Copy the token from response!**

3. **Authorize in Swagger:**
   - Click **"Authorize"** button (top-right, lock icon)
   - Enter: `Bearer <your-token-here>`
   - Click "Authorize"
   - Click "Close"
   - ✅ Lock icon should be unlocked

4. **Test Protected Endpoints:**

   **a) Get All Users (SuperAdmin):**
   - Endpoint: `GET /api/users`
   - Click "Try it out"
   - Click "Execute"
   - ✅ Should return list of users

   **b) Get All Orders:**
   - Endpoint: `GET /api/orders`
   - Click "Try it out"
   - Click "Execute"
   - ✅ Should return list of orders

   **c) Create Order (as Client):**
   - First, login as client and get token
   - Endpoint: `POST /api/orders`
   - Click "Try it out"
   - Use this body:
     ```json
     {
       "title": "Test Logo Order",
       "description": "Testing order creation",
       "price": 500.00,
       "deadline": "2024-12-31T00:00:00Z",
       "requirements": "Need vector format",
       "colorPreferences": "Blue and White",
       "stylePreferences": "Modern"
     }
     ```
   - Click "Execute"
   - ✅ Should return 201 with order details
   - **Save the order ID!**

---

### Test 6: Test File Upload (via Swagger)

1. **Create an Order First** (see Test 5)
2. **Get Order ID** from previous test
3. **File Upload:**
   - Endpoint: `POST /api/files/upload/{orderId}`
   - Replace `{orderId}` with your order ID
   - Click "Try it out"
   - Click "Choose File"
   - Select an image file (JPG, PNG, < 10MB)
   - Click "Execute"
   - ✅ Should return 200 with file details

4. **Get Order Files:**
   - Endpoint: `GET /api/files/order/{orderId}`
   - Replace `{orderId}` with your order ID
   - Click "Try it out"
   - Click "Execute"
   - ✅ Should return list of files for that order

---

### Test 7: Test Different User Roles

**Test as Client:**
1. Login as: `client1@test.com` / `Password123!`
2. ✅ Should see limited menu (Dashboard, Orders, Files)
3. ✅ Should NOT see Users, Designers, Permissions
4. ✅ Can create orders
5. ✅ Can view own orders

**Test as SuperAdmin:**
1. Login as: `superadmin@logodesign.com` / `SuperAdmin@123`
2. ✅ Should see all menu items
3. ✅ Can view all users
4. ✅ Can view all orders
5. ✅ Can assign orders to designers
6. ✅ Can manage permissions

---

### Test 8: Test Error Handling

1. **Invalid Login:**
   - Try login with wrong password
   - ✅ Should show error message
   - ✅ Should NOT redirect

2. **Invalid Registration:**
   - Try register with existing email
   - ✅ Should show error message
   - ✅ Should NOT redirect

3. **Unauthorized Access:**
   - Logout
   - Try to access `/dashboard` directly in URL
   - ✅ Should redirect to login

4. **Expired Token:**
   - Wait for token to expire (or manually remove from storage)
   - Try to access protected route
   - ✅ Should redirect to login

---

## ✅ Final Verification Checklist

### Backend Verification:
- ✅ Backend running on `http://localhost:5000`
- ✅ Swagger UI accessible at `http://localhost:5000/swagger`
- ✅ All API endpoints visible in Swagger
- ✅ Can register new user
- ✅ Can login and get token
- ✅ Can access protected endpoints with token
- ✅ Can create order
- ✅ Can upload file

### Frontend Verification:
- ✅ Frontend running on `http://localhost:4200`
- ✅ Login page displays correctly
- ✅ Can login with SuperAdmin credentials
- ✅ Dashboard loads after login
- ✅ Sidebar navigation works
- ✅ User menu works
- ✅ Logout works
- ✅ Registration works
- ✅ Error messages display correctly
- ✅ Protected routes redirect to login when not authenticated

### Integration Verification:
- ✅ Frontend can communicate with Backend
- ✅ Login from Frontend works
- ✅ Registration from Frontend works
- ✅ Token stored in browser storage
- ✅ Token sent with API requests
- ✅ CORS configured correctly (no CORS errors)

---

## 🐛 Troubleshooting

### Backend Issues:

**Problem: Port 5000 already in use**
```bash
# Find process using port 5000
netstat -ano | findstr :5000
# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

**Problem: Database connection error**
```bash
# Start SQL Server LocalDB
sqllocaldb start MSSQLLocalDB
# Check status
sqllocaldb info MSSQLLocalDB
```

**Problem: Swagger not loading**
- Check backend is running
- Check URL: `http://localhost:5000/swagger`
- Check browser console for errors

### Frontend Issues:

**Problem: Port 4200 already in use**
```bash
# Kill process on port 4200
netstat -ano | findstr :4200
taskkill /PID <PID> /F
# Or use different port
ng serve --port 4201
```

**Problem: Cannot connect to backend**
- Check backend is running
- Check CORS configuration
- Check API URL in `environment.ts`
- Check browser console for errors

**Problem: Login fails**
- Check backend is running
- Check credentials are correct
- Check browser DevTools → Network tab for API response
- Check token is being stored

**Problem: Blank page after login**
- Check browser console for errors
- Check token is stored in Local Storage
- Check routing configuration
- Check dashboard component exists

---

## 🎉 Success Criteria

You've successfully completed testing when:

✅ **Backend:**
- All API endpoints accessible via Swagger
- Can register, login, and get tokens
- Protected endpoints require authentication
- File upload works

✅ **Frontend:**
- Login and registration work
- Dashboard displays after login
- Navigation works correctly
- Logout works
- Error handling works

✅ **Integration:**
- Frontend communicates with Backend
- Authentication flow works end-to-end
- Token management works
- Protected routes work

---

## 📝 Next Steps

After successful testing:

1. **Explore Features:**
   - Test order creation from Frontend
   - Test file upload from Frontend
   - Test user management (if implemented)
   - Test order assignment (if implemented)

2. **Check Documentation:**
   - `START_HERE.md` - Quick reference
   - `QUICK_START.md` - Quick setup
   - `COMPLETE_TESTING_GUIDE.md` - Detailed API testing
   - `Backend/README.md` - Backend documentation

3. **Development:**
   - Start building new features
   - Fix any bugs found during testing
   - Add new components as needed

---

## 🎯 Quick Command Reference

```bash
# Backend
cd Backend/src/LogoDesignPortal.API
dotnet run

# Frontend
cd Frontend
npm start

# Swagger
http://localhost:5000/swagger

# Frontend App
http://localhost:4200

# Login Credentials
Email: superadmin@logodesign.com
Password: SuperAdmin@123
```

---

**🎉 You're all set! Happy testing! 🚀**
