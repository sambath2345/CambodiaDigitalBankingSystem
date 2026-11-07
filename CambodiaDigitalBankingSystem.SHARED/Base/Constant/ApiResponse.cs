namespace CambodiaDigitalBankingSystem.SHARED.Base.Constant;

/// <summary>A stable envelope. Supply DTOs containing only fields the caller may see.</summary>
public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data = default,
    IReadOnlyDictionary<string, string[]>? Errors = null,
    string? TraceId = null,
    PagingResponse? Paging = null)
{
    public static ApiResponse<T> Ok(T? data, string message = MessageConstant.Success,
        PagingResponse? paging = null, string? traceId = null) =>
        new(true, message, data, TraceId: traceId, Paging: paging);
    public static ApiResponse<T> Created(T? data, string? traceId = null) =>
        new(true, MessageConstant.Created, data, TraceId: traceId);
    public static ApiResponse<T> Fail(string message,
        IReadOnlyDictionary<string, string[]>? errors = null, string? traceId = null) =>
        new(false, message, default, errors, traceId);
}
