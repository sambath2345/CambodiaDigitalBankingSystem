using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = MessageConstant.Unauthorized, object? details = null)
        : base(message, 401, details) { }
    public UnauthorizedException(string message, Exception innerException)
        : base(message, 401, innerException: innerException) { }
}
