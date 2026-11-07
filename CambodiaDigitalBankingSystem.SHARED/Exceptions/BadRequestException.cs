namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class BadRequestException : DomainException
{
    public BadRequestException(string message, object? data = null) : base(message, 400, data) { }
    public BadRequestException(string message, Exception innerException) : base(message, 400, null, innerException) { }
}
