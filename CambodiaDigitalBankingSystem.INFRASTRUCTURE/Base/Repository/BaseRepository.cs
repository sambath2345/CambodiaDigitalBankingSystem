using System.Linq.Expressions;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;

namespace CambodiaDigitalBankingSystem.INFRASTRUCTURE.Base.Repository;

/// <summary>
/// Optional base for mutable repositories. Derived implementations enforce access scope,
/// soft deletion and retention, and stage writes in their unit of work without committing.
/// Ledger and audit repositories should implement read-only or append-only contracts instead.
/// </summary>
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    public abstract Task<TEntity?> GetByIdAsync<TKey>(TKey id,
        CancellationToken cancellationToken = default) where TKey : notnull;

    public abstract Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    public abstract Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return FindAsync(_ => true, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();
        return await CountAsync(predicate, cancellationToken) > 0;
    }

    public Task<IReadOnlyList<TEntity>> GetPageAsync(int skip, int take,
        Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfLessThan(take, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(take, PagedRequest.MaximumPageSize);
        cancellationToken.ThrowIfCancellationRequested();
        return ReadPageAsync(skip, take, predicate, cancellationToken);
    }

    /// <summary>Apply deterministic ordering before skipping/taking, and propagate cancellation to the provider.</summary>
    protected abstract Task<IReadOnlyList<TEntity>> ReadPageAsync(int skip, int take,
        Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);

    /// <summary>Override to read count and items in one database snapshot when required.</summary>
    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(PagedRequest request,
        Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        // Capture mutable request values before awaiting provider operations.
        var page = request.Page;
        var size = request.PageSize;
        var offset = request.Offset;
        var total = await CountAsync(predicate, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var items = await GetPageAsync(offset, size, predicate, cancellationToken);
        return PagedResult<TEntity>.Create(items, page, size, total);
    }

    public abstract Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = entities.ToArray();
        foreach (var entity in snapshot) ArgumentNullException.ThrowIfNull(entity);
        foreach (var entity in snapshot)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddAsync(entity, cancellationToken);
        }
    }

    public abstract void Update(TEntity entity);
    public abstract void Delete(TEntity entity);
    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Delete(entity);
    }
}
