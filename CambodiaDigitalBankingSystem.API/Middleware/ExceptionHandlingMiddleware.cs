using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

namespace CambodiaDigitalBankingSystem.API.Middleware;

/// <summary>Translates failures at the HTTP boundary without logging or returning sensitive exception content.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            context.Response.StatusCode = 499; // Client closed the request; do not write to an aborted connection.
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted) throw;
            var (status, response) = ExceptionResponseMapper.Map(exception, context.TraceIdentifier);
            if (status >= 500)
                logger.LogError("Request {TraceId} failed with {ExceptionType}.", context.TraceIdentifier, exception.GetType().Name);
            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
        }
    }
}
