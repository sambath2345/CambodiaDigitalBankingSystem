using System.Linq.Expressions;
using CambodiaDigitalBankingSystem.INFRASTRUCTURE.Base.Repository;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;
using Xunit;

namespace CambodiaDigitalBankingSystem.TEST;

public class BaseRepositoryTests
{
    private sealed record Entry(int Id);
    private sealed class Request : PagedRequest;
    private sealed class Repository : BaseRepository<Entry>
    {
        public List<Entry> Entries { get; } = [];
        public int PageReads { get; private set; }
        public CancellationToken ObservedToken { get; private set; }
        public override Task<Entry?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(Entries.Find(e => Equals(e.Id, id))); }
        public override Task<IReadOnlyList<Entry>> FindAsync(Expression<Func<Entry, bool>> predicate,
            CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult<IReadOnlyList<Entry>>(Entries.Where(predicate.Compile()).ToArray()); }
        public override Task<long> CountAsync(Expression<Func<Entry, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); return Task.FromResult(Entries.LongCount(predicate?.Compile() ?? (_ => true))); }
        protected override Task<IReadOnlyList<Entry>> ReadPageAsync(int skip, int take,
            Expression<Func<Entry, bool>>? predicate, CancellationToken cancellationToken)
        {
            PageReads++;
            ObservedToken = cancellationToken;
            return Task.FromResult<IReadOnlyList<Entry>>(Entries.Where(predicate?.Compile() ?? (_ => true))
                .OrderBy(e => e.Id).Skip(skip).Take(take).ToArray());
        }
        public override Task AddAsync(Entry entity, CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); Entries.Add(entity); return Task.CompletedTask; }
        public override void Update(Entry entity) => Entries[Entries.FindIndex(e => e.Id == entity.Id)] = entity;
        public override void Delete(Entry entity) => Entries.Remove(entity);
    }

    [Fact]
    public async Task PagingFiltersOrdersAndPropagatesCancellation()
    {
        var repository = new Repository();
        await repository.AddRangeAsync([new(3), new(1), new(2)]);
        using var cancellation = new CancellationTokenSource();
        var page = await repository.GetPagedAsync(new Request { Page = 2, PageSize = 1 },
            entry => entry.Id > 1, cancellation.Token);
        Assert.Equal(3, Assert.Single(page.Items).Id);
        Assert.Equal(2, page.TotalItems);
        Assert.Equal(cancellation.Token, repository.ObservedToken);
        Assert.True(await repository.ExistsAsync(entry => entry.Id == 1));
        Assert.Equal(3, (await repository.GetAllAsync()).Count);
        repository.Remove(new Entry(1));
        Assert.Equal(2, repository.Entries.Count);
    }

    [Theory]
    [InlineData(-1, 20)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public async Task InvalidPageNeverCallsProvider(int skip, int take)
    {
        var repository = new Repository();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetPageAsync(skip, take));
        Assert.Equal(0, repository.PageReads);
    }

    [Fact]
    public async Task InvalidBatchAndCancellationDoNotStageChanges()
    {
        var repository = new Repository();
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddRangeAsync([new(1), null!]));
        Assert.Empty(repository.Entries);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.AddRangeAsync([], cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.GetPageAsync(0, 20, cancellationToken: cancellation.Token));
        Assert.Equal(0, repository.PageReads);
    }
}
