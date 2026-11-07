using System.Linq.Expressions;
using CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;
using Xunit;

namespace CambodiaDigitalBankingSystem.TEST;

public class RepositoryContractTests
{
    private sealed class Entry { public int Id { get; init; } }
    private sealed class Request : PagedRequest;

    // Implements the pre-repair interface members only, demonstrating compatible default additions.
    private sealed class Repository : IBaseRepository<Entry>
    {
        public List<Entry> Entries { get; } = [];
        public CancellationToken LastToken { get; private set; }
        private void Observe(CancellationToken token) { token.ThrowIfCancellationRequested(); LastToken = token; }
        public Task<Entry?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull
        { Observe(cancellationToken); return Task.FromResult(Entries.FirstOrDefault(e => Equals(e.Id, id))); }
        public Task<IReadOnlyList<Entry>> FindAsync(Expression<Func<Entry, bool>> predicate, CancellationToken cancellationToken = default)
        { Observe(cancellationToken); return Task.FromResult<IReadOnlyList<Entry>>(Entries.Where(predicate.Compile()).ToArray()); }
        public Task<long> CountAsync(Expression<Func<Entry, bool>>? predicate = null, CancellationToken cancellationToken = default)
        { Observe(cancellationToken); return Task.FromResult(Entries.LongCount(predicate?.Compile() ?? (_ => true))); }
        public Task<IReadOnlyList<Entry>> GetAllAsync(CancellationToken cancellationToken = default) =>
            FindAsync(_ => true, cancellationToken);
        public Task<IReadOnlyList<Entry>> GetPageAsync(int skip, int take, Expression<Func<Entry, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            Observe(cancellationToken);
            return Task.FromResult<IReadOnlyList<Entry>>(Entries.Where(predicate?.Compile() ?? (_ => true))
                .OrderBy(e => e.Id).Skip(skip).Take(take).ToArray());
        }
        public async Task<bool> ExistsAsync(Expression<Func<Entry, bool>> predicate, CancellationToken cancellationToken = default) =>
            await CountAsync(predicate, cancellationToken) > 0;
        public Task AddAsync(Entry entity, CancellationToken cancellationToken = default)
        { Observe(cancellationToken); Entries.Add(entity); return Task.CompletedTask; }
        public void Update(Entry entity) { Entries[Entries.FindIndex(e => e.Id == entity.Id)] = entity; }
        public void Delete(Entry entity) => Entries.Remove(entity);
    }

    [Fact]
    public async Task OriginalImplementationSupportsAddRangeRemoveAndPaging()
    {
        var implementation = new Repository();
        IBaseRepository<Entry> repository = implementation;
        using var cancellation = new CancellationTokenSource();
        var entries = Enumerable.Range(1, 21).Select(id => new Entry { Id = id }).ToArray();
        await repository.AddRangeAsync(entries, cancellation.Token);
        var page = await repository.GetPagedAsync(new Request { Page = 2, PageSize = 20 }, cancellationToken: cancellation.Token);
        Assert.Single(page.Items);
        Assert.Equal(21, page.TotalItems);
        Assert.Equal(2, page.TotalPages);
        Assert.Equal(cancellation.Token, implementation.LastToken);
        Assert.NotNull(await repository.GetByIdAsync(1, cancellation.Token));
        Assert.True(await repository.ExistsAsync(e => e.Id == 1, cancellation.Token));
        repository.Remove(entries[0]);
        Assert.Equal(20, (await repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task InvalidPagesAndCancellationStopBeforeMutation()
    {
        var implementation = new Repository();
        IBaseRepository<Entry> repository = implementation;
        await Assert.ThrowsAsync<RequestValidationException>(() => repository.GetPagedAsync(new Request { Page = 0 }));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repository.AddRangeAsync([new Entry { Id = 1 }], cancellation.Token));
        Assert.Empty(implementation.Entries);
    }

    [Fact]
    public void LedgerAndAuditContractsExposeNoMutationOrDeletion()
    {
        var types = new[] { typeof(IReadOnlyRepository<Entry>), typeof(IAppendOnlyRepository<Entry>) };
        foreach (var type in types)
        {
            var names = type.GetInterfaces().Append(type).SelectMany(t => t.GetMethods()).Select(m => m.Name);
            Assert.DoesNotContain("Delete", names);
            Assert.DoesNotContain("Remove", names);
            Assert.DoesNotContain("Update", names);
        }
        Assert.All(typeof(IUnitOfWork).GetMethods(),
            method => Assert.Contains(method.GetParameters(), parameter => parameter.ParameterType == typeof(CancellationToken)));
    }
}
