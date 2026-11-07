namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class TemporaryException : DomainException
{
    public TemporaryException(string message, object? data = null) : base(message, 503, data) { }
    public TemporaryException(string message, Exception innerException) : base(message, 503, null, innerException) { }
}
