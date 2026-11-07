using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Dto;

/// <summary>Legacy paging names, validated and calculated by the canonical model.</summary>
public class PagingRS
{
    private PagingResponse _paging;
    public PagingRS(int page, int size, int totalPage, long totals) =>
        _paging = new(page, size, totals, totalPage);
    public PagingRS(PagedResultBase page)
    {
        ArgumentNullException.ThrowIfNull(page);
        _paging = PagingResponse.From(page.PageNumber, page.PageSize, page.TotalItems);
    }
    public int Page { get => _paging.Page; set => _paging = _paging with { Page = value }; }
    public int Size { get => _paging.PageSize; set => _paging = PagingResponse.From(Page, value, Totals); }
    public int TotalPage
    {
        get => _paging.TotalPages;
        set { if (value != TotalPage) throw new ArgumentException("Total pages must match the count.", nameof(value)); }
    }
    public long Totals { get => _paging.TotalItems; set => _paging = PagingResponse.From(Page, Size, value); }
    public PagingResponse ToPagingResponse() => _paging;
    public static PagingRS From(PagingResponse paging)
    {
        ArgumentNullException.ThrowIfNull(paging);
        return new(paging.Page, paging.PageSize, paging.TotalPages, paging.TotalItems);
    }
}

public class PagedResultBase
{
    private PagingResponse _paging;
    public PagedResultBase(int pageNumber, int pageSize, long totalItems) =>
        _paging = PagingResponse.From(pageNumber, pageSize, totalItems);
    public int PageNumber { get => _paging.Page; set => _paging = _paging with { Page = value }; }
    public int PageSize { get => _paging.PageSize; set => _paging = PagingResponse.From(PageNumber, value, TotalItems); }
    public int TotalPages
    {
        get => _paging.TotalPages;
        set { if (value != TotalPages) throw new ArgumentException("Total pages must match the count.", nameof(value)); }
    }
    public long TotalItems { get => _paging.TotalItems; set => _paging = PagingResponse.From(PageNumber, PageSize, value); }
}
