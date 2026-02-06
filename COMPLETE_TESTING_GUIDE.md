# 🚀 Complete Portal Testing Guide

## 📋 Overview

This guide will help you run and test the complete Logo Design Portal with both backend and frontend working together.

---

## ✅ Prerequisites Check

Before starting, ensure you have:

- [ ] .NET 8 SDK installed (`dotnet --version` should show 8.0.x)
- [ ] SQL Server LocalDB available (`sqllocaldb info` should work)
- [ ] Node.js 18+ installed (`node --version`)
- [ ] npm installed (`npm --version`)
- [ ] Backend project restored (`dotnet restore` completed)
- [ ] Frontend dependencies installed (`npm install` completed)

---

## 🔧 Step 1: Start the Backend API

### 1.1 Navigate to Backend
```bash
cd Backend/src/LogoDesignPortal.API
```

### 1.2 Start the API
```bash
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### 1.3 Verify Backend is Running
- Open browser: `https://localhost:5001/swagger`
- You should see Swagger UI with all API endpoints
- ✅ Backend is ready!

**Keep this terminal window open!**

---

## 🎨 Step 2: Start the Frontend

### 2.1 Open a NEW Terminal Window
(Keep backend running in the first terminal)

### 2.2 Navigate to Frontend
```bash
cd Frontend
```

### 2.3 Install Dependencies (First Time Only)
```bash
npm install
```

**Note:** This may take 2-5 minutes on first run.

### 2.4 Start Frontend Development Server
```bash
npm start
```

**Expected Output:**
```
✔ Browser application bundle generation complete.

Initial Chunk Files | Names         |  Size
main.js             | main          | ...
polyfills.js        | polyfills     | ...

** Angular Live Development Server is listening on localhost:4200 **
```

### 2.5 Verify Frontend is Running
- Browser should automatically open: `http://localhost:4200`
- If not, manually open: `http://localhost:4200`
- ✅ Frontend is ready!

**Keep this terminal window open too!**

---

## 🔗 Step 3: Verify Connection

### 3.1 Check CORS Configuration
- Backend CORS is configured to allow all origins in development
- Frontend should be able to call backend APIs
- If you see CORS errors, check backend `Program.cs` CORS configuration

### 3.2 Test API Connection
1. Open browser DevTools (F12)
2. Go to Network tab
3. Try to login/register from frontend
4. Check if API calls are being made to `http://localhost:5000/api/...`

---

## 🧪 Step 4: Complete Testing Flow

### Test Scenario 1: Client Registration & Login

#### 4.1 Register a New Client
1. **Frontend:** Navigate to `http://localhost:4200/register`
2. **Fill the form:**
   - Email: `client1@test.com`
   - Password: `Password123!`
   - First Name: `John`
   - Last Name: `Doe`
   - Company Name: `Acme Corporation`
   - Phone Number: `+1234567890`
3. **Click "Register"**
4. **Expected:**
   - ✅ Success message appears
   - ✅ Redirected to login page
   - ✅ User created in database

#### 4.2 Login as Client
1. **Frontend:** Navigate to `http://localhost:4200/login`
2. **Enter credentials:**
   - Email: `client1@test.com`
   - Password: `Password123!`
3. **Click "Login"**
4. **Expected:**
   - ✅ Success message appears
   - ✅ Redirected to dashboard
   - ✅ Sidebar shows menu items
   - ✅ User info displayed in top navbar

#### 4.3 Verify Dashboard
1. **After login, you should see:**
   - ✅ Dashboard page loads
   - ✅ Welcome message with user name
   - ✅ Stats cards (may show 0 for now)
   - ✅ Sidebar navigation visible

---

### Test Scenario 2: Login as SuperAdmin

#### 4.4 Login as SuperAdmin
1. **Logout** from current session (click user menu → Logout)
2. **Navigate to:** `http://localhost:4200/login`
3. **Enter SuperAdmin credentials:**
   - Email: `superadmin@logodesign.com`
   - Password: `SuperAdmin@123`
4. **Click "Login"**
5. **Expected:**
   - ✅ Login successful
   - ✅ Dashboard shows
   - ✅ Sidebar shows more menu items (Users, Orders, Designers, Permissions)

---

### Test Scenario 3: Create Order (Client)

#### 4.5 Create an Order
1. **Login as Client** (`client1@test.com`)
2. **Navigate to Orders** (from sidebar or `/orders`)
3. **Note:** Order creation UI may not be fully implemented yet
4. **Alternative - Test via Swagger:**
   - Go to: `https://localhost:5001/swagger`
   - Authorize with client token
   - Use `POST /api/orders` endpoint
   - Create an order

