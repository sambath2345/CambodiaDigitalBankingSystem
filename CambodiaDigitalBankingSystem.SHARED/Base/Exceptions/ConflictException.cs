using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public sealed class ConflictException : DomainException
{
    public ConflictException(string message = MessageConstant.Conflict, object? details = null)
        : base(message, 409, details) { }
    public ConflictException(string message, Exception innerException)
        : base(message, 409, innerException: innerException) { }
}
