namespace CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;

/// <summary>Insertion capability for immutable ledger/audit entries. No update or removal.</summary>
public interface IAppendOnlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    /// <summary>Stages entries in the same unit of work; does not commit per entry.</summary>
    async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        foreach (var entity in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(entity);
            await AddAsync(entity, cancellationToken);
        }
    }
}
