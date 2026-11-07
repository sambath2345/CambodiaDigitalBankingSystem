namespace CambodiaDigitalBankingSystem.SHARED.Base.Constant;

/// <summary>A bounded page. Repositories must apply a deterministic ordering.</summary>
public sealed record PagedResult<T>
{
    private IReadOnlyList<T> _items;
    private int _page;
    private int _pageSize;
    private long _totalItems;

    public PagedResult(IReadOnlyList<T> Items, int Page, int PageSize, long TotalItems)
    {
        ArgumentNullException.ThrowIfNull(Items);
        _items = Array.AsReadOnly(Items.ToArray());
        _page = Page;
        _pageSize = PageSize;
        _totalItems = TotalItems;
        _ = Paging;
        if (Items.Count > PageSize || Items.Count > TotalItems)
            throw new ArgumentException("Items exceed the page size or total count.", nameof(Items));
    }
    public IReadOnlyList<T> Items
    {
        get => _items;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Count > PageSize || value.Count > TotalItems)
                throw new ArgumentException("Items exceed the page size or total count.", nameof(value));
            _items = Array.AsReadOnly(value.ToArray());
        }
    }
    public int Page { get => _page; init => _page = PagingResponse.ValidatePage(value); }
    public int PageSize
    {
        get => _pageSize;
        init
        {
            PagingResponse.ValidateSize(value);
            if (Items.Count > value) throw new ArgumentException("Page size is smaller than the payload.", nameof(value));
            _pageSize = value;
        }
    }
    public long TotalItems
    {
        get => _totalItems;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (Items.Count > value) throw new ArgumentException("Count is smaller than the payload.", nameof(value));
            _totalItems = value;
        }
    }
    public int TotalPages => Paging.TotalPages;
    public PagingResponse Paging => PagingResponse.From(Page, PageSize, TotalItems);
    public static PagedResult<T> Create(IEnumerable<T> items, int page, int pageSize, long totalItems)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new(items.ToArray(), page, pageSize, totalItems);
    }
    public void Deconstruct(out IReadOnlyList<T> Items, out int Page, out int PageSize, out long TotalItems) =>
        (Items, Page, PageSize, TotalItems) = (this.Items, this.Page, this.PageSize, this.TotalItems);
}
