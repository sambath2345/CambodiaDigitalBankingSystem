namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class ReadyException : DomainException
{
    public ReadyException(string message, object? data = null) : base(message, 409, data) { }
    public ReadyException(string message, Exception innerException) : base(message, 409, null, innerException) { }
}
