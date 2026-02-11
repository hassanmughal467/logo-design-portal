# User Profile Management Guide

## Overview

This guide explains how user profile information is managed in the system, including:
- How SuperAdmin can add complete information when creating users
- How users can update their complete profile information
- How admins can view full user details

---

## 1. SuperAdmin Create User - Adding Complete Information

### Backend Changes

**CreateUserRequestDto** now includes optional profile fields:

#### For Client Role:
- `CompanyName` (optional, but recommended)
- `PhoneNumber` (optional)
- `Address` (optional)
- `City` (optional)
- `Country` (optional)
- `PostalCode` (optional)

#### For Designer Role:
- `Specialization` (optional)
- `Bio` (optional)
- `HourlyRate` (optional)
- `IsAvailable` (optional, defaults to true)

### How It Works

When SuperAdmin creates a user via the API:

1. **Basic User Information** is always required:
   - Email
   - FirstName
   - LastName
   - Password
   - RoleId

2. **Profile Creation** happens automatically:
   - If `RoleId` is **Client** and `CompanyName` is provided → Creates `ClientProfile`
   - If `RoleId` is **Designer** → Creates `DesignerProfile` (even if fields are empty)

3. **Response** includes full profile information in `UserResponseDto`

### Example API Request (Create Client User)

```json
POST /api/users
{
  "email": "client@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePass123",
  "roleId": "<client-role-guid>",
  "companyName": "Acme Corporation",
  "phoneNumber": "+1234567890",
  "address": "123 Main St",
  "city": "New York",
  "country": "United States",
  "postalCode": "10001"
}
```

### Example API Request (Create Designer User)

```json
POST /api/users
{
  "email": "designer@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "SecurePass123",
  "roleId": "<designer-role-guid>",
  "specialization": "Logo Design",
  "bio": "Experienced logo designer with 10+ years",
  "hourlyRate": 50.00,
  "isAvailable": true
}
```

---

## 2. User Profile Updates - Complete Information

### Backend Changes

**UpdateUserRequestDto** now includes profile fields that users can update:

#### For Client Role:
- `CompanyName`
- `PhoneNumber`
- `Address`
- `City`
- `Country`
- `PostalCode`

#### For Designer Role:
- `Specialization`
- `Bio`
- `HourlyRate`
- `IsAvailable`

### How It Works

When a user updates their profile via the API:

1. **User Basic Information** is updated:
   - Email
   - FirstName
   - LastName
   - Role (if admin is updating)
   - IsActive (if admin is updating)

2. **Profile Update/Creation**:
   - If profile exists → Updates existing profile
   - If profile doesn't exist → Creates new profile
   - Profile fields are updated based on the user's role

3. **Response** includes updated profile information

### Example API Request (Update Client Profile)

```json
PUT /api/users/{userId}
{
  "email": "client@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Client",
  "isActive": true,
  "companyName": "Acme Corporation Updated",
  "phoneNumber": "+1234567890",
  "address": "456 New St",
  "city": "Los Angeles",
  "country": "United States",
  "postalCode": "90001"
}
```

---

## 3. Admin View - Full User Information

### Backend Changes

**UserResponseDto** now includes complete profile information:

```csharp
public class UserResponseDto
{
    // Basic user info
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Profile information (null if profile doesn't exist)
    public ClientProfileDto? ClientProfile { get; set; }
    public DesignerProfileDto? DesignerProfile { get; set; }
}
```

### How It Works

When an admin views a user via `GET /api/users/{id}`:

1. **User Information** is loaded with:
   - Basic user data
   - Role information
   - ClientProfile (if user is a Client)
   - DesignerProfile (if user is a Designer)

2. **Response** includes:
   - All basic user fields
   - Complete profile information (if exists)
   - `ClientProfile` or `DesignerProfile` will be `null` if profile doesn't exist

### Example API Response (Client User)

```json
GET /api/users/{userId}
{
  "id": "guid-here",
  "email": "client@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roleName": "Client",
  "isActive": true,
  "createdAt": "2024-01-15T10:00:00Z",
  "clientProfile": {
    "id": "profile-guid",
    "userId": "user-guid",
    "companyName": "Acme Corporation",
    "phoneNumber": "+1234567890",
    "address": "123 Main St",
    "city": "New York",
    "country": "United States",
    "postalCode": "10001"
  },
  "designerProfile": null
}
```

