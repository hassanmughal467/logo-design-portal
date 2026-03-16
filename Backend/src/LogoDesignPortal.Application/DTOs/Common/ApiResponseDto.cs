namespace LogoDesignPortal.Application.DTOs.Common;

public class ApiResponseDto<T>
{
    public T? Data { get; set; }
    public ApiMetaDto? Meta { get; set; }
}

public class ApiMetaDto
{
    public int? Total { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? TotalPages { get; set; }
}

public class ApiErrorDto
{
    public string Error { get; set; } = string.Empty;
    public string? Code { get; set; }
    public object? Details { get; set; }
}
