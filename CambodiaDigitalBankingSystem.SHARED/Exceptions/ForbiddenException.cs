namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message, object? data = null) : base(message, 403, data) { }
    public ForbiddenException(string message, Exception innerException) : base(message, 403, null, innerException) { }
}
