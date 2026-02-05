# Logo Design Portal - Backend (Phase 1)

## Overview
This is the backend foundation for the Logo Design Business Web Portal, built with ASP.NET Core Web API (.NET 8) using Clean Architecture principles.

## Architecture

The solution follows Clean Architecture with the following structure:

```
LogoDesignPortal.sln
│
├── src
│   ├── LogoDesignPortal.Domain
│   │   ├── Entities
│   │   │   ├── BaseEntity.cs
│   │   │   ├── User.cs
│   │   │   └── Role.cs
│   │   └── Enums
│   │       └── SystemRoles.cs
│
│   ├── LogoDesignPortal.Application
│   │   ├── Interfaces
│   │   │   ├── Authentication
│   │   │   │   └── IJwtTokenService.cs
│   │   │   ├── Persistence
│   │   │   │   └── IApplicationDbContext.cs
│   │   │   ├── IAuthService.cs
│   │   │   └── IUserService.cs
│   │   ├── DTOs
│   │   │   ├── Auth
│   │   │   │   ├── LoginRequestDto.cs
│   │   │   │   ├── AuthResponseDto.cs
│   │   │   │   └── UserDto.cs
│   │   │   └── Users
│   │   │       ├── CreateUserRequestDto.cs
│   │   │       └── UserResponseDto.cs
│   │   └── Services
│   │       ├── AuthService.cs
│   │       └── UserService.cs
│
│   ├── LogoDesignPortal.Infrastructure
│   │   ├── Authentication
│   │   │   └── JwtTokenService.cs
│   │   ├── Persistence
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Configurations
│   │   │       ├── UserConfiguration.cs
│   │   │       └── RoleConfiguration.cs
│   │   └── DependencyInjection.cs
│
│   ├── LogoDesignPortal.API
│   │   ├── Controllers
│   │   │   ├── AuthController.cs
│   │   │   └── UsersController.cs
│   │   ├── Middleware
│   │   │   └── ExceptionMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
```

## Features (Phase 1)

✅ Clean Architecture project structure  
✅ JWT Authentication  
✅ Role-based Authorization  
✅ Base entities (User, Role) with BaseEntity  
✅ EF Core + SQL Server  
✅ DbContext with separate configurations  
✅ Swagger (JWT enabled)  
✅ Global exception handling  
✅ SuperAdmin can create users with any role  

## Roles

- **SuperAdmin**: Full system access, can create users with any role
- **Admin**: Administrative access with restricted client data access
- **Designer**: Designer access without client identity information
- **Client**: Client access to their own data

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full SQL Server instance)
- Visual Studio 2022 or VS Code / Rider

## Setup Instructions

1. **Restore NuGet packages:**
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

3. **Update JWT settings** in `src/LogoDesignPortal.API/appsettings.json`:
   ```json
   "Jwt": {
     "Key": "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong!",
     "Issuer": "LogoDesignPortal",
     "Audience": "LogoDesignPortalUsers"
   }
   ```
   ⚠️ **Important**: Change the JWT Key to a secure random string in production!

4. **Run the application:**
   ```bash
   cd src/LogoDesignPortal.API
   dotnet run
   ```

5. **Access Swagger UI:**
   - Navigate to `https://localhost:5001/swagger` or `http://localhost:5000/swagger`

## Default SuperAdmin Credentials

- **Email**: `superadmin@logodesign.com`
- **Password**: `SuperAdmin@123`

⚠️ **Important**: Change these credentials immediately in production!

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login and get JWT token

### Users (Requires Authentication)
- `POST /api/users` - Create a new user (SuperAdmin only)
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users` - Get all users (SuperAdmin, Admin)

## Testing with Swagger

1. Open Swagger UI at `/swagger`
2. Use the `/api/auth/login` endpoint to authenticate:
   ```json
   {
     "email": "superadmin@logodesign.com",
     "password": "SuperAdmin@123"
   }
   ```
3. Copy the `token` from the response
4. Click the "Authorize" button in Swagger
5. Enter: `Bearer <your-token>`
6. Now you can test protected endpoints

## Database

The database is automatically created on first run using `EnsureCreated()`. The following are seeded:

- **Roles**: SuperAdmin, Admin, Designer, Client
- **Default User**: SuperAdmin user with email `superadmin@logodesign.com`

## Architecture Highlights

- **BaseEntity**: All entities inherit from BaseEntity with common properties (Id, CreatedAt, UpdatedAt)
- **IApplicationDbContext**: Interface for database context abstraction
- **IJwtTokenService**: Separate JWT token generation service
- **Entity Configurations**: Separate configuration files for each entity (UserConfiguration, RoleConfiguration)
- **ExceptionMiddleware**: Centralized exception handling

## Next Steps (Future Phases)

- Orders management
- File uploads
- Invoices
- Frontend integration
- Advanced features

## Notes

- This is Phase 1 - Backend Foundation only
- No frontend implementation
- No advanced features (Orders, Files, Invoices)
- Code is production-ready and extensible
