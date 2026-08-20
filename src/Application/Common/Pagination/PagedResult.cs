namespace StockFlow.Application.Common.Pagination;

/// <summary>
/// Standard pagination envelope for list endpoints, per the pagination
/// convention in tech-stack.md. Offset-based (page/pageSize).
/// </summary>
public sealed record PagedResult<TItem>(IReadOnlyList<TItem> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
