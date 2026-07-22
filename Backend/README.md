# Logo Design Portal - Production-Ready Backend

## 🎯 Overview
A production-ready ASP.NET Core Web API backend for a Logo Design Business Portal, built with Clean Architecture principles, featuring JWT authentication with refresh tokens, role-based authorization, secure file handling, and comprehensive order management.

## 🏗️ Architecture

### Clean Architecture Layers

```
LogoDesignPortal.sln
│
├── src
│   ├── LogoDesignPortal.Domain          # Core entities, enums
│   ├── LogoDesignPortal.Application     # Business logic, DTOs, services
│   ├── LogoDesignPortal.Infrastructure  # Data access, external services
│   └── LogoDesignPortal.API             # Controllers, middleware, configuration
```

## ✨ Features

### Authentication & Authorization
- ✅ JWT Authentication with Refresh Tokens
- ✅ Role-based Authorization (SuperAdmin, Admin, Designer, Client)
- ✅ Client Registration & Login
- ✅ Secure token management

### Order Management
- ✅ Logo order creation
- ✅ Assign orders to designers
- ✅ Order status tracking (Pending, InProgress, Completed, Cancelled)
- ✅ Order status history
- ✅ Role-based order access

### File Management
- ✅ Secure file upload (max 10MB)
- ✅ File type validation
- ✅ Secure file download (not publicly accessible)
- ✅ File deletion with authorization

### Data Security
- ✅ Client info masking for Admin (no personal data)
- ✅ Designers never see client identity
- ✅ Soft delete implementation
- ✅ Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)

### Technical Features
- ✅ AutoMapper for DTO mapping
- ✅ Repository pattern
- ✅ Global exception handling
- ✅ Swagger with JWT support
- ✅ EF Core Code-First with Migrations
- ✅ Soft delete support

## 📊 Entities

- **User** - System users with roles
- **Role** - User roles (SuperAdmin, Admin, Designer, Client)
- **ClientProfile** - Client-specific information
- **DesignerProfile** - Designer-specific information
- **LogoOrder** - Logo design orders
- **LogoFile** - Uploaded logo files
- **OrderStatusHistory** - Order status change history
- **Invoice** - Order invoices

## 🔐 User Roles & Permissions

### SuperAdmin
- Full system access
- Can create users with any role
- Can view all orders and client information
- Can assign orders to designers

### Admin
- Administrative access
- **Cannot see client personal data** (email, phone masked)
- Can view all orders
- Can assign orders to designers

### Designer
- **Cannot see client identity** (client info completely hidden)
- Can view assigned orders only
- Can upload/download files for assigned orders
- Can update order status

### Client
- Can create orders
- Can view own orders only
- Can upload/download files for own orders
- Can update own order status

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 / VS Code / Rider

### Setup

1. **Clone and restore packages:**
   ```bash
   cd Backend
   dotnet restore
   ```

2. **Update connection string** in `src/LogoDesignPortal.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LogoDesignPortalDb;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```

3. **Set a JWT signing key** — generate your own random 32+ character secret (do not reuse an example value) in `appsettings.Development.json`, or via the `Jwt__Key` environment variable:
   ```json
   "Jwt": {
     "Key": "<your own random secret, 32+ characters>",
     "Issuer": "LogoDesignPortal",
     "Audience": "LogoDesignPortalUsers"
   }
   ```
   Staging/Production must set `Jwt__Key` and `ConnectionStrings__DefaultConnection` as real environment variables — the checked-in `appsettings.*.json` values for those environments are intentionally non-functional placeholders, and the app fails fast at startup if they're still in effect.

4. **Run the application:**
   ```bash
   cd src/LogoDesignPortal.API
   dotnet run
   ```

5. **Access Swagger:**
   - Navigate to `https://localhost:5001/swagger`

### Database Migrations

