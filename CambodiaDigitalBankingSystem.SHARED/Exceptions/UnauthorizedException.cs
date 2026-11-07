namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message, object? data = null) : base(message, 401, data) { }
    public UnauthorizedException(string message, Exception innerException) : base(message, 401, null, innerException) { }
}
