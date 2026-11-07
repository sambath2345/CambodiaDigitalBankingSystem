namespace CambodiaDigitalBankingSystem.SHARED.Base.DataAccess;

/// <summary>One persistence scope. Implementations belong in infrastructure.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists staged changes atomically. Inside a transaction, must not commit that transaction.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Executes and commits one atomic operation, rolling back on failure or cancellation.
    /// All participating repositories share this scope and transaction.
    /// Implementations reject unsupported/nested transactions instead of running independently.
    /// The callback stages changes and calls SaveChangesAsync as needed.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
