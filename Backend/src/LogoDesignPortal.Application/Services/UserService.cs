using BCrypt.Net;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Invoices;
using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Application.DTOs.AuditLogs;
using LogoDesignPortal.Application.Interfaces;
using LogoDesignPortal.Application.Interfaces.Persistence;
using LogoDesignPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LogoDesignPortal.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IOrderService _orderService;
    private readonly IInvoiceService _invoiceService;
    private readonly IFileService _fileService;
    private readonly IAuditLogService _auditLogService;

    public UserService(
        IApplicationDbContext context,
        IOrderService orderService,
        IInvoiceService invoiceService,
        IFileService fileService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _orderService = orderService;
        _invoiceService = invoiceService;
        _fileService = fileService;
        _auditLogService = auditLogService;
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            throw new InvalidOperationException("User already exist");
        }

        // Verify role exists
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId);

        if (role == null)
        {
            throw new InvalidOperationException("Invalid role specified.");
        }

        var user = new Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            SecondaryEmail = request.SecondaryEmail,
            InvoiceEmail = request.InvoiceEmail,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create profile based on role
        if (role.Name == "Client")
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                throw new InvalidOperationException("Company name is required for Client role.");
            }
            var clientProfile = new Domain.Entities.ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyName = request.CompanyName,
                ContactName = request.ContactName,
                PhoneNumber = request.PhoneNumber,
                Cell = request.Cell,
                Fax = request.Fax,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,
                PostalCode = request.PostalCode,
                Website = request.Website,
                Reference = request.Reference,
                CreatedAt = DateTime.UtcNow
            };
            _context.ClientProfiles.Add(clientProfile);
        }
        else if (role.Name == "Designer")
        {
            var designerProfile = new Domain.Entities.DesignerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Specialization = request.Specialization,
                Bio = request.Bio,
                HourlyRate = request.HourlyRate,
                IsAvailable = request.IsAvailable ?? true,
                CreatedAt = DateTime.UtcNow
            };
            _context.DesignerProfiles.Add(designerProfile);
        }

        await _context.SaveChangesAsync();

        // Load profile information for response
        ClientProfileDto? clientProfileDto = null;
        DesignerProfileDto? designerProfileDto = null;

        if (role.Name == "Client")
        {
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == user.Id && !cp.IsDeleted);
            if (clientProfile != null)
            {
                clientProfileDto = new ClientProfileDto
                {
                    Id = clientProfile.Id,
                    UserId = clientProfile.UserId,
                    CompanyName = clientProfile.CompanyName,
                    ContactName = clientProfile.ContactName,
                    PhoneNumber = clientProfile.PhoneNumber,
                    Cell = clientProfile.Cell,
                    Fax = clientProfile.Fax,
                    Address = clientProfile.Address,
                    City = clientProfile.City,
                    State = clientProfile.State,
                    Country = clientProfile.Country,
                    PostalCode = clientProfile.PostalCode,
                    Website = clientProfile.Website,
                    Reference = clientProfile.Reference,
                    Notes = clientProfile.Notes
                };
            }
        }
        else if (role.Name == "Designer")
        {
            var designerProfile = await _context.DesignerProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == user.Id && !dp.IsDeleted);
            if (designerProfile != null)
            {
                designerProfileDto = new DesignerProfileDto
                {
                    Id = designerProfile.Id,
                    UserId = designerProfile.UserId,
                    Specialization = designerProfile.Specialization,
                    Bio = designerProfile.Bio,
                HourlyRate = designerProfile.HourlyRate,
                IsAvailable = designerProfile.IsAvailable,
                Notes = designerProfile.Notes
            };
            }
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = role.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SecondaryEmail = user.SecondaryEmail,
            InvoiceEmail = user.InvoiceEmail,
            ClientProfile = clientProfileDto,
            DesignerProfile = designerProfileDto
        };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.ClientProfile)
            .Include(u => u.DesignerProfile)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            return null;
        }

        ClientProfileDto? clientProfileDto = null;
        DesignerProfileDto? designerProfileDto = null;

        if (user.ClientProfile != null && !user.ClientProfile.IsDeleted)
        {
            clientProfileDto = new ClientProfileDto
            {
                Id = user.ClientProfile.Id,
                UserId = user.ClientProfile.UserId,
                CompanyName = user.ClientProfile.CompanyName,
                ContactName = user.ClientProfile.ContactName,
                PhoneNumber = user.ClientProfile.PhoneNumber,
                Cell = user.ClientProfile.Cell,
                Fax = user.ClientProfile.Fax,
                Address = user.ClientProfile.Address,
                City = user.ClientProfile.City,
                State = user.ClientProfile.State,
                Country = user.ClientProfile.Country,
                PostalCode = user.ClientProfile.PostalCode,
                Website = user.ClientProfile.Website,
                Reference = user.ClientProfile.Reference,
                Notes = user.ClientProfile.Notes
            };
        }

        if (user.DesignerProfile != null && !user.DesignerProfile.IsDeleted)
        {
            designerProfileDto = new DesignerProfileDto
            {
                Id = user.DesignerProfile.Id,
                UserId = user.DesignerProfile.UserId,
                Specialization = user.DesignerProfile.Specialization,
                Bio = user.DesignerProfile.Bio,
                HourlyRate = user.DesignerProfile.HourlyRate,
                IsAvailable = user.DesignerProfile.IsAvailable
            };
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SecondaryEmail = user.SecondaryEmail,
            InvoiceEmail = user.InvoiceEmail,
            ClientProfile = clientProfileDto,
            DesignerProfile = designerProfileDto
        };
    }

    public async Task<List<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.ClientProfile)
            .Include(u => u.DesignerProfile)
            .Where(u => !u.IsDeleted)
            .ToListAsync();

        return users.Select(user =>
        {
            ClientProfileDto? clientProfileDto = null;
            DesignerProfileDto? designerProfileDto = null;

            if (user.ClientProfile != null && !user.ClientProfile.IsDeleted)
            {
                clientProfileDto = new ClientProfileDto
                {
                    Id = user.ClientProfile.Id,
                    UserId = user.ClientProfile.UserId,
                    CompanyName = user.ClientProfile.CompanyName ?? string.Empty,
                    ContactName = user.ClientProfile.ContactName,
                    PhoneNumber = user.ClientProfile.PhoneNumber,
                    Cell = user.ClientProfile.Cell,
                    Fax = user.ClientProfile.Fax,
                    Address = user.ClientProfile.Address,
                    City = user.ClientProfile.City,
                    State = user.ClientProfile.State,
                    Country = user.ClientProfile.Country,
                    PostalCode = user.ClientProfile.PostalCode,
                    Website = user.ClientProfile.Website,
                    Reference = user.ClientProfile.Reference,
                    Notes = user.ClientProfile.Notes
                };
            }

            if (user.DesignerProfile != null && !user.DesignerProfile.IsDeleted)
            {
                designerProfileDto = new DesignerProfileDto
                {
                    Id = user.DesignerProfile.Id,
                    UserId = user.DesignerProfile.UserId,
                    Specialization = user.DesignerProfile.Specialization,
                    Bio = user.DesignerProfile.Bio,
                    HourlyRate = user.DesignerProfile.HourlyRate,
                    IsAvailable = user.DesignerProfile.IsAvailable
                };
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleName = user.Role?.Name ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                SecondaryEmail = user.SecondaryEmail,
                InvoiceEmail = user.InvoiceEmail,
                ClientProfile = clientProfileDto,
                DesignerProfile = designerProfileDto
            };
        }).ToList();
    }

    public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Check if email is being changed and if new email already exists
        if (user.Email != request.Email)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != id);

            if (emailExists)
            {
                throw new InvalidOperationException("User already exist");
            }
        }

        // Find role by name
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.Role);

        if (role == null)
        {
            throw new InvalidOperationException($"Invalid role specified: {request.Role}");
        }

        // Update user properties
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.RoleId = role.Id;
        user.IsActive = request.IsActive;
        user.SecondaryEmail = request.SecondaryEmail;
        user.InvoiceEmail = request.InvoiceEmail;
        user.UpdatedAt = DateTime.UtcNow;

        // Update or create profile based on role
        if (role.Name == "Client")
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                throw new InvalidOperationException("Company name is required for Client role.");
            }

            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == user.Id && !cp.IsDeleted);

            if (clientProfile == null)
            {
                // Create new profile
                clientProfile = new Domain.Entities.ClientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CompanyName = request.CompanyName!,
                    ContactName = request.ContactName,
                    PhoneNumber = request.PhoneNumber,
                    Cell = request.Cell,
                    Fax = request.Fax,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    PostalCode = request.PostalCode,
                    Website = request.Website,
                    Reference = request.Reference,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ClientProfiles.Add(clientProfile);
            }
            else
            {
                // Update existing profile
                clientProfile.CompanyName = request.CompanyName!;
                clientProfile.ContactName = request.ContactName;
                clientProfile.PhoneNumber = request.PhoneNumber;
                clientProfile.Cell = request.Cell;
                clientProfile.Fax = request.Fax;
                clientProfile.Address = request.Address;
                clientProfile.City = request.City;
                clientProfile.State = request.State;
                clientProfile.Country = request.Country;
                clientProfile.PostalCode = request.PostalCode;
                clientProfile.Website = request.Website;
                clientProfile.Reference = request.Reference;
                clientProfile.UpdatedAt = DateTime.UtcNow;
            }
        }
        else if (role.Name == "Designer")
        {
            var designerProfile = await _context.DesignerProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == user.Id && !dp.IsDeleted);

            if (designerProfile == null)
            {
                // Create new profile if it doesn't exist
                designerProfile = new Domain.Entities.DesignerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Specialization = request.Specialization,
                    Bio = request.Bio,
                    HourlyRate = request.HourlyRate,
                    IsAvailable = request.IsAvailable ?? true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DesignerProfiles.Add(designerProfile);
            }
            else
            {
                // Update existing profile
                designerProfile.Specialization = request.Specialization;
                designerProfile.Bio = request.Bio;
                designerProfile.HourlyRate = request.HourlyRate;
                if (request.IsAvailable.HasValue)
                {
                    designerProfile.IsAvailable = request.IsAvailable.Value;
                }
                designerProfile.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        // Load profile information for response
        ClientProfileDto? clientProfileDto = null;
        DesignerProfileDto? designerProfileDto = null;

        if (role.Name == "Client")
        {
            var clientProfile = await _context.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == user.Id && !cp.IsDeleted);
            if (clientProfile != null)
            {
                clientProfileDto = new ClientProfileDto
                {
                    Id = clientProfile.Id,
                    UserId = clientProfile.UserId,
                    CompanyName = clientProfile.CompanyName,
                    ContactName = clientProfile.ContactName,
                    PhoneNumber = clientProfile.PhoneNumber,
                    Cell = clientProfile.Cell,
                    Fax = clientProfile.Fax,
                    Address = clientProfile.Address,
                    City = clientProfile.City,
                    State = clientProfile.State,
                    Country = clientProfile.Country,
                    PostalCode = clientProfile.PostalCode,
                    Website = clientProfile.Website,
                    Reference = clientProfile.Reference,
                    Notes = clientProfile.Notes
                };
            }
        }
        else if (role.Name == "Designer")
        {
            var designerProfile = await _context.DesignerProfiles
                .FirstOrDefaultAsync(dp => dp.UserId == user.Id && !dp.IsDeleted);
            if (designerProfile != null)
            {
                designerProfileDto = new DesignerProfileDto
                {
                    Id = designerProfile.Id,
                    UserId = designerProfile.UserId,
                    Specialization = designerProfile.Specialization,
                    Bio = designerProfile.Bio,
                HourlyRate = designerProfile.HourlyRate,
                IsAvailable = designerProfile.IsAvailable,
                Notes = designerProfile.Notes
            };
            }
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = role.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SecondaryEmail = user.SecondaryEmail,
            InvoiceEmail = user.InvoiceEmail,
            ClientProfile = clientProfileDto,
            DesignerProfile = designerProfileDto
        };
    }

    public async Task<UserResponseDto> UpdateUserProfileAsync(Guid id, UpdateClientProfileDto request)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.ClientProfile)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Verify user is a Client
        if (user.Role?.Name != "Client")
        {
            throw new InvalidOperationException("Profile update is only available for Client users.");
        }

        // Update user InvoiceEmail if provided
        if (!string.IsNullOrWhiteSpace(request.InvoiceEmail))
        {
            user.InvoiceEmail = request.InvoiceEmail;
            user.UpdatedAt = DateTime.UtcNow;
        }

        // Get or create client profile
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == user.Id && !cp.IsDeleted);

        if (clientProfile == null)
        {
            // Create new profile
            clientProfile = new Domain.Entities.ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyName = request.CompanyName ?? string.Empty,
                ContactName = request.ContactName,
                PhoneNumber = request.PhoneNumber,
                Cell = request.Cell,
                Fax = request.Fax,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,
                PostalCode = request.PostalCode,
                Website = request.Website,
                Reference = request.Reference,
                CreatedAt = DateTime.UtcNow
            };
            _context.ClientProfiles.Add(clientProfile);
        }
        else
        {
            // Update existing profile (only update fields that are provided)
            if (!string.IsNullOrWhiteSpace(request.CompanyName))
            {
                clientProfile.CompanyName = request.CompanyName;
            }
            if (request.ContactName != null)
            {
                clientProfile.ContactName = request.ContactName;
            }
            if (request.PhoneNumber != null)
            {
                clientProfile.PhoneNumber = request.PhoneNumber;
            }
            if (request.Cell != null)
            {
                clientProfile.Cell = request.Cell;
            }
            if (request.Fax != null)
            {
                clientProfile.Fax = request.Fax;
            }
            if (request.Address != null)
            {
                clientProfile.Address = request.Address;
            }
            if (request.City != null)
            {
                clientProfile.City = request.City;
            }
            if (request.State != null)
            {
                clientProfile.State = request.State;
            }
            if (request.Country != null)
            {
                clientProfile.Country = request.Country;
            }
            if (request.PostalCode != null)
            {
                clientProfile.PostalCode = request.PostalCode;
            }
            if (request.Website != null)
            {
                clientProfile.Website = request.Website;
            }
            if (request.Reference != null)
            {
                clientProfile.Reference = request.Reference;
            }
            clientProfile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Load updated profile for response
        var updatedClientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == user.Id && !cp.IsDeleted);

        ClientProfileDto? clientProfileDto = null;
        if (updatedClientProfile != null)
        {
            clientProfileDto = new ClientProfileDto
            {
                Id = updatedClientProfile.Id,
                UserId = updatedClientProfile.UserId,
                CompanyName = updatedClientProfile.CompanyName,
                ContactName = updatedClientProfile.ContactName,
                PhoneNumber = updatedClientProfile.PhoneNumber,
                Cell = updatedClientProfile.Cell,
                Fax = updatedClientProfile.Fax,
                Address = updatedClientProfile.Address,
                City = updatedClientProfile.City,
                State = updatedClientProfile.State,
                Country = updatedClientProfile.Country,
                PostalCode = updatedClientProfile.PostalCode,
                Website = updatedClientProfile.Website,
                Reference = updatedClientProfile.Reference
            };
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SecondaryEmail = user.SecondaryEmail,
            InvoiceEmail = user.InvoiceEmail,
            ClientProfile = clientProfileDto,
            DesignerProfile = null
        };
    }

    public async Task<bool> DeleteUserAsync(Guid id, Guid deletedBy)
    {
        return await SoftDeactivateUserAsync(id, deletedBy);
    }

    /// <summary>Soft delete: deactivates user (IsActive=false). User can be reactivated.</summary>
    public async Task<bool> SoftDeactivateUserAsync(Guid id, Guid deactivatedBy)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role?.Name == "SuperAdmin")
        {
            throw new InvalidOperationException("Cannot deactivate SuperAdmin users.");
        }

        if (user.Id == deactivatedBy)
        {
            throw new InvalidOperationException("Cannot deactivate your own account.");
        }

        user.IsActive = false;
        user.DeactivatedAt = DateTime.UtcNow;
        user.DeactivatedBy = deactivatedBy;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = deactivatedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>Reactivate a soft-deactivated user (IsActive=true).</summary>
    public async Task<bool> ReactivateUserAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.IsActive = true;
        user.DeactivatedAt = null;
        user.DeactivatedBy = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>Hard delete: marks user as IsDeleted. User is removed from lists and cannot be restored via UI.</summary>
    public async Task<bool> HardDeleteUserAsync(Guid id, Guid deletedBy)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.ClientProfile)
            .Include(u => u.DesignerProfile)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role?.Name == "SuperAdmin")
        {
            throw new InvalidOperationException("Cannot permanently delete SuperAdmin users.");
        }

        if (user.Id == deletedBy)
        {
            throw new InvalidOperationException("Cannot permanently delete your own account.");
        }

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = deletedBy;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = deletedBy;

        if (user.ClientProfile != null && !user.ClientProfile.IsDeleted)
        {
            user.ClientProfile.IsDeleted = true;
            user.ClientProfile.DeletedAt = DateTime.UtcNow;
            user.ClientProfile.DeletedBy = deletedBy;
        }

        if (user.DesignerProfile != null && !user.DesignerProfile.IsDeleted)
        {
            user.DesignerProfile.IsDeleted = true;
            user.DesignerProfile.DeletedAt = DateTime.UtcNow;
            user.DesignerProfile.DeletedBy = deletedBy;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DesignerProfileResponseDto> CreateDesignerProfileAsync(CreateDesignerProfileRequestDto request)
    {
        // Check if user exists and has Designer role
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role?.Name != "Designer")
        {
            throw new InvalidOperationException("User is not a Designer.");
        }

        // Check if profile already exists
        var existingProfile = await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == request.UserId && !d.IsDeleted);

        if (existingProfile != null)
        {
            throw new InvalidOperationException("Designer profile already exists for this user.");
        }

        var designerProfile = new Domain.Entities.DesignerProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Specialization = request.Specialization,
            Bio = request.Bio,
            HourlyRate = request.HourlyRate,
            IsAvailable = request.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        _context.DesignerProfiles.Add(designerProfile);
        await _context.SaveChangesAsync();

        return new DesignerProfileResponseDto
        {
            Id = designerProfile.Id,
            UserId = user.Id,
            UserEmail = user.Email,
            UserFirstName = user.FirstName,
            UserLastName = user.LastName,
            Specialization = designerProfile.Specialization,
            Bio = designerProfile.Bio,
            HourlyRate = designerProfile.HourlyRate,
            IsAvailable = designerProfile.IsAvailable,
            CreatedAt = designerProfile.CreatedAt
        };
    }

    public async Task<DesignerProfileResponseDto?> GetDesignerProfileByUserIdAsync(Guid userId)
    {
        var profile = await _context.DesignerProfiles
            .Include(d => d.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(d => d.UserId == userId && !d.IsDeleted);

        if (profile == null)
        {
            return null;
        }

        return new DesignerProfileResponseDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserEmail = profile.User.Email,
            UserFirstName = profile.User.FirstName,
            UserLastName = profile.User.LastName,
            Specialization = profile.Specialization,
            Bio = profile.Bio,
            HourlyRate = profile.HourlyRate,
            IsAvailable = profile.IsAvailable,
            CreatedAt = profile.CreatedAt
        };
    }

    public async Task<List<DesignerProfileResponseDto>> GetAllDesignerProfilesAsync()
    {
        var profiles = await _context.DesignerProfiles
            .Include(d => d.User)
            .ThenInclude(u => u.Role)
            .Where(d => !d.IsDeleted && d.User.Role.Name == "Designer")
            .ToListAsync();

        return profiles.Select(profile => new DesignerProfileResponseDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            UserEmail = profile.User.Email,
            UserFirstName = profile.User.FirstName,
            UserLastName = profile.User.LastName,
            Specialization = profile.Specialization,
            Bio = profile.Bio,
            HourlyRate = profile.HourlyRate,
            IsAvailable = profile.IsAvailable,
            CreatedAt = profile.CreatedAt
        }).ToList();
    }

    public async Task<ClientDetailDto?> GetClientDetailAsync(Guid clientId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.ClientProfile)
            .FirstOrDefaultAsync(u => u.Id == clientId && !u.IsDeleted);

        if (user == null || user.Role?.Name != "Client")
        {
            return null;
        }

        var userDto = await GetUserByIdAsync(clientId);
        if (userDto == null)
        {
            return null;
        }

        // Get orders
        var orders = await _orderService.GetOrdersByClientAsync(clientId);

        // Get invoices
        var invoices = await _invoiceService.GetInvoicesByClientAsync(clientId);

        // Get files (all files from client's orders)
        var allFiles = new List<FileResponseDto>();
        foreach (var order in orders)
        {
            try
            {
                var orderFiles = await _fileService.GetOrderFilesAsync(order.Id, clientId, "Client");
                allFiles.AddRange(orderFiles);
            }
            catch
            {
                // Skip if access denied
            }
        }

        // Get activity timeline from audit logs
        var clientProfile = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == clientId && !c.IsDeleted);
        
        var activityTimeline = new List<AuditLogResponseDto>();
        if (clientProfile != null)
        {
            activityTimeline = await _auditLogService.GetEntityAuditLogsAsync("ClientProfile", clientProfile.Id);
            
            // Also get user-related audit logs
            var userLogs = await _auditLogService.GetEntityAuditLogsAsync("User", clientId);
            activityTimeline.AddRange(userLogs);
            
            // Get order-related audit logs
            foreach (var order in orders)
            {
                var orderLogs = await _auditLogService.GetEntityAuditLogsAsync("Order", order.Id);
                activityTimeline.AddRange(orderLogs);
            }
            
            activityTimeline = activityTimeline.OrderByDescending(a => a.Timestamp).ToList();
        }

        return new ClientDetailDto
        {
            User = userDto,
            ClientProfile = userDto.ClientProfile,
            OrderHistory = orders,
            Invoices = invoices,
            Files = allFiles,
            ActivityTimeline = activityTimeline
        };
    }

    public async Task<DesignerDetailDto?> GetDesignerDetailAsync(Guid designerId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.DesignerProfile)
            .FirstOrDefaultAsync(u => u.Id == designerId && !u.IsDeleted);

        if (user == null || user.Role?.Name != "Designer")
        {
            return null;
        }

        var userDto = await GetUserByIdAsync(designerId);
        if (userDto == null)
        {
            return null;
        }

        // Get assigned orders
        var assignedOrders = await _orderService.GetOrdersByDesignerAsync(designerId);

        // Calculate statistics
        var completedOrders = assignedOrders.Where(o => o.Status == "Completed").ToList();
        var completedCount = completedOrders.Count;

        // Calculate average delivery time
        // Get actual order entities to access UpdatedAt when status is Completed
        double? averageDeliveryTimeDays = null;
        if (completedOrders.Any())
        {
            var completedOrderIds = completedOrders.Select(o => o.Id).ToList();
            var completedOrderEntities = await _context.LogoOrders
                .Where(o => completedOrderIds.Contains(o.Id) && o.Status == Domain.Enums.OrderStatus.Completed)
                .ToListAsync();
            
            var deliveryTimes = completedOrderEntities
                .Where(o => o.UpdatedAt.HasValue)
                .Select(o => (o.UpdatedAt!.Value - o.CreatedAt).TotalDays)
                .ToList();
            
            if (deliveryTimes.Any())
            {
                averageDeliveryTimeDays = deliveryTimes.Average();
            }
        }

        var designerProfile = await _context.DesignerProfiles
            .FirstOrDefaultAsync(d => d.UserId == designerId && !d.IsDeleted);

        return new DesignerDetailDto
        {
            User = userDto,
            DesignerProfile = userDto.DesignerProfile,
            AssignedOrders = assignedOrders,
            CompletedOrdersCount = completedCount,
            AverageDeliveryTimeDays = averageDeliveryTimeDays,
            IsAvailable = designerProfile?.IsAvailable ?? false,
            Notes = designerProfile?.Notes
        };
    }
}
