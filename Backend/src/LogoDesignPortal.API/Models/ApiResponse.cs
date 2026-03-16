namespace LogoDesignPortal.API.Models;

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public ApiMeta? Meta { get; set; }
}

public class ApiMeta
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? Total { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? TotalPages { get; set; }
}

public class ApiErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string? Code { get; set; }
    public object? Details { get; set; }
}
