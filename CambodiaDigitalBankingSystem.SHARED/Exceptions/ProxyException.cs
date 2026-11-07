namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class ProxyException : DomainException
{
    public ProxyException(string message, object? data = null) : base(message, 502, data) { }
    public ProxyException(string message, Exception innerException) : base(message, 502, null, innerException) { }
}
