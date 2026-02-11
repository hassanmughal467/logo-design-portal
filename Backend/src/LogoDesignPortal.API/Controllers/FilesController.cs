using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Application.Exceptions;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("upload/{orderId}")]
    [ProducesResponseType(typeof(FileUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(Guid orderId, IFormFile file, [FromForm] string? fileType = "Reference", [FromForm] string? description = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded." });
            }

            var uploadedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _fileService.UploadFileAsync(orderId, file, uploadedBy, fileType, description);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("upload-multiple/{orderId}")]
    [ProducesResponseType(typeof(List<FileUploadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadMultipleFiles(Guid orderId, [FromForm] IFormFile[] files, [FromForm] string? fileType = "Reference", [FromForm] string? description = null)
    {
        try
        {
            if (files == null || files.Length == 0)
            {
                return BadRequest(new { error = "No files uploaded." });
            }

            var uploadedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _fileService.UploadMultipleFilesAsync(orderId, files, uploadedBy, fileType, description);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(FileResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveFile(Guid id, [FromBody] ApproveFileDto request)
    {
        try
        {
            var approvedBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _fileService.ApproveFileAsync(id, request, approvedBy);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { error = "User not authenticated." });
            }

            var userId = Guid.Parse(userIdClaim);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            var (fileContent, fileName, contentType) = await _fileService.DownloadFileAsync(id, userId, userRole);
            
            return File(fileContent, contentType, fileName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (FormatException)
        {
            return Unauthorized(new { error = "Invalid user identifier." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while downloading the file." });
        }
    }

    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(List<FileUploadResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderFiles(Guid orderId)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var files = await _fileService.GetOrderFilesAsync(orderId, userId, userRole);
            return Ok(files);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var result = await _fileService.DeleteFileAsync(id, userId, userRole);
            return Ok(new { message = "File deleted successfully." });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
