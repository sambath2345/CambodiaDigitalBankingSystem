using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

/// <summary>Maps failures without exposing details, inner exceptions or server error messages.</summary>
public static class ExceptionResponseMapper
{
    public static (int StatusCode, ApiResponse<object?> Response) Map(Exception exception, string? traceId = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is RequestValidationException validation)
            return (validation.StatusCode, ApiResponse<object?>.Fail(validation.Message, validation.Errors, traceId));
        if (exception is DomainException domain)
            return (domain.StatusCode, ApiResponse<object?>.Fail(
                domain.StatusCode >= 500 ? MessageConstant.UnexpectedError : domain.Message, traceId: traceId));
        return (500, ApiResponse<object?>.Fail(MessageConstant.UnexpectedError, traceId: traceId));
    }
}
