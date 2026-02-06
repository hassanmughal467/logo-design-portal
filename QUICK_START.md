# ⚡ Quick Start Guide

## 🚀 Run Everything in 3 Steps

### Step 1: Start Backend (Terminal 1)
```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```
✅ Backend running on: `http://localhost:5000`

### Step 2: Start Frontend (Terminal 2 - NEW WINDOW)
```bash
cd Frontend
npm install  # First time only
npm start
```
✅ Frontend running on: `http://localhost:4200`

### Step 3: Test Login
1. Open: `http://localhost:4200`
2. Click "Register" or use existing:
   - **SuperAdmin:** `superadmin@logodesign.com` / `SuperAdmin@123`
3. Login and explore!

---

## 🔧 Quick Fixes

### Fix API Versioning (If Frontend Can't Connect)
Edit `Frontend/src/environments/environment.ts`:
```typescript
apiVersion: ''  // Remove 'v1'
```

### Fix Auth Response (If Login Fails)
Edit `Frontend/src/app/core/services/auth.service.ts`:
- Change `response.accessToken` to `response.token`
- Change `response.expiresIn` to use `response.expiresAt`

---

## 📋 What's Working

✅ **Authentication:** Login, Register, Logout  
✅ **Dashboard:** Basic UI with stats cards  
✅ **Navigation:** Sidebar, user menu, role-based menus  
✅ **Backend APIs:** All endpoints working via Swagger  

⚠️ **In Progress:** Users list, Orders list, File upload UI

---

## 🐛 Troubleshooting

**CORS Error?** → Backend CORS already configured  
**401 Error?** → Check token in browser DevTools  
**Blank Page?** → Check browser console for errors  
**Port in Use?** → Kill process or use different port  

---

## 📖 Full Guide

See `COMPLETE_TESTING_GUIDE.md` for detailed testing steps.
