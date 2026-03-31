using LogoDesignPortal.API.Attributes;
using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.API.Models;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClientDetailDto = LogoDesignPortal.Application.DTOs.Users.ClientDetailDto;
using DesignerDetailDto = LogoDesignPortal.Application.DTOs.Users.DesignerDetailDto;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, IPermissionService permissionService, IRoleService roleService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _permissionService = permissionService;
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet("me/permissions")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = User.GetUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "User identity could not be determined." });
        }
        var permissions = await _permissionService.GetUserPermissionsAsync(userId.Value);
        return Ok(permissions);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [RequirePermission("CreateUser")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        var roleMeta = await _roleService.GetByIdAsync(request.RoleId);
        if (roleMeta == null)
        {
            return BadRequest(new { error = "Invalid role specified." });
        }

        // Only SuperAdmin may assign the SuperAdmin role. Admins with CreateUser may create other roles.
        if (!User.IsInRole("SuperAdmin") &&
            string.Equals(roleMeta.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only SuperAdmin can assign the SuperAdmin role." });
        }

        try
        {
            var user = await _userService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize] // Must be authenticated
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        // Check if user can view this profile (own profile or admin)
        var currentUserId = User.GetUserIdOrThrow();
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");
        
        // Users can only view their own profile, admins can view any
        if (id != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "User not found." });
        }

        return Ok(user);
    }

    /// <summary>Typeahead search (10–20 items); debounce on the client (~300ms).</summary>
    [HttpGet("search")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<UserTypeaheadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchUsers([FromQuery] string? query, [FromQuery] string? role, [FromQuery] int limit = 15)
    {
        limit = Math.Clamp(limit, 1, 20);
        var items = await _userService.SearchUsersForTypeaheadAsync(query, role, limit);
        return Ok(items);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var paged = await _userService.GetUsersPagedAsync(page, pageSize);
        var totalPages = paged.PageSize > 0 ? (int)Math.Ceiling((double)paged.Total / paged.PageSize) : 0;
        return Ok(new ApiResponse<object>
        {
            Data = new { items = paged.Items, total = paged.Total, page = paged.Page, pageSize = paged.PageSize },
            Meta = new ApiMeta { Total = paged.Total, Page = paged.Page, PageSize = paged.PageSize, TotalPages = totalPages }
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var performedBy = User.GetUserIdOrThrow();
            var user = await _userService.UpdateUserAsync(id, request, performedBy);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("designer-profiles")]
    [Authorize] // Must be authenticated
    [RequirePermission("CreateDesignerProfile")] // Permission-based: SuperAdmin can grant to Admin
    [ProducesResponseType(typeof(DesignerProfileResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDesignerProfile([FromBody] CreateDesignerProfileRequestDto request)
    {
        try
        {
            var profile = await _userService.CreateDesignerProfileAsync(request);
            return CreatedAtAction(nameof(GetDesignerProfileByUserId), new { userId = request.UserId }, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("designer-profiles/{userId}")]
    [Authorize] // Must be authenticated
    [RequirePermission("ViewDesignerProfiles")] // Permission-based: SuperAdmin can grant to Admin
    [ProducesResponseType(typeof(DesignerProfileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDesignerProfileByUserId(Guid userId)
    {
        var profile = await _userService.GetDesignerProfileByUserIdAsync(userId);
        if (profile == null)
        {
            return NotFound(new { error = "Designer profile not found." });
        }

        return Ok(profile);
    }

    [HttpGet("designer-profiles")]
    [Authorize(Roles = "SuperAdmin,Admin")] // Clients must not access designer directory
    [RequirePermission("ViewDesignerProfiles")] // Permission-based: SuperAdmin can grant to Admin
    [ProducesResponseType(typeof(List<DesignerProfileResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllDesignerProfiles()
    {
        var profiles = await _userService.GetAllDesignerProfilesAsync();
        return Ok(profiles);
    }

    [HttpPut("{id}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateClientProfileDto request)
    {
        // Check if user can update (own profile or admin)
        var currentUserId = User.GetUserIdOrThrow();
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin");
        
        // Users can only update their own profile, admins can update any
        if (id != currentUserId && !isAdmin)
        {
            return Forbid();
        }

        try
        {
            var user = await _userService.UpdateUserProfileAsync(id, request);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid id, [FromQuery] bool permanent = false)
    {
        try
        {
            var deletedBy = User.GetUserIdOrThrow();
            if (permanent)
            {
                await _userService.HardDeleteUserAsync(id, deletedBy);
                return Ok(new { message = "User permanently removed from the system." });
            }
            else
            {
                await _userService.SoftDeactivateUserAsync(id, deletedBy);
                return Ok(new { message = "User deactivated successfully. You can reactivate them later." });
            }
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/activate")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReactivateUser(Guid id)
    {
        try
        {
            await _userService.ReactivateUserAsync(id);
            return Ok(new { message = "User reactivated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("clients/{id}/detail")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ClientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientDetail(Guid id)
    {
        var detail = await _userService.GetClientDetailAsync(id);
        if (detail == null)
        {
            return NotFound(new { error = "Client not found." });
        }
        return Ok(detail);
    }

    [HttpGet("designers/{id}/detail")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(DesignerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDesignerDetail(Guid id)
    {
        var detail = await _userService.GetDesignerDetailAsync(id);
        if (detail == null)
        {
            return NotFound(new { error = "Designer not found." });
        }
        return Ok(detail);
    }
}
