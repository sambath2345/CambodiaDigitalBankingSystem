namespace CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;

/// <summary>
/// Explicit opt-in to mutable/deletable persistence. Never expose this for ledger or audit records.
/// Implementations enforce deletion policy and stage changes until the unit of work commits.
/// </summary>
public interface IRepository<TEntity> : IAppendOnlyRepository<TEntity> where TEntity : class
{
    void Update(TEntity entity);
    /// <summary>Legacy removal method retained for existing implementations.</summary>
    void Delete(TEntity entity);
    /// <summary>Stages a removal permitted by the entity's retention policy.</summary>
    void Remove(TEntity entity) => Delete(entity);
}
