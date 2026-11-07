using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CambodiaDigitalBankingSystem.SHARED.DataAccess;

/// <summary>SQL Server implementation. Legacy namespace retained; assembly is INFRASTRUCTURE.</summary>
public class SqlDataAccess : ISqlDataAccess
{
    private const string DefaultConnectionStringSentinel = "Default";
    private const string DefaultConnectionStringKey = "DefaultConnection";

    private readonly IConfiguration _configuration;
    private readonly AsyncLocal<bool> _transactionActive = new();

    public SqlDataAccess(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<int> ExecuteAsync<U>(
        string query,
        U parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);

        return await connection.ExecuteAsync(new CommandDefinition(
            query,
            parameters,
            commandType: commandType,
            cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<T>> LoadData<T, U>(
        string storedProcedure,
        U parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedProcedure);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);

        return await connection.QueryAsync<T>(new CommandDefinition(
            storedProcedure,
            parameters,
            commandType: commandType,
            cancellationToken: cancellationToken));
    }

    public async Task<T?> LoadSingleData<T, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(
            query,
            parameters,
            commandType: commandType,
            cancellationToken: cancellationToken));
    }

    public async Task<T?> ExecuteScalarAsync<T, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);

        return await connection.ExecuteScalarAsync<T>(new CommandDefinition(
            query,
            parameters,
            commandType: commandType,
            cancellationToken: cancellationToken));
    }

    public async Task<(IEnumerable<T1>, IEnumerable<T2>)> LoadMultipleData<T1, T2, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            query,
            parameters,
            commandType: commandType,
            cancellationToken: cancellationToken));

        var result1 = await multi.ReadAsync<T1>();
        var result2 = await multi.ReadAsync<T2>();

        return (result1, result2);
    }

    public async Task<(IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>)> LoadMultipleData<T1, T2, T3, TU>(
        string query,
        TU parameters,
        CommandType commandType = CommandType.Text,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(new CommandDefinition(
            query,
            parameters,
            commandType: commandType,
            cancellationToken: cancellationToken));

        var result1 = await multi.ReadAsync<T1>();
        var result2 = await multi.ReadAsync<T2>();
        var result3 = await multi.ReadAsync<T3>();

        return (result1, result2, result3);
    }

    public async Task ExecuteTransactionAsync(
        Func<IDbConnection, IDbTransaction, Task> action,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        await ExecuteTransactionAsync((connection, transaction, token) => action(connection, transaction),
            connectionString, cancellationToken);
    }

    /// <summary>Runs commands on the supplied connection/transaction and propagates cancellation to the callback.</summary>
    public async Task ExecuteTransactionAsync(
        Func<IDbConnection, IDbTransaction, CancellationToken, Task> action,
        string connectionString = DefaultConnectionStringSentinel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await using var connection = await CreateOpenConnectionAsync(connectionString, cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        _transactionActive.Value = true;
        try
        {
            await action(connection, transaction, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception operationError)
        {
            try { await transaction.RollbackAsync(CancellationToken.None); }
            catch (Exception rollbackError)
            {
                throw new AggregateException("The operation and transaction rollback both failed.", operationError, rollbackError);
            }
            throw;
        }
        finally { _transactionActive.Value = false; }
    }

    private string GetConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var connStr = connectionString == DefaultConnectionStringSentinel
            ? _configuration.GetConnectionString(DefaultConnectionStringKey)
            : connectionString;

        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException(connectionString == DefaultConnectionStringSentinel
                ? $"Connection string '{DefaultConnectionStringKey}' is not configured. " +
                  "Check the ConnectionStrings section of appsettings.json."
                : "The provided connection string is null or empty.");

        return connStr;
    }

    private async Task<SqlConnection> CreateOpenConnectionAsync(
        string connectionStringNameOrValue,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_transactionActive.Value)
            throw new InvalidOperationException(
                "Use the callback connection and transaction for every command. Independent connections and nested transactions are not allowed.");
        var connection = new SqlConnection(GetConnectionString(connectionStringNameOrValue));

        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
