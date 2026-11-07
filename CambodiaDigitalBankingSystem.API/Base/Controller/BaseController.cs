using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using Microsoft.AspNetCore.Mvc;

// Namespace retained for existing source consumers; HTTP helpers are compiled only into API.
namespace CambodiaDigitalBankingSystem.SHARED.Base.Controller;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T? data, string message,
        PagingResponse? paging) =>
        Ok(ApiResponse<T>.Ok(data, message, paging, HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<T>> Success<T>(T? data, string message = MessageConstant.Success) =>
        Success(data, message, null);

    protected ActionResult<ApiResponse<T>> Created<T>(string actionName, object? routeValues, T data) =>
        CreatedAtAction(actionName, routeValues, ApiResponse<T>.Created(data, HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<T>> NoContentResponse<T>() => NoContent();

    protected ActionResult<ApiResponse<IReadOnlyList<T>>> SuccessPage<T>(PagedResult<T> page,
        string message = MessageConstant.Success)
    {
        ArgumentNullException.ThrowIfNull(page);
        return Success(page.Items, message, page.Paging);
    }

    protected ActionResult<ApiResponse<T>> InternalServerErrorResponse<T>() =>
        Failure<T>(MessageConstant.UnexpectedError, 500);
    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message = MessageConstant.BadRequest,
        IReadOnlyDictionary<string, string[]>? errors = null) => Failure<T>(message, 400, errors);
    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string message = MessageConstant.Unauthorized) =>
        Failure<T>(message, 401);
    protected ActionResult<ApiResponse<T>> ForbiddenResponse<T>(string message = MessageConstant.Forbidden) =>
        Failure<T>(message, 403);
    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message = MessageConstant.NotFound) =>
        Failure<T>(message, 404);
    protected ActionResult<ApiResponse<T>> ConflictResponse<T>(string message = MessageConstant.Conflict) =>
        Failure<T>(message, 409);
    protected ActionResult<ApiResponse<T>> Failure<T>(string message, int statusCode,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        if (statusCode is < 400 or > 599) throw new ArgumentOutOfRangeException(nameof(statusCode));
        return StatusCode(statusCode, ApiResponse<T>.Fail(
            statusCode >= 500 ? MessageConstant.UnexpectedError : message,
            statusCode >= 500 ? null : errors, HttpContext.TraceIdentifier));
    }
}
