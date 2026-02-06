# ✅ Pre-Push Checklist for TESTING_GUIDE.md

## 🔍 Steps to Follow Before Pushing

### 1. **Verify API Endpoint Paths**
- [ ] Check that all endpoints in TESTING_GUIDE.md match actual controller routes
- [ ] Current backend uses `/api/` prefix (not `/api/v1/`)
- [ ] Frontend is configured for `/api/v1/` - **NEEDS ALIGNMENT**

### 2. **Verify Response Formats**
- [ ] Check login/register response structure matches actual DTOs
- [ ] Verify order response includes all mentioned fields
- [ ] Confirm permission response structure is accurate

### 3. **Check API Versioning**
- [ ] Backend currently uses: `/api/auth`, `/api/orders`, etc.
- [ ] Frontend expects: `/api/v1/auth`, `/api/v1/orders`, etc.
- [ ] **DECISION NEEDED**: Update backend to `/api/v1/` OR update frontend to `/api/`

### 4. **Verify Authentication Response**
- [ ] Check if response uses `token` or `accessToken`
- [ ] Verify `refreshToken` field name
- [ ] Confirm `expiresAt` vs `expiresIn` field name

### 5. **Test All Endpoints Manually**
- [ ] Run backend locally
- [ ] Test each endpoint mentioned in guide
- [ ] Compare actual responses with documented responses
- [ ] Update guide if discrepancies found

### 6. **Check CORS Configuration**
- [ ] Verify CORS allows frontend origin (`http://localhost:4200`)
- [ ] Test frontend can call backend APIs

### 7. **Verify Default Credentials**
- [ ] Confirm SuperAdmin email: `superadmin@logodesign.com`
- [ ] Confirm SuperAdmin password: `SuperAdmin@123`
- [ ] Test login with these credentials

### 8. **Check File Upload Endpoints**
- [ ] Verify file upload endpoint path
- [ ] Confirm file size limits (10MB)
- [ ] Verify allowed file types

### 9. **Review Permission Endpoints**
- [ ] Verify permission endpoints exist
- [ ] Check grant/revoke endpoints
- [ ] Confirm role permission endpoints

### 10. **Update Documentation if Needed**
- [ ] Fix any endpoint path mismatches
- [ ] Update response examples to match actual API
- [ ] Add missing endpoints if any
- [ ] Remove deprecated endpoints if any

---

## ⚠️ Critical Issues Found

### Issue 1: API Versioning Mismatch
**Problem:**
- Backend uses: `/api/auth`, `/api/orders`
- Frontend expects: `/api/v1/auth`, `/api/v1/orders`
- TESTING_GUIDE.md documents: `/api/auth`, `/api/orders`

**Solution Options:**
1. **Update Backend** (Recommended for future-proofing):
   - Change all `[Route("api/[controller]")]` to `[Route("api/v1/[controller]")]`
   - Update TESTING_GUIDE.md to reflect `/api/v1/` paths

2. **Update Frontend** (Quick fix):
   - Change `apiVersion: 'v1'` to empty string
   - Update baseUrl construction in `api.service.ts`

**Recommendation:** Update backend to use `/api/v1/` for future API versioning support.

### Issue 2: Authentication Response Field Names
**Need to Verify:**
- Does backend return `token` or `accessToken`?
- Does backend return `expiresAt` or `expiresIn`?
- Frontend expects: `accessToken`, `expiresIn`

**Action:** Check actual AuthController response DTOs.

### Issue 3: Order Creation Request
**Need to Verify:**
- TESTING_GUIDE.md shows fields: `title`, `description`, `price`, `deadline`, `requirements`, `colorPreferences`, `stylePreferences`
- Frontend model has: `title`, `description`, `priority`, `dueDate`
- **Mismatch detected!**

**Action:** Verify actual Order DTO structure and align documentation.

---

## 📝 Required Changes

### Change 1: Add API Versioning Note
Add a section explaining current API versioning status:

```markdown
## 📌 API Versioning Note

**Current Status:** Backend uses `/api/` prefix (not versioned yet)
**Frontend Configuration:** Configured for `/api/v1/` (ready for versioning)

**For Testing:** Use `/api/` prefix in all requests
**Future:** Backend will be updated to `/api/v1/` for proper versioning
```

### Change 2: Verify and Update Response Examples
- [ ] Check actual response DTOs
- [ ] Update all response examples to match actual API responses
- [ ] Ensure field names are consistent

### Change 3: Add Frontend Integration Section
Add a new section:

```markdown
## 🌐 Frontend Integration

### API Base URL
- Development: `http://localhost:5000`
- Frontend configured for: `/api/v1/` (update backend or frontend)

### CORS
- Backend CORS configured to allow all origins in development
- Production: Update CORS policy to specific frontend domain
```

---

## 🧪 Testing Steps

### Step 1: Start Backend
```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```

### Step 2: Verify Swagger
- Open: `https://localhost:5001/swagger`
- Check all endpoints are listed
- Verify endpoint paths match documentation

### Step 3: Test Authentication Flow
1. Register a client
2. Login
3. Verify response structure matches guide
4. Test refresh token

### Step 4: Test Order Creation
1. Create order as client
2. Verify request/response match guide
3. Check field names

### Step 5: Test Permission Endpoints
1. Get all permissions
2. Get role permissions
3. Grant permission
4. Revoke permission

---

## ✅ Final Checklist Before Push

- [ ] All endpoint paths verified
- [ ] All response examples match actual API
- [ ] Authentication flow tested
- [ ] Order creation tested
- [ ] Permission endpoints tested
- [ ] File upload tested
- [ ] CORS configuration verified
- [ ] Default credentials tested
- [ ] API versioning decision made and documented
- [ ] Frontend integration notes added
- [ ] No typos or broken links
- [ ] All code examples are valid

---

## 🚀 After Push

1. **Update Backend** to use `/api/v1/` (if decision made)
2. **Update Frontend** API service if backend changes
3. **Test Full Integration** between frontend and backend
4. **Update TESTING_GUIDE.md** with any new findings

---

## 📌 Quick Reference

### Current Backend Endpoints
- Auth: `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh-token`
- Orders: `/api/orders`, `/api/orders/{id}`, `/api/orders/my-orders`
- Users: `/api/users`, `/api/users/{id}`
- Files: `/api/files/upload/{orderId}`, `/api/files/{id}/download`
- Permissions: `/api/permissions`, `/api/permissions/assign`

### Frontend Configuration
- Base URL: `http://localhost:5000`
- API Version: `v1` (needs backend update)
- Expected: `/api/v1/auth`, `/api/v1/orders`, etc.