**Expected Order Request:**
```json
{
  "title": "Modern Tech Logo",
  "description": "Need a modern, minimalist logo for our tech startup",
  "price": 500.00,
  "deadline": "2024-12-31T00:00:00Z",
  "requirements": "Logo should work in both color and black/white",
  "colorPreferences": "Blue (#0066CC) and White",
  "stylePreferences": "Modern, minimalist, professional"
}
```

---

### Test Scenario 4: SuperAdmin Operations

#### 4.6 Create a Designer User (SuperAdmin)
1. **Login as SuperAdmin**
2. **Navigate to Users** (from sidebar)
3. **Note:** User creation UI may not be fully implemented yet
4. **Alternative - Test via Swagger:**
   - Go to: `https://localhost:5001/swagger`
   - Authorize with SuperAdmin token
   - Use `POST /api/users` endpoint

**Expected Request:**
```json
{
  "email": "designer1@test.com",
  "password": "Designer123!",
  "firstName": "Jane",
  "lastName": "Designer",
  "roleId": "33333333-3333-3333-3333-333333333333"
}
```

#### 4.7 Create Designer Profile
1. **In Swagger:** Use `POST /api/users/designer-profiles`
2. **Request:**
```json
{
  "userId": "designer-user-id-from-previous-step",
  "specialization": "Logo Design, Brand Identity",
  "bio": "Experienced logo designer with 5+ years",
  "hourlyRate": 50.00,
  "isAvailable": true
}
```

---

## 🔍 Step 5: Verify What's Working

### ✅ Currently Working Features

#### Frontend:
- ✅ **Authentication Pages:**
  - Login page (`/login`)
  - Register page (`/register`)
  - Form validation
  - Error handling

- ✅ **Layout & Navigation:**
  - Main layout with sidebar
  - Responsive design
  - User menu dropdown
  - Role-based menu items

- ✅ **Dashboard:**
  - Basic dashboard page
  - Stats cards (UI ready, data needs API integration)
  - Welcome message

- ✅ **Core Services:**
  - Auth service (login, register, token management)
  - API service (version-aware)
  - Permission service
  - Route guards (auth, role, permission)

- ✅ **HTTP Interceptors:**
  - Token injection
  - Error handling
  - Toast notifications

#### Backend:
- ✅ **Authentication:**
  - Login (`POST /api/auth/login`)
  - Register (`POST /api/auth/register`)
  - Refresh token (`POST /api/auth/refresh-token`)

- ✅ **Users:**
  - Create user (`POST /api/users`)
  - Get users (`GET /api/users`)
  - Get user by ID (`GET /api/users/{id}`)

- ✅ **Orders:**
  - Create order (`POST /api/orders`)
  - Get orders (`GET /api/orders`)
  - Get my orders (`GET /api/orders/my-orders`)
  - Get assigned orders (`GET /api/orders/assigned-orders`)
  - Assign order (`POST /api/orders/{id}/assign`)

- ✅ **Permissions:**
  - Get all permissions (`GET /api/permissions`)
  - Get role permissions (`GET /api/permissions/role/{roleId}`)
  - Grant permission (`POST /api/permissions/assign`)
  - Revoke permission (`DELETE /api/permissions/revoke`)

- ✅ **Files:**
  - Upload file (`POST /api/files/upload/{orderId}`)
  - Download file (`GET /api/files/{id}/download`)
  - Get order files (`GET /api/files/order/{orderId}`)

---

## ⚠️ Known Issues & Workarounds

### Issue 1: API Versioning Mismatch
**Problem:**
- Backend uses: `/api/auth`
- Frontend expects: `/api/v1/auth`

**Workaround:**
- Update `Frontend/src/environments/environment.ts`:
  ```typescript
  export const environment = {
    production: false,
    apiUrl: 'http://localhost:5000',
    apiVersion: ''  // Remove 'v1' for now
  };
  ```
- Update `Frontend/src/app/core/services/api.service.ts`:
  ```typescript
  private baseUrl = `${environment.apiUrl}/api${environment.apiVersion ? '/' + environment.apiVersion : ''}`;
  ```

### Issue 2: Authentication Response Field Names
**Problem:**
- Backend returns: `token`, `expiresAt`
- Frontend expects: `accessToken`, `expiresIn`

**Workaround:**
- Update `Frontend/src/app/core/services/auth.service.ts`:
  ```typescript
  // In setAuthData method, change:
  this.accessToken = response.accessToken;  // Change to: response.token
  this.tokenExpiry = Date.now() + (response.expiresIn * 1000);  // Change to use expiresAt
  ```

### Issue 3: Order Creation Model Mismatch
**Problem:**
- Backend requires: `price`, `deadline`
- Frontend model has: `priority`, `dueDate`

