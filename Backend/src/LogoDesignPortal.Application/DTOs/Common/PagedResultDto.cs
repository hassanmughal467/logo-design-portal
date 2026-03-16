namespace LogoDesignPortal.Application.DTOs.Common;

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
