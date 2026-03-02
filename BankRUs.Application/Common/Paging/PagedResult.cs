namespace BankRUs.Application.Common.Paging;

public class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

