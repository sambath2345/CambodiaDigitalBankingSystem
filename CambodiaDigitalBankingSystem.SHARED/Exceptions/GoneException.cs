namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class GoneException : DomainException
{
    public GoneException(string message, object? data = null) : base(message, 410, data) { }
    public GoneException(string message, Exception innerException) : base(message, 410, null, innerException) { }
}
