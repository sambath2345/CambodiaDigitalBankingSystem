using System.Linq.Expressions;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;

namespace CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;

/// <summary>Compatibility contract combining mutable persistence and bounded paging.</summary>
public interface IBaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    new Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetPageAsync(int skip, int take,
        Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    new Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>Override when count and page require a consistent database snapshot.</summary>
    async Task<PagedResult<TEntity>> GetPagedAsync(PagedRequest request,
        Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        var total = await CountAsync(predicate, cancellationToken);
        var items = await GetPageAsync(request.Offset, request.PageSize, predicate, cancellationToken);
        return PagedResult<TEntity>.Create(items, request.Page, request.PageSize, total);
    }
}
