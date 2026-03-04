namespace LogoDesignPortal.Application.DTOs.Files;

public class LogoGroupDto
{
    public Guid OrderId { get; set; }
    public string LogoName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FileCount { get; set; }
    public List<FileResponseDto> Files { get; set; } = new();
}

public class GroupedFilesResponseDto
{
    public List<LogoGroupDto> Logos { get; set; } = new();
    public int TotalLogos { get; set; }
    public int TotalFiles { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
