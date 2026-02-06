# 🚀 Run and Test Complete Portal - Step by Step

## 📋 Prerequisites

Before starting, verify you have:
- ✅ .NET 8 SDK: `dotnet --version` (should show 8.0.x)
- ✅ Node.js 18+: `node --version`
- ✅ SQL Server LocalDB: `sqllocaldb info`

---

## 🔧 Step 1: Fix Frontend-Backend Compatibility

### Fix 1.1: API Versioning
**File:** `Frontend/src/environments/environment.ts`

**Change:**
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  apiVersion: ''  // Change from 'v1' to '' (empty string)
};
```

### Fix 1.2: Authentication Response Mapping
**File:** `Frontend/src/app/shared/models/user.model.ts`

**Change LoginResponse interface:**
```typescript
export interface LoginResponse {
  token: string;           // Changed from accessToken
  refreshToken?: string;
  user: User;
  expiresAt: string;       // Changed from expiresIn (number) to expiresAt (string/DateTime)
}
```

### Fix 1.3: Update Auth Service
**File:** `Frontend/src/app/core/services/auth.service.ts`

**Update setAuthData method (around line 93):**
```typescript
private setAuthData(response: LoginResponse): void {
  this.accessToken = response.token;  // Changed from response.accessToken
  // Parse expiresAt DateTime string
  const expiresAt = new Date(response.expiresAt);
  this.tokenExpiry = expiresAt.getTime();  // Changed from Date.now() + (response.expiresIn * 1000)
  
  // Store user in sessionStorage for persistence across page refreshes
  sessionStorage.setItem('user', JSON.stringify(response.user));
  this.currentUserSubject.next(response.user);
}
```

### Fix 1.4: Update API Service Base URL
**File:** `Frontend/src/app/core/services/api.service.ts`

**Update baseUrl construction:**
```typescript
private baseUrl = `${environment.apiUrl}/api${environment.apiVersion ? '/' + environment.apiVersion : ''}`;
```

This will create `/api/` when apiVersion is empty, matching the backend.

---

## 🚀 Step 2: Start Backend

### 2.1 Open Terminal 1
```bash
# Navigate to backend
cd "C:\Users\MuhammadHassan\Desktop\Web Portal\Backend\src\LogoDesignPortal.API"

# Start backend
dotnet run
```

### 2.2 Wait for Backend to Start
**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 2.3 Verify Backend
- Open browser: `https://localhost:5001/swagger`
- You should see Swagger UI with all endpoints
- ✅ **Backend is running!**

**⚠️ Keep this terminal open!**

---

## 🎨 Step 3: Start Frontend

### 3.1 Open Terminal 2 (NEW TERMINAL)
**Important:** Keep Terminal 1 running backend!

```bash
# Navigate to frontend
cd "C:\Users\MuhammadHassan\Desktop\Web Portal\Frontend"

# Install dependencies (first time only)
npm install
```

**Wait for installation to complete (2-5 minutes)**

### 3.2 Start Frontend
```bash
npm start
```

### 3.3 Wait for Frontend to Start
**Expected Output:**
```
✔ Browser application bundle generation complete.

** Angular Live Development Server is listening on localhost:4200 **
✔ Compiled successfully.
```

### 3.4 Verify Frontend
- Browser should auto-open: `http://localhost:4200`
- If not, manually open: `http://localhost:4200`
- You should see the login page
- ✅ **Frontend is running!**

**⚠️ Keep this terminal open too!**

---

## 🧪 Step 4: Test Complete Flow

### Test 4.1: Register a New Client

1. **On Frontend** (`http://localhost:4200`):
   - Click "Register here" link (or go to `/register`)
   - Fill the form:
     ```
     Email: client1@test.com
     Password: Password123!
     First Name: John
     Last Name: Doe
     Company Name: Acme Corporation
     Phone Number: +1234567890
     ```
   - Click "Register" button

2. **Expected Result:**
   - ✅ Green success toast: "Registration successful. Please login."
   - ✅ Redirected to login page
   - ✅ User created in database

3. **Verify in Backend:**
   - Check Swagger: `https://localhost:5001/swagger`
   - Use `GET /api/users` (with SuperAdmin token)
   - Should see new user in list

---

### Test 4.2: Login as Client

1. **On Frontend Login Page:**
   - Email: `client1@test.com`
   - Password: `Password123!`
   - Click "Login"

2. **Expected Result:**
   - ✅ Green success toast: "Login successful"
   - ✅ Redirected to dashboard (`/dashboard`)
   - ✅ Dashboard shows:
     - Welcome message: "Welcome back, John!"
     - Stats cards (may show 0)
     - Sidebar with menu items
   - ✅ Top navbar shows user name

3. **Verify in Browser DevTools:**
   - Press F12 → Network tab
   - Look for API calls to `http://localhost:5000/api/auth/login`
   - Should see 200 OK response
   - Check Application/Storage → Session Storage
   - Should see `user` object stored

---

### Test 4.3: Test Navigation

1. **Check Sidebar Menu:**
   - Should see: Dashboard, My Orders, Files
   - (Client role has limited menu)

2. **Click on Menu Items:**
   - Dashboard → Should load dashboard
   - My Orders → May show "Coming soon" (UI not fully implemented)
   - Files → May show "Coming soon"

