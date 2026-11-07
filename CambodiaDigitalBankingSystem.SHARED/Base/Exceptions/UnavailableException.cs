using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public sealed class UnavailableException : DomainException
{
    public UnavailableException(string message = MessageConstant.UnexpectedError, object? details = null)
        : base(message, 503, details) { }
    public UnavailableException(string message, Exception innerException)
        : base(message, 503, innerException: innerException) { }
}
