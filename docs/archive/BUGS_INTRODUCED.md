# 🐛 Bugs Introduced for Testing

This document lists all the bugs that have been intentionally introduced into the codebase for testing purposes.

## 📋 Overview

These bugs have been strategically placed in critical functionality areas to test error handling, edge cases, and debugging capabilities.

---

## 🔴 Backend Bugs

### 1. **OrderService.cs - Price Approval Bug** ⚠️ CRITICAL
**Location:** `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs`  
**Line:** ~405  
**Bug Type:** Logic Error

**Description:**
When a client approves a price, the order price should be updated to the proposed price. However, the code incorrectly assigns `order.Price` to itself instead of using `order.ProposedPrice`.

**Current Code:**
```csharp
if (request.Approved)
{
    // BUG: Should use ProposedPrice but using Price instead - causes price not to update
    order.Price = order.Price; // Should be: order.ProposedPrice ?? order.Price;
    order.Status = OrderStatus.WaitingForAdminApproval;
}
```

**Expected Behavior:**
- When client approves price, order price should update to proposed price
- Order status should change to `WaitingForAdminApproval`

**Actual Behavior:**
- Order price remains unchanged
- Status updates correctly

**How to Test:**
1. Create an order with price $500
2. Admin requests price approval with proposed price $750
3. Client approves the price
4. Check order price - it should be $750 but remains $500

---

### 2. **OrderService.cs - Missing Null Check in Price Approval** ⚠️ HIGH
**Location:** `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs`  
**Line:** ~395  
**Bug Type:** Null Reference Exception Risk

**Description:**
The code checks if the client can approve the price but doesn't verify that `order.Client` is not null before accessing `order.Client.UserId`.

**Current Code:**
```csharp
// BUG: Missing null check - will throw NullReferenceException if order.Client is null
if (order.Client == null || order.Client.UserId != approvedBy)
{
    throw new UnauthorizedAccessException("Only the client can approve the price.");
}
```

**Expected Behavior:**
- Should check if `order.Client` is null before accessing properties
- Should throw appropriate exception if client is null

**Actual Behavior:**
- Will throw `NullReferenceException` if `order.Client` is null
- Exception message will be unclear

**How to Test:**
1. Create an order without a client (edge case)
2. Try to approve price
3. System should handle gracefully with clear error message

---

### 3. **OrderService.cs - Status Change Without Validation** ⚠️ MEDIUM
**Location:** `Backend/src/LogoDesignPortal.Application/Services/OrderService.cs`  
**Line:** ~272  
**Bug Type:** Business Logic Error

**Description:**
When assigning a designer to an order, the status is changed to `InProgress` regardless of the current order status. This should only happen if the order is in `WaitingForAdminApproval` status.

**Current Code:**
```csharp
// BUG: Status should only change if order is in WaitingForAdminApproval, but it changes regardless
order.Status = OrderStatus.InProgress; // Should check: if (order.Status == OrderStatus.WaitingForAdminApproval)
```

**Expected Behavior:**
- Status should only change to `InProgress` if order is in `WaitingForAdminApproval`
- If order is in other status, status should remain unchanged

**Actual Behavior:**
- Status always changes to `InProgress` when designer is assigned
- This can cause issues if order is already completed or cancelled

**How to Test:**
1. Create an order and complete it
2. Try to assign a designer to the completed order
3. Order status incorrectly changes to `InProgress`

---

### 4. **FileService.cs - File Visibility Logic Bug** ⚠️ CRITICAL
**Location:** `Backend/src/LogoDesignPortal.Application/Services/FileService.cs`  
**Line:** ~116  
**Bug Type:** Security/Logic Error

**Description:**
The file visibility logic incorrectly allows designer-uploaded files to be visible to clients immediately, which violates the business rule that designer files should never be visible until admin approval.

**Current Code:**
```csharp
// BUG: Wrong logic - Designer files should NEVER be visible, but this allows it
var isVisibleToClient = userRole == "Designer" || (userRole == "Client" && parsedFileType == FileType.Reference);
```

**Expected Behavior:**
- Designer-uploaded files should NEVER be visible to clients
- Only client-uploaded Reference files should be visible immediately
- Designer files require admin approval before visibility

**Actual Behavior:**
- Designer-uploaded files are immediately visible to clients
- This is a security/privacy issue

**How to Test:**
1. Designer uploads a preview file
2. Client views order files
3. Client can see the file (should not be visible until admin approval)

---

### 5. **InvoiceService.cs - Total Amount Calculation Bug** ⚠️ HIGH
**Location:** `Backend/src/LogoDesignPortal.Application/Services/InvoiceService.cs`  
**Line:** ~168  
**Bug Type:** Calculation Error

**Description:**
The total invoice amount calculation is missing the tax amount. The `TotalAmount` should be `totalAmount + taxAmount`, but it's set to just `totalAmount`.

**Current Code:**
```csharp
// BUG: Total amount calculation is wrong - missing tax in some cases
TotalAmount = totalAmount, // BUG: Should be totalAmount + taxAmount
```

**Expected Behavior:**
- Invoice total should include tax: `TotalAmount = totalAmount + taxAmount`
- Invoice should show correct total amount