3. **Test User Menu:**
   - Click user name in top-right
   - Should see dropdown: Profile, Logout
   - Click Logout → Should redirect to login

---

### Test 4.4: Login as SuperAdmin

1. **Logout** from current session (if logged in)

2. **Login with SuperAdmin:**
   - Email: `superadmin@logodesign.com`
   - Password: `SuperAdmin@123`
   - Click "Login"

3. **Expected Result:**
   - ✅ Login successful
   - ✅ Dashboard loads
   - ✅ Sidebar shows MORE menu items:
     - Dashboard
     - Users
     - Orders
     - Designers
     - Permissions
     - Files

4. **Verify SuperAdmin Access:**
   - Click "Users" in sidebar
   - Should navigate to users page (may show "Coming soon")
   - Click "Permissions" in sidebar
   - Should navigate to permissions page

---

### Test 4.5: Test API Integration (Via Swagger)

1. **Get SuperAdmin Token:**
   - In Swagger: `POST /api/auth/login`
   - Body:
     ```json
     {
       "email": "superadmin@logodesign.com",
       "password": "SuperAdmin@123"
     }
     ```
   - Copy the `token` from response

2. **Authorize in Swagger:**
   - Click "Authorize" button (top right)
   - Enter: `Bearer <your-token>`
   - Click "Authorize" → "Close"

3. **Test Create Order:**
   - Endpoint: `POST /api/orders`
   - Body:
     ```json
     {
       "title": "Test Logo Design",
       "description": "Testing order creation",
       "price": 500.00,
       "deadline": "2024-12-31T00:00:00Z"
     }
     ```
   - Should return 201 Created with order details

4. **Test Get All Orders:**
   - Endpoint: `GET /api/orders`
   - Should return list of orders

---

## ✅ Step 5: Verify Everything Works

### Checklist:

- [ ] Backend starts without errors
- [ ] Frontend starts without errors
- [ ] Can register new client
- [ ] Can login as client
- [ ] Can login as SuperAdmin
- [ ] Dashboard loads after login
- [ ] Sidebar shows correct menu (role-based)
- [ ] User menu dropdown works
- [ ] Logout works
- [ ] No console errors in browser
- [ ] API calls visible in Network tab
- [ ] Toast notifications appear
- [ ] Can create orders via Swagger
- [ ] Can view orders via Swagger

---

## 🐛 Troubleshooting

### Problem: Frontend shows blank page
**Solution:**
1. Check browser console (F12) for errors
2. Verify fixes in Step 1 were applied
3. Try: `npm install` again
4. Clear browser cache

### Problem: "Cannot connect to API"
**Solution:**
1. Verify backend is running (Terminal 1)
2. Check `http://localhost:5000` is accessible
3. Verify API URL in `environment.ts` is correct
4. Check CORS errors in browser console

### Problem: "401 Unauthorized"
**Solution:**
1. Check token is being sent (Network tab → Headers)
2. Verify token format: `Bearer <token>`
3. Try logging in again
4. Check backend logs for errors

### Problem: Login fails silently
**Solution:**
1. Check browser console for errors
2. Verify auth service fixes (Step 1.3)
3. Check Network tab for API response
4. Verify backend is running

### Problem: Port already in use
**Solution:**
- **Backend (5000):**
  ```bash
  netstat -ano | findstr :5000
  taskkill /PID <process-id> /F
  ```
- **Frontend (4200):**
  ```bash
  ng serve --port 4201
  ```

---

## 📊 What's Currently Working

### ✅ Fully Working:
- Backend API (all endpoints)
- Frontend authentication (login/register)
- Frontend layout & navigation
- Frontend dashboard (UI)
- Route guards
- HTTP interceptors
- Toast notifications
- Role-based menu

### ⚠️ Partially Working:
- Users list (backend ready, UI skeleton)
- Orders list (backend ready, UI skeleton)
- Files (backend ready, UI skeleton)

### ❌ Not Implemented Yet:
- Order creation form (use Swagger)
- File upload UI (use Swagger)
- Permission matrix view (use Swagger)
- User creation form (use Swagger)

---

## 🎯 Quick Test Summary

**Fastest way to verify everything:**

1. **Start Backend:** `cd Backend/src/LogoDesignPortal.API && dotnet run`
2. **Start Frontend:** `cd Frontend && npm start`
3. **Test Login:** Use SuperAdmin credentials
4. **Verify:** Dashboard loads, sidebar shows, no errors

**If all 4 steps work → ✅ Portal is running correctly!**

---

## 📝 Next Steps

After verifying everything works:

1. **Complete UI Implementations:**
   - Users list with data table
   - Orders list with filters
   - Order creation form
   - File upload UI

2. **Connect APIs:**
   - Dashboard stats from backend
   - Role-aware data loading
   - Real-time updates

3. **Add Features:**
   - Permission matrix visualization
   - Designer profile management
   - Order status workflow

---

## 🎉 Success!

If you can:
- ✅ Start both backend and frontend
- ✅ Register and login
- ✅ See dashboard
- ✅ Navigate between pages
- ✅ No console errors

**Then your portal is successfully running! 🚀**

For detailed testing scenarios, see `COMPLETE_TESTING_GUIDE.md`