**Status:** Order creation UI not fully implemented yet. Use Swagger for now.

---

## 🧪 Step 6: Complete Test Checklist

### Authentication Flow
- [ ] Client registration works
- [ ] Client login works
- [ ] SuperAdmin login works
- [ ] Logout works
- [ ] Token stored correctly
- [ ] Auto-redirect on unauthorized access

### Navigation & Layout
- [ ] Sidebar shows correct menu items based on role
- [ ] User menu dropdown works
- [ ] Logout from user menu works
- [ ] Responsive design works (try resizing browser)

### Dashboard
- [ ] Dashboard loads after login
- [ ] Welcome message shows user name
- [ ] Stats cards display (even if 0)
- [ ] No console errors

### API Integration
- [ ] Login API call succeeds
- [ ] Register API call succeeds
- [ ] Token included in subsequent requests
- [ ] 401 errors handled gracefully
- [ ] Toast notifications appear

### Backend API (via Swagger)
- [ ] All endpoints listed in Swagger
- [ ] Authentication works in Swagger
- [ ] Can create orders
- [ ] Can create users (SuperAdmin)
- [ ] Can assign orders
- [ ] Can manage permissions

---

## 🐛 Troubleshooting

### Problem: Frontend can't connect to backend
**Solution:**
1. Verify backend is running on `http://localhost:5000`
2. Check CORS configuration in backend `Program.cs`
3. Check browser console for errors
4. Verify API URL in `environment.ts`

### Problem: CORS errors
**Solution:**
1. Backend CORS should allow all origins in development
2. Check `Program.cs` has:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowAll", policy =>
       {
           policy.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   ```

### Problem: 401 Unauthorized
**Solution:**
1. Check token is being sent in headers
2. Verify token format: `Bearer <token>`
3. Check token hasn't expired
4. Verify user is active in database

### Problem: Frontend shows blank page
**Solution:**
1. Check browser console for errors
2. Verify all dependencies installed: `npm install`
3. Check if Angular compiled successfully
4. Try clearing browser cache

### Problem: Port already in use
**Solution:**
- **Backend (5000/5001):**
  ```bash
  # Find process
  netstat -ano | findstr :5000
  # Kill process (replace PID)
  taskkill /PID <process-id> /F
  ```

- **Frontend (4200):**
  ```bash
  # Use different port
  ng serve --port 4201
  ```

---

## 📊 Testing Matrix

| Feature | Frontend | Backend | Status |
|---------|----------|---------|--------|
| Login | ✅ | ✅ | Working |
| Register | ✅ | ✅ | Working |
| Dashboard | ✅ | ⚠️ | UI ready, needs API |
| Users List | ⚠️ | ✅ | Backend ready |
| Orders List | ⚠️ | ✅ | Backend ready |
| Create Order | ❌ | ✅ | Use Swagger |
| File Upload | ❌ | ✅ | Use Swagger |
| Permissions | ❌ | ✅ | Use Swagger |

**Legend:**
- ✅ Fully working
- ⚠️ Partially implemented
- ❌ Not implemented yet

---

## 🎯 Quick Test Commands

### Test Backend Only
```bash
# Start backend
cd Backend/src/LogoDesignPortal.API
dotnet run

# Test in Swagger
# Open: https://localhost:5001/swagger
```

### Test Frontend Only
```bash
# Start frontend
cd Frontend
npm start

# Open: http://localhost:4200
```

### Test Both Together
1. Start backend in Terminal 1
2. Start frontend in Terminal 2
3. Test complete flow in browser

---

## 📝 Next Steps

After verifying everything works:

1. **Fix API Versioning:**
   - Update backend to `/api/v1/` OR
   - Update frontend to `/api/`

2. **Fix Authentication Response:**
   - Align field names between frontend and backend

3. **Complete Feature Implementations:**
   - Users list page
   - Orders list page
   - Order creation form
   - File upload UI
   - Permission matrix view

4. **Add API Integration:**
   - Connect dashboard to backend APIs
   - Implement role-aware data loading
   - Add error handling

---

## ✅ Success Criteria

You've successfully tested the portal if:

- ✅ Backend starts without errors
- ✅ Frontend starts without errors
- ✅ Can register a new client
- ✅ Can login as client
- ✅ Can login as SuperAdmin
- ✅ Dashboard loads after login
- ✅ Sidebar shows correct menu items
- ✅ No console errors in browser
- ✅ API calls are being made (check Network tab)
- ✅ Toast notifications appear on success/error

---

## 🎉 You're Ready!

If all the above works, your portal is successfully running and ready for further development!

**Happy Testing! 🚀**
