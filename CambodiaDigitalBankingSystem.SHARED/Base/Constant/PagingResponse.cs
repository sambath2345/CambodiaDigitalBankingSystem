using CambodiaDigitalBankingSystem.SHARED.Base.Requests;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Constant;

/// <summary>Validated metadata. TotalPages is always derived using integer arithmetic.</summary>
public sealed record PagingResponse
{
    private int _page;
    private int _pageSize;
    private long _totalItems;

    public PagingResponse(int Page, int PageSize, long TotalItems, int TotalPages)
    {
        this.Page = Page;
        this.PageSize = PageSize;
        this.TotalItems = TotalItems;
        if (TotalPages != this.TotalPages)
            throw new ArgumentException("Total pages must match the count and page size.", nameof(TotalPages));
    }
    public int Page { get => _page; init => _page = ValidatePage(value); }
    public int PageSize { get => _pageSize; init => _pageSize = ValidateSize(value); }
    public long TotalItems
    {
        get => _totalItems;
        init { ArgumentOutOfRangeException.ThrowIfNegative(value); _totalItems = value; }
    }
    public int TotalPages
    {
        get => checked((int)(TotalItems / PageSize + (TotalItems % PageSize == 0 ? 0 : 1)));
        init
        {
            if (value != TotalPages)
                throw new ArgumentException("Total pages must match the count and page size.", nameof(value));
        }
    }
    public static PagingResponse From(int page, int pageSize, long totalItems)
    {
        ValidatePage(page);
        ValidateSize(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegative(totalItems);
        var pages = checked((int)(totalItems / pageSize + (totalItems % pageSize == 0 ? 0 : 1)));
        return new(page, pageSize, totalItems, pages);
    }
    public void Deconstruct(out int Page, out int PageSize, out long TotalItems, out int TotalPages) =>
        (Page, PageSize, TotalItems, TotalPages) = (this.Page, this.PageSize, this.TotalItems, this.TotalPages);

    internal static int ValidatePage(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
        return value;
    }
    internal static int ValidateSize(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, PagedRequest.MaximumPageSize);
        return value;
    }
}
