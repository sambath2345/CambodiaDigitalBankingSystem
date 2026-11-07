namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class InternalServerErrorException : DomainException
{
    public InternalServerErrorException(string message, object? data = null) : base(message, 500, data) { }
    public InternalServerErrorException(string message, Exception innerException) : base(message, 500, null, innerException) { }
}
