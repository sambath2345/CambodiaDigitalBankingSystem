using System.Text.Json;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;
using CambodiaDigitalBankingSystem.SHARED.Dto;
using Xunit;

namespace CambodiaDigitalBankingSystem.TEST;

public class PaginationTests
{
    private sealed class Request : PagedRequest;

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(2147483647L, 100, 21474837)]
    public void TotalPagesUsesCorrectCeiling(long count, int size, int expected)
    {
        var page = PagedResult<int>.Create([], 1, size, count);
        Assert.Equal(expected, page.TotalPages);
        Assert.Equal(expected, new PagedResultBase(1, size, count).TotalPages);
        Assert.Equal(expected, new PagingRS(1, size, expected, count).TotalPage);
    }

    [Theory]
    [InlineData(0, 20, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 101, 1)]
    [InlineData(1, 20, -1)]
    public void AllConstructionPathsValidate(int page, int size, long count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PagingResponse.From(page, size, count));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PagedResult<int>([], page, size, count));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PagedResultBase(page, size, count));
    }

    [Fact]
    public void EmptyPageAndImmutableSnapshotAreSupported()
    {
        var items = new List<int> { 1 };
        var page = PagedResult<int>.Create(items, 1, 20, 1);
        items.Clear();
        Assert.Single(page.Items);
        Assert.Empty(PagedResult<int>.Create([], 50, 20, 1).Items);
        Assert.Throws<ArgumentException>(() => new PagedResult<int>([1, 2], 1, 1, 2));
        Assert.Throws<ArgumentException>(() => new PagedResult<int>([1], 1, 20, 0));
    }

    [Fact]
    public void CountsCannotOverflowSilentlyOrHaveConflictingMetadata()
    {
        Assert.Throws<OverflowException>(() => PagingResponse.From(1, 1, long.MaxValue));
        Assert.Throws<ArgumentException>(() => new PagingResponse(1, 20, 21, 1));
        var paging = PagingResponse.From(1, 20, 21);
        Assert.Equal(3, (paging with { PageSize = 10 }).TotalPages);
        Assert.Throws<ArgumentOutOfRangeException>(() => paging with { PageSize = 0 });
        Assert.Throws<ArgumentException>(() => paging with { TotalPages = 77 });
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public void InvalidRequestProducesValidationFailure(int page, int size)
    {
        var request = new Request { Page = page, PageSize = size };
        Assert.Throws<RequestValidationException>(request.Validate);
    }

    [Fact]
    public void LegacyRequestRetainsZeroBasedPageAndProvidesValidatedMigration()
    {
        var request = new BaseListingRQ { Page = 2, Size = 500, Sort = "Id; DROP TABLE data" };
        Assert.Equal(1, request.GetPage());
        Assert.Equal(100, request.GetSize());
        Assert.Equal("Id", request.GetSort());
        Assert.Throws<RequestValidationException>(() => request.ToPagedRequest());
        request.Size = 20;
        var canonical = request.ToPagedRequest();
        Assert.Equal(2, canonical.PageNumber);
        Assert.Equal(20, canonical.Offset);
        Assert.Equal("Id", canonical.GetSortByOrDefault(["Id"], "Id"));
        Assert.Throws<ArgumentException>(() => canonical.GetSortByOrDefault(["Name"], "Id"));
    }

    [Fact]
    public void CanonicalModelsSurviveJsonRoundTrip()
    {
        var page = PagedResult<string>.Create(["a"], 1, 20, 21);
        var copy = JsonSerializer.Deserialize<PagedResult<string>>(JsonSerializer.Serialize(page))!;
        Assert.Equal(2, copy.TotalPages);
        Assert.Equal(page.Items, copy.Items);
        Assert.Equal(page.Paging, JsonSerializer.Deserialize<PagingResponse>(JsonSerializer.Serialize(page.Paging)));
    }
}