```bash
cd src/LogoDesignPortal.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../LogoDesignPortal.API
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

## 🔑 Default Credentials

- **Email**: `superadmin@logodesign.com`
- **Password**: `SuperAdmin@123`

⚠️ **Change these immediately in production!**

## 📡 API Endpoints

### Authentication
- `POST /api/auth/login` - Login (returns JWT + Refresh Token)
- `POST /api/auth/register` - Client registration
- `POST /api/auth/refresh-token` - Refresh access token

### Orders
- `POST /api/orders` - Create order (Client only)
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/my-orders` - Get client's orders
- `GET /api/orders/assigned-orders` - Get designer's assigned orders
- `GET /api/orders` - Get all orders (SuperAdmin, Admin)
- `POST /api/orders/{id}/assign` - Assign order to designer (SuperAdmin, Admin)
- `PUT /api/orders/{id}/status` - Update order status

### Files
- `POST /api/files/upload/{orderId}` - Upload file
- `GET /api/files/{id}/download` - Download file (secure)
- `GET /api/files/order/{orderId}` - Get order files
- `DELETE /api/files/{id}` - Delete file

### Users
- `POST /api/users` - Create user (SuperAdmin only)
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users` - Get all users (SuperAdmin, Admin)

## 🧪 Testing with Swagger

1. **Register a client:**
   ```json
   POST /api/auth/register
   {
     "email": "client@example.com",
     "password": "Password123",
     "firstName": "John",
     "lastName": "Doe",
     "companyName": "Acme Corp",
     "phoneNumber": "+1234567890"
   }
   ```

2. **Login:**
   ```json
   POST /api/auth/login
   {
     "email": "client@example.com",
     "password": "Password123"
   }
   ```

3. **Copy the token** from response

4. **Click "Authorize"** in Swagger and enter: `Bearer <your-token>`

5. **Create an order:**
   ```json
   POST /api/orders
   {
     "title": "New Logo Design",
     "description": "Need a modern logo for my company",
     "price": 500.00,
     "deadline": "2024-12-31T00:00:00Z",
     "colorPreferences": "Blue and white",
     "stylePreferences": "Modern, minimalist"
   }
   ```

## 🔒 Security Features

### File Security
- Files stored outside web root
- Access controlled by authorization
- File type validation
- File size limits (10MB)
- Secure download endpoints

### Data Privacy
- **Admin**: Client email/phone masked
- **Designer**: Client identity completely hidden
- Soft delete (data not permanently removed)
- Audit trail (CreatedBy, UpdatedBy)

### Authentication
- JWT tokens with 1-hour expiry
- Refresh tokens with 7-day expiry
- Secure password hashing (BCrypt)
- Token validation

## 📁 File Storage

Files are stored in the `Files` directory (configurable in `appsettings.json`):
```json
"FileStorage": {
  "Path": "Files"
}
```

⚠️ **Ensure this directory is not publicly accessible!**

## 🗄️ Database

- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Migrations**: Code-First approach
- **Soft Delete**: Implemented via `IsDeleted` flag
- **Audit Fields**: Automatic tracking

## 🛠️ Technologies

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core 8.0**
- **SQL Server**
- **JWT Authentication**
- **AutoMapper**
- **BCrypt.Net**
- **Swagger/OpenAPI**

## 📝 Code Structure

### Domain Layer
- Entities (User, Role, LogoOrder, etc.)
- Enums (OrderStatus, SystemRoles)
- BaseEntity with audit fields

### Application Layer
- DTOs (Data Transfer Objects)
- Interfaces (IAuthService, IOrderService, etc.)
- Services (Business logic)
- AutoMapper profiles

### Infrastructure Layer
- DbContext and configurations
- Repository implementations
- JWT token service
- File storage service

### API Layer
- Controllers
- Middleware (Exception handling)
- Program.cs configuration

## 🚨 Production Checklist

- [ ] Change JWT key to secure random string
- [ ] Change default SuperAdmin credentials
- [ ] Update connection string
- [ ] Configure proper CORS policy
- [ ] Set up HTTPS only
- [ ] Configure file storage path (outside web root)
- [ ] Set up logging
- [ ] Configure environment variables
- [ ] Set up database backups
- [ ] Review and test all authorization rules

## 📚 Next Steps

- Add invoice generation
- Add email notifications
- Add payment integration
- Add order comments/chat
- Add file versioning
- Add advanced reporting
- Add unit tests
- Add integration tests

## 📄 License

This project is production-ready and follows industry best practices for security, scalability, and maintainability.
