using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public class BusinessException : DomainException
{
    public BusinessException(string message = MessageConstant.BadRequest, object? details = null)
        : base(message, 400, details) { }
    public BusinessException(string message, Exception innerException)
        : base(message, 400, innerException: innerException) { }
}
