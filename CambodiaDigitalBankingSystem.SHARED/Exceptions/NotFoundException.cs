namespace CambodiaDigitalBankingSystem.SHARED.Exceptions;

/// <summary>Compatibility exception mapped through Base.Exceptions.DomainException.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message, object? data = null) : base(message, 404, data) { }
    public NotFoundException(string message, Exception innerException) : base(message, 404, null, innerException) { }
}
