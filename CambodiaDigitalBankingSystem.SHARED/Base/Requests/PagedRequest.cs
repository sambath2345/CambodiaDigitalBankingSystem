using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Requests;

/// <summary>One-based, bounded paging. Validate before querying; allow-list SQL sort identifiers.</summary>
public abstract class PagedRequest
{
    public const int DefaultPageSize = 20;
    public const int MaximumPageSize = 100;
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(1, MaximumPageSize)]
    public int PageSize { get; set; } = DefaultPageSize;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    [JsonIgnore]
    public int PageNumber { get => Page; set => Page = value; }
    [JsonIgnore]
    public int SafePage => Math.Max(1, Page);
    [JsonIgnore]
    public int SafePageSize => Math.Clamp(PageSize, 1, MaximumPageSize);
    [JsonIgnore]
    public int Offset => checked((SafePage - 1) * SafePageSize);
    [JsonIgnore]
    public string? NormalizedSearch => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();

    public void Validate()
    {
        var errors = new Dictionary<string, string[]>();
        if (Page < 1) errors[nameof(Page)] = ["Page numbers start at 1."];
        if (PageSize is < 1 or > MaximumPageSize)
            errors[nameof(PageSize)] = [$"Page size must be between 1 and {MaximumPageSize}."];
        if (errors.Count == 0 && (long)(Page - 1) * PageSize > int.MaxValue)
            errors[nameof(Page)] = ["The requested page exceeds the supported offset."];
        if (errors.Count > 0) throw new RequestValidationException(errors);
    }
    public string GetSortByOrDefault(IEnumerable<string> allowedColumns, string defaultColumn)
    {
        ArgumentNullException.ThrowIfNull(allowedColumns);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultColumn);
        var columns = allowedColumns.ToArray();
        var fallback = columns.FirstOrDefault(column =>
            string.Equals(column, defaultColumn, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("The default sort column must be allowed.", nameof(defaultColumn));
        return columns.FirstOrDefault(column =>
            string.Equals(column, SortBy, StringComparison.OrdinalIgnoreCase)) ?? fallback;
    }
}
