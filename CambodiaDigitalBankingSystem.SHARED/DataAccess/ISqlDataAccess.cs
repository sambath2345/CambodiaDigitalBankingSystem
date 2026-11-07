using System.Data;

namespace CambodiaDigitalBankingSystem.SHARED.DataAccess;

public interface ISqlDataAccess
{
    /// <summary>
    ///     Executes a non-query command (INSERT, UPDATE, DELETE) and returns
    ///     the number of affected rows.
    /// </summary>
    /// <remarks>
    ///     Never concatenate user input into <paramref name="query"/>; always
    ///     pass values through <paramref name="parameters"/> so they are sent
    ///     as SQL parameters.
    /// </remarks>
    Task<int> ExecuteAsync<U>(
        string query,
        U parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Loads a list of rows mapped to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    ///     Never concatenate user input into <paramref name="storedProcedure"/>;
    ///     always pass values through <paramref name="parameters"/>.
    /// </remarks>
    Task<IEnumerable<T>> LoadData<T, U>(
        string storedProcedure,
        U parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Loads the first matching row mapped to <typeparamref name="T"/>,
    ///     or <c>null</c> when no row matches.
    /// </summary>
    Task<T?> LoadSingleData<T, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes a query returning a single scalar value, or <c>null</c>
    ///     when the result set is empty.
    /// </summary>
    Task<T?> ExecuteScalarAsync<T, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes a query yielding two result sets and reads them in order
    ///     into <typeparamref name="T1"/> and <typeparamref name="T2"/>.
    /// </summary>
    Task<(IEnumerable<T1>, IEnumerable<T2>)> LoadMultipleData<T1, T2, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes a query yielding three result sets and reads them in order
    ///     into <typeparamref name="T1"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/>.
    /// </summary>
    Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> LoadMultipleData<T1, T2, T3, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes multiple commands inside a single database transaction.
    ///     The transaction commits on success and rolls back on any exception.
    /// </summary>
    /// <param name="action">
    ///     Receives the open connection and active transaction; use Dapper's
    ///     async extension methods and pass the transaction to every call.
    /// </param>
    Task ExecuteTransactionAsync(
        Func<IDbConnection, IDbTransaction, Task> action,
        string connectionString = "Default",
        CancellationToken cancellationToken = default);

    /// <summary>Cancellation-aware callback. Use only its supplied connection and transaction for commands.</summary>
    Task ExecuteTransactionAsync(
        Func<IDbConnection, IDbTransaction, CancellationToken, Task> action,
        string connectionString = "Default",
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        cancellationToken.ThrowIfCancellationRequested();
        return ExecuteTransactionAsync(async (connection, transaction) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action(connection, transaction, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }, connectionString, cancellationToken);
    }
}
