using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;

namespace CambodiaDigitalBankingSystem.SHARED.Dto;

/// <summary>Legacy input names and zero-based GetPage. Use ToPagedRequest for canonical validation.</summary>
public class BaseListingRQ
{
    [Range(1, int.MaxValue)]
    public int? Page { get; set; }
    [Range(1, PagedRequest.MaximumPageSize)]
    public int? Size { get; set; }
    public string Query { get; set; } = string.Empty;
    public string Sort { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;
    public int GetPage() => Math.Max(1, Page ?? 1) - 1;
    public int GetSize() => Math.Clamp(Size ?? PagedRequest.DefaultPageSize, 1, PagedRequest.MaximumPageSize);
    /// <summary>Legacy identifier syntax guard. New SQL consumers must use the allow-list overload.</summary>
    public string GetSort() => !string.IsNullOrEmpty(Sort) && Regex.IsMatch(Sort, @"\A[A-Za-z_][A-Za-z0-9_]*\z") ? Sort : "Id";
    public string GetSort(IEnumerable<string> allowedColumns, string defaultColumn = "Id") =>
        ToPagedRequest().GetSortByOrDefault(allowedColumns, defaultColumn);
    public string GetOrder() => string.Equals(Order, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
    public bool HasQuery() => !string.IsNullOrWhiteSpace(Query);
    public string GetQuery() => string.IsNullOrWhiteSpace(Query) ? "ALL" : Query;

    public PagedRequest ToPagedRequest()
    {
        var request = new ListingPage
        {
            Page = Page ?? 1, PageSize = Size ?? PagedRequest.DefaultPageSize,
            Search = Query, SortBy = Sort, Descending = GetOrder() == "DESC"
        };
        request.Validate();
        return request;
    }
    private sealed class ListingPage : PagedRequest;
}
