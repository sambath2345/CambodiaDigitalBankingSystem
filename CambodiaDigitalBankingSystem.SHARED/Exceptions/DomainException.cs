namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility base using the canonical status-aware exception hierarchy.</summary>
public abstract class DomainException : Base.Exceptions.DomainException
{
    protected DomainException(string message, object? data = null) : base(message, 400, data) { }
    protected DomainException(string message, int statusCode, object? data, Exception? innerException = null)
        : base(message, statusCode, data, innerException) { }
    public new object? Data => Details;
}
