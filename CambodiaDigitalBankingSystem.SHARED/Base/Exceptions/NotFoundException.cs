using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message = MessageConstant.NotFound, object? details = null)
        : base(message, 404, details) { }
    public NotFoundException(string message, Exception innerException)
        : base(message, 404, innerException: innerException) { }

    /// <summary>The key is intentionally omitted to avoid exposing sensitive identifiers.</summary>
    public static NotFoundException ForEntity(string entityName, object key) => new(MessageConstant.NotFound);
}
