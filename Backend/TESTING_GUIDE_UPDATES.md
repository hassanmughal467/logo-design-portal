# 🔄 Required Updates for TESTING_GUIDE.md

## Changes Needed Before Push

### 1. Add API Versioning Section (Line ~150, after "Access Swagger")

**Add this section:**

```markdown
### API Versioning Note
**Current Status:** The backend currently uses `/api/` prefix (not versioned).
**Frontend Configuration:** The frontend is configured for `/api/v1/` to support future API versioning.

**For Testing:** Use `/api/` prefix in all Swagger/Postman requests.
**Future:** Backend will be updated to `/api/v1/` for proper versioning support.

**Example Endpoints:**
- Current: `POST /api/auth/login`
- Future: `POST /api/v1/auth/login`
```

### 2. Update Authentication Response Example (Line ~488)

**Current shows:**
```json
{
  "token": "...",
  "refreshToken": "...",
  "expiresAt": "...",
  "user": {...}
}
```

**Need to verify actual response structure** - Check if it's:
- `token` or `accessToken`?
- `expiresAt` or `expiresIn`?

**Action:** Check `LoginResponse` DTO in backend code.

### 3. Verify Order Creation Request (Line ~572)

**Current shows:**
```json
{
  "title": "...",
  "description": "...",
  "price": 500.00,
  "deadline": "2024-12-31T00:00:00Z",
  "requirements": "...",
  "colorPreferences": "...",
  "stylePreferences": "..."
}
```

**Frontend model expects:**
```typescript
{
  title: string;
  description: string;
  priority: OrderPriority;
  dueDate?: Date;
}
```

**Action:** Verify actual `CreateOrderRequest` DTO structure.

### 4. Add Frontend Integration Section (After "Troubleshooting")

**Add new section:**

```markdown
## 🌐 Frontend Integration

### Connecting Frontend to Backend

1. **Start Backend:**
   ```bash
   cd Backend/src/LogoDesignPortal.API
   dotnet run
   ```
   Backend runs on: `http://localhost:5000`

2. **Start Frontend:**
   ```bash
   cd Frontend
   npm install
   npm start
   ```
   Frontend runs on: `http://localhost:4200`

3. **API Configuration:**
   - Backend API: `http://localhost:5000/api/`
   - Frontend expects: `http://localhost:5000/api/v1/`
   - **Note:** Update backend to `/api/v1/` OR update frontend environment to remove version

4. **CORS:**
   - Backend CORS is configured to allow all origins in development
   - Frontend can call backend APIs without CORS issues

### Testing Frontend-Backend Integration

1. Register a client via frontend
2. Login via frontend
3. Create an order via frontend
4. Verify data appears in backend database
5. Test all CRUD operations
```

### 5. Update cURL Examples (Line ~1129)

**Current shows:**
```bash
curl -X POST "https://localhost:5001/api/auth/register"
```

**Should be:**
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
  -k  # Skip SSL verification for localhost
```

**Or if using HTTPS:**
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -k  # Skip SSL verification for localhost
```

### 6. Add Missing Phone Number in Registration cURL (Line ~1130)

**Current shows:**
```json
{
  "email": "test@test.com",
  "password": "Test123!",
  "firstName": "Test",
  "lastName": "User",
  "companyName": "Test Corp"
}
```

**Should include `phoneNumber`:**
```json
{
  "email": "test@test.com",
  "password": "Test123!",
  "firstName": "Test",
  "lastName": "User",
  "companyName": "Test Corp",
  "phoneNumber": "+1234567890"
}
```

### 7. Verify Permission Endpoints Exist

**Check if these endpoints actually exist:**
- `GET /api/permissions` ✅
- `GET /api/permissions/role/{roleId}` ✅
- `POST /api/permissions/assign` ✅
- `DELETE /api/permissions/revoke` ✅

**Action:** Verify in PermissionsController.

### 8. Add Note About Designer Profile Endpoint

**Current shows:** `POST /api/users/designer-profiles`

**Verify if this is correct or if it should be:**
- `POST /api/designers` or
- `POST /api/users/{userId}/designer-profile` or
- Current is correct

**Action:** Check UsersController for designer profile endpoint.

---

## Summary of Required Actions

1. ✅ Add API versioning note
2. ⚠️ Verify authentication response structure
3. ⚠️ Verify order creation request structure
4. ✅ Add frontend integration section
5. ✅ Update cURL examples
6. ✅ Fix registration cURL example
7. ⚠️ Verify permission endpoints
8. ⚠️ Verify designer profile endpoint

**Priority:**
- **High:** Verify response/request structures (#2, #3)
- **Medium:** Add frontend integration (#4)
- **Low:** Fix examples and add notes (#1, #5, #6)
