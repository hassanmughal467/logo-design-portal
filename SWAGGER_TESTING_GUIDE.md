# Swagger Testing Guide for New Endpoints

## Prerequisites
1. Start the backend API: `dotnet run` in `Backend/src/LogoDesignPortal.API`
2. Navigate to: `https://localhost:5001/swagger` or `http://localhost:5000/swagger`
3. Authenticate with a SuperAdmin or Admin token

## New Endpoints to Test

### 1. Audit Logs (SuperAdmin/Admin Only)

#### Get All Audit Logs
- **Endpoint**: `GET /api/AuditLogs`
- **Query Parameters**:
  - `entityType` (optional): Filter by entity type (Order, Invoice, User, File, Project)
  - `entityId` (optional): Filter by specific entity ID
  - `pageNumber` (default: 1)
  - `pageSize` (default: 50)
- **Expected Response**: List of audit log entries with entity type, action, performer, timestamp

#### Get Entity Audit Logs
- **Endpoint**: `GET /api/AuditLogs/{entityType}/{entityId}`
- **Example**: `GET /api/AuditLogs/Order/{orderId}`
- **Expected Response**: List of audit logs for the specific entity

### 2. Invoice Statistics

#### Get Invoice Statistics
- **Endpoint**: `GET /api/Invoices/statistics`
- **Authorization**: Any authenticated user (filtered by role)
- **Expected Response**:
```json
{
  "totalInvoices": 10,
  "paidInvoices": 5,
  "dueInvoices": 3,
  "overdueInvoices": 2,
  "totalAmount": 5000.00,
  "paidAmount": 2500.00,
  "dueAmount": 1500.00,
  "overdueAmount": 1000.00
}
```

### 3. Client Detail Page

#### Get Client Detail
- **Endpoint**: `GET /api/Users/clients/{clientId}/detail`
- **Authorization**: SuperAdmin, Admin
- **Expected Response**: Comprehensive client information including:
  - User information
  - Client profile
  - Order history
  - Invoices
  - Files
  - Activity timeline (from audit logs)

### 4. Designer Detail Page

#### Get Designer Detail
- **Endpoint**: `GET /api/Users/designers/{designerId}/detail`
- **Authorization**: SuperAdmin, Admin
- **Expected Response**: Comprehensive designer information including:
  - User information
  - Designer profile
  - Assigned orders
  - Completed orders count
  - Average delivery time (days)
  - Availability status
  - Notes

### 5. Settings - Invoice Configuration

#### Update Invoice Settings (SuperAdmin Only)
- **Endpoint**: `POST /api/Settings/invoice`
- **Body**:
```json
{
  "invoicePrefix": "INV",
  "defaultDueDays": 30,
  "defaultTaxPercentage": 10.0,
  "currency": "USD"
}
```

### 6. User Deactivation (Updated)

#### Deactivate User (Soft Deactivation)
- **Endpoint**: `DELETE /api/Users/{id}`
- **Authorization**: SuperAdmin
- **Expected Response**: `{ "message": "User deactivated successfully." }`
- **Note**: This now performs soft deactivation instead of deletion

## Testing Checklist

- [ ] Test audit logs retrieval with different filters
- [ ] Test invoice statistics endpoint
- [ ] Test client detail endpoint with valid client ID
- [ ] Test designer detail endpoint with valid designer ID
- [ ] Test invoice settings update (SuperAdmin only)
- [ ] Test user deactivation (verify IsActive=false, DeactivatedAt set)
- [ ] Verify rate limiting on auth endpoints (try 6+ requests quickly)
- [ ] Test account lockout (5 failed login attempts)

## Security Testing

### Rate Limiting
1. Try making 6+ requests to `/api/Auth/login` within 1 minute
2. Should receive `429 Too Many Requests` after 5 attempts

### Account Lockout
1. Attempt login with wrong password 5 times
2. 6th attempt should show account locked message
3. Wait 30 minutes or manually reset `LockoutEnd` in database

### Input Sanitization
1. Try submitting forms with `<script>alert('xss')</script>` in text fields
2. Script tags should be removed from input
