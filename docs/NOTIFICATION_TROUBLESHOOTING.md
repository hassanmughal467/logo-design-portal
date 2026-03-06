# Notification Troubleshooting Guide

If notifications are not showing, follow these checks:

---

## 1. Restart the Backend

**Critical:** After any backend changes (including the "New Order Submitted" notification), restart the API:

```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```

---

## 2. Verify API is Working

Open browser DevTools (F12) → Network tab. When logged in as Admin:

- **GET** `https://your-api-url/api/notifications` → Should return 200 with JSON array
- **GET** `https://your-api-url/api/notifications/unread-count` → Should return 200 with `{ "count": N }`

If you see **401 Unauthorized**:
- Token may be expired → Log out and log back in
- Check that `Authorization: Bearer <token>` header is sent (TokenInterceptor)

---

## 3. Use the Diagnostic Endpoint (SuperAdmin Only)

**Before creating an order**, call the debug endpoint to verify the system is ready:

```
GET /api/notifications/debug
Authorization: Bearer <your-superadmin-token>
```

**Expected response when OK:**
```json
{
  "adminRoleCount": 2,
  "adminUserCount": 1,
  "notificationCount": 0,
  "message": "OK"
}
```

**If `adminUserCount` is 0:**
- Notifications will NOT be created when clients submit orders
- Run this SQL to verify user roles:
  ```sql
  SELECT u.Id, u.Email, u.RoleId, r.Name as RoleName
  FROM Users u
  JOIN Roles r ON u.RoleId = r.Id
  WHERE u.IsDeleted = 0;
  ```
- Ensure SuperAdmin/Admin users have `RoleId` matching the Admin or SuperAdmin role

---

## 4. Verify Notifications Are Created

When a **Client creates an order**, the backend creates notifications for all Admin and SuperAdmin users.

**Check the database:**
```sql
SELECT Id, UserId, Title, IsRead, CreatedAt FROM Notifications WHERE IsDeleted = 0 ORDER BY CreatedAt DESC;
```

If the table is empty after creating an order:
- **Restart the backend** – ensure you're running the latest build
- Check backend logs for: `"New order X created but no Admin/SuperAdmin users found"` (indicates adminUserCount = 0)
- Ensure you're hitting the correct backend (local vs deployed – see environment.ts `apiUrl`)

---

## 5. Check Environment / API URL

In `Frontend/src/environments/environment.ts`:
- `apiUrl` must point to your running backend (e.g. `http://localhost:5000` or `http://api.hawkmerchandising.com`)
- CORS must allow your frontend origin

---

## 6. Where Notifications Appear

| Location | Who Sees It |
|----------|-------------|
| **Bell icon (navbar)** | All logged-in users |
| **Bell dropdown** | Click bell → latest 5, "View All" expands |
| **Dashboard panel** | Admin/SuperAdmin + Client (collapsible) |
| **/notifications page** | All users |

---

## 7. Polling

Unread count refreshes every **15 seconds**. If you create an order as Client:
- Admin may need to wait up to 15 seconds, OR
- Click the bell to trigger an immediate load

---

## 8. Quick Test Flow

1. **Restart backend**
2. Login as **Client** → Create a new order
3. Logout (or use incognito/second browser)
4. Login as **Admin** or **SuperAdmin**
5. Wait 15 seconds OR click the bell icon
6. You should see "New Order Submitted" in the dropdown and/or dashboard panel

---

## 9. Common Issues

| Issue | Fix |
|-------|-----|
| 401 on `/api/notifications` | Log out and log back in to refresh token |
| Empty dropdown | Check Network tab for API response; verify backend is running |
| No "New Order" notification | Restart backend; verify Admin user has correct role |
| Badge shows 0 but notifications exist | Wait for polling (15s) or click bell to refresh |
