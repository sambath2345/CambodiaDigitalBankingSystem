using System.Linq.Expressions;

namespace CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;

/// <summary>Read access only. Implementations enforce caller data scope, including soft deletion.</summary>
public interface IReadOnlyRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default) where TKey : notnull;
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task<long> CountAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        FindAsync(_ => true, cancellationToken);
    async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        cancellationToken.ThrowIfCancellationRequested();
        return await CountAsync(predicate, cancellationToken) > 0;
    }
}