### Example API Response (Designer User)

```json
GET /api/users/{userId}
{
  "id": "guid-here",
  "email": "designer@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "roleName": "Designer",
  "isActive": true,
  "createdAt": "2024-01-15T10:00:00Z",
  "clientProfile": null,
  "designerProfile": {
    "id": "profile-guid",
    "userId": "user-guid",
    "specialization": "Logo Design",
    "bio": "Experienced logo designer",
    "hourlyRate": 50.00,
    "isAvailable": true
  }
}
```

---

## 4. Registration Form Fields - Complete Implementation

### All Registration Form Fields

The registration form (`RegisterRequestDto`) includes all these fields, and **ALL are now saved**:

#### User Table Fields:
- ✅ Email (required)
- ✅ FirstName (required)
- ✅ LastName (required)
- ✅ Password (required, hashed)
- ✅ **SecondaryEmail** (optional) - **NOW SAVED**
- ✅ **InvoiceEmail** (required) - **NOW SAVED**

#### ClientProfile Table Fields:
- ✅ CompanyName (required)
- ✅ **ContactName** (required) - **NOW SAVED**
- ✅ Phone (saved as PhoneNumber)
- ✅ **Cell** (optional) - **NOW SAVED**
- ✅ **Fax** (optional) - **NOW SAVED**
- ✅ Address (optional)
- ✅ City (optional)
- ✅ **State** (optional) - **NOW SAVED**
- ✅ Country (optional)
- ✅ ZipCode (saved as PostalCode)
- ✅ **Website** (optional) - **NOW SAVED**
- ✅ **Reference** (optional) - **NOW SAVED**

### Fields NOT Saved

- **UserName** - Not used in User entity (only used for registration validation)
- **AgreeToTerms** - Only used for validation, not stored

### Database Migration Required

⚠️ **IMPORTANT**: You must run a database migration to add the new fields. See `DATABASE_MIGRATION_REQUIRED.md` for instructions.

---

## 5. Frontend Integration

### For SuperAdmin Create User Form

The frontend form should include:
- Basic user fields (Email, FirstName, LastName, Password, RoleId)
- **Conditional profile fields** based on selected role:
  - If Client role selected → Show Client profile fields
  - If Designer role selected → Show Designer profile fields

### For User Profile Update Form

The frontend form should:
- Load existing user data via `GET /api/users/{id}`
- Display all profile fields based on user's role
- Allow users to update their complete information
- Submit updates via `PUT /api/users/{id}`

### For Admin User Detail View

The frontend should:
- Display basic user information
- **Display full profile information**:
  - If `clientProfile` exists → Show all client profile fields
  - If `designerProfile` exists → Show all designer profile fields
- Show "No profile information" if both are null

---

## 6. Summary

### What's Now Possible

✅ **SuperAdmin can add complete profile information** when creating users
✅ **Users can update their complete profile information** via the update endpoint
✅ **Admins can view full user details** including all profile information
✅ **Profile creation is automatic** based on user role
✅ **Profile updates work seamlessly** - creates if missing, updates if exists

### Key Points

1. **Profile fields are optional** when creating users, but recommended
2. **Profile is created automatically** when profile fields are provided
3. **Profile can be updated** along with user information
4. **Full profile information is returned** in all user responses
5. **Profile type depends on user role** (Client vs Designer)

---

## 7. Next Steps (Optional Enhancements)

If you want to save additional registration fields:

1. **Add fields to entities**:
   - Add `SecondaryEmail`, `InvoiceEmail` to `User` entity
   - Add `Cell`, `Fax`, `State`, `Website` to `ClientProfile` entity

2. **Create migration**:
   ```bash
   dotnet ef migrations add AddAdditionalProfileFields
   dotnet ef database update
   ```

3. **Update DTOs**:
   - Add fields to `RegisterRequestDto`
   - Add fields to `CreateUserRequestDto`
   - Add fields to `UpdateUserRequestDto`
   - Add fields to `ClientProfileDto`

4. **Update services**:
   - Update `AuthService.RegisterAsync` to save new fields
   - Update `UserService.CreateUserAsync` to save new fields
   - Update `UserService.UpdateUserAsync` to update new fields

---

**Happy Coding!** 🎉
