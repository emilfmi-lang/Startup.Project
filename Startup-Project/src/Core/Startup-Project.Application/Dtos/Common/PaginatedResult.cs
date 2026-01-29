namespace Startup_Project.Application.Dtos.Common;

public record PaginatedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;

    public static PaginatedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }
}
