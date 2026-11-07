using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message = MessageConstant.Forbidden, object? details = null)
        : base(message, 403, details) { }
    public ForbiddenException(string message, Exception innerException)
        : base(message, 403, innerException: innerException) { }
}