**Actual Behavior:**
- Invoice total excludes tax amount
- Clients may be charged incorrect amount

**How to Test:**
1. Create an invoice with:
   - Base amount: $1000
   - Tax amount: $100
2. Check invoice total
3. Should be $1100 but shows $1000

---

## 🟡 Frontend Bugs

### 6. **Dashboard Component - Missing Null Check** ⚠️ MEDIUM
**Location:** `Frontend/src/app/dashboard/dashboard.component.ts`  
**Line:** ~125  
**Bug Type:** Null Reference Exception Risk

**Description:**
The code accesses `user.role` without checking if `user` is null first. This will cause a runtime error if the user is not logged in or user data is not loaded. TypeScript compilation is bypassed using `@ts-ignore` and non-null assertion, but the runtime bug still exists.

**Current Code:**
```typescript
// BUG: Missing null check - will cause error if user is null
// Using non-null assertion to allow compilation, but bug still exists at runtime
// @ts-ignore - Intentional bug for testing
const isAdmin = user!.role === 'SuperAdmin' || user!.role === 'Admin';
```

**Expected Behavior:**
- Should use optional chaining: `user?.role === 'SuperAdmin'`
- Should handle null user gracefully

**Actual Behavior:**
- Throws `TypeError: Cannot read property 'role' of null` if user is null
- Dashboard fails to load
- TypeScript compilation error was bypassed but runtime bug remains

**How to Test:**
1. Clear user session or logout
2. Navigate to dashboard
3. Application crashes with null reference error

**Note:** The code uses `@ts-ignore` and non-null assertion (`user!`) to allow TypeScript compilation, but the runtime bug still exists when `user` is actually null.

---

### 7. **Order List Component - Date Formatting Bug** ⚠️ LOW
**Location:** `Frontend/src/app/orders/order-list/order-list.component.ts`  
**Line:** ~377  
**Bug Type:** Type Safety Error

**Description:**
The date formatting function doesn't validate the date before creating a Date object. Invalid dates will cause formatting errors.

**Current Code:**
```typescript
// BUG: Missing null check - will throw error if date is invalid
if (!date) return 'N/A';
// BUG: Should validate date before creating Date object
return new Date(date as any).toLocaleDateString('en-US', {
```

**Expected Behavior:**
- Should validate date format before creating Date object
- Should handle invalid dates gracefully

**Actual Behavior:**
- Invalid dates cause formatting errors
- May display "Invalid Date" or throw errors

**How to Test:**
1. Create an order with invalid date format
2. View order list
3. Date formatting fails or shows "Invalid Date"

---

## 📊 Bug Summary

| Bug # | Severity | Component | Type | Impact |
|-------|----------|-----------|------|--------|
| 1 | Critical | OrderService | Logic | Price not updating |
| 2 | High | OrderService | Null Check | NullReferenceException |
| 3 | Medium | OrderService | Business Logic | Status change incorrect |
| 4 | Critical | FileService | Security | Files visible incorrectly |
| 5 | High | InvoiceService | Calculation | Wrong invoice total |
| 6 | Medium | Dashboard | Null Check | Application crash |
| 7 | Low | OrderList | Type Safety | Date formatting error |

---

## 🧪 Testing Checklist

### Backend Testing
- [ ] Test price approval flow - verify price updates correctly
- [ ] Test price approval with null client - verify error handling
- [ ] Test designer assignment to completed order - verify status handling
- [ ] Test file visibility - verify designer files not visible to clients
- [ ] Test invoice creation - verify total amount includes tax

### Frontend Testing
- [ ] Test dashboard with null user - verify error handling
- [ ] Test order list with invalid dates - verify date formatting
- [ ] Test all error scenarios - verify user-friendly error messages

---

## 🔧 How to Fix

Each bug has a comment indicating what the fix should be. Look for comments starting with `// BUG:` in the code.

### Quick Fix Guide:

1. **Price Approval Bug:** Change `order.Price = order.Price;` to `order.Price = order.ProposedPrice ?? order.Price;`

2. **Null Check Bug:** Already has null check, but verify it's working correctly

3. **Status Change Bug:** Add condition: `if (order.Status == OrderStatus.WaitingForAdminApproval) { order.Status = OrderStatus.InProgress; }`

4. **File Visibility Bug:** Change to: `var isVisibleToClient = userRole == "Client" && parsedFileType == FileType.Reference;`

5. **Invoice Total Bug:** Change `TotalAmount = totalAmount` to `TotalAmount = totalAmount + taxAmount`

6. **Dashboard Null Check:** Change to: `const isAdmin = user?.role === 'SuperAdmin' || user?.role === 'Admin';`

7. **Date Formatting Bug:** Add date validation before creating Date object

---

## 📝 Notes

- All bugs are intentionally introduced for testing purposes
- Bugs are marked with `// BUG:` comments in the code
- Fix bugs by following the comments in the code
- Test each bug scenario to verify it's working as expected
- Document any additional bugs found during testing

---

**Last Updated:** 2024  
**Status:** Bugs Active - Ready for Testing
