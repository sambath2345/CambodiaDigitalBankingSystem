using System.Text.Json;
using CambodiaDigitalBankingSystem.API.Middleware;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;
using CambodiaDigitalBankingSystem.SHARED.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CambodiaDigitalBankingSystem.TEST;

public class HttpAndSqlTests
{
    private sealed class Controller : CambodiaDigitalBankingSystem.SHARED.Base.Controller.BaseController
    {
        public ActionResult<ApiResponse<string>> SuccessResult() => Success("payload");
        public ActionResult<ApiResponse<string>> CreatedResult() => Created("Find", new { id = 1 }, "payload");
        public ActionResult<ApiResponse<string>> EmptyResult() => NoContentResponse<string>();
        public ActionResult<ApiResponse<string>> FailureResult(int status) => Failure<string>("private", status);
        public ActionResult<ApiResponse<IReadOnlyList<int>>> PageResult() =>
            SuccessPage(PagedResult<int>.Create([7], 1, 20, 1));
        public ActionResult<ApiResponse<string>> ServerErrorResult() => InternalServerErrorResponse<string>();
        public ActionResult<ApiResponse<string>> DetailedFailureResult() => Failure<string>("private", 500,
            new Dictionary<string, string[]> { ["Connection"] = ["Password=secret"] });
    }

    [Fact]
    public void ControllerHelpersPreserveHttpStatusLocationAndTrace()
    {
        var controller = new Controller { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        controller.HttpContext.TraceIdentifier = "trace";
        var ok = Assert.IsType<OkObjectResult>(controller.SuccessResult().Result);
        Assert.Equal("trace", Assert.IsType<ApiResponse<string>>(ok.Value).TraceId);
        var created = Assert.IsType<CreatedAtActionResult>(controller.CreatedResult().Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal("Find", created.ActionName);
        Assert.Equal(1, created.RouteValues!["id"]);
        Assert.Equal(MessageConstant.Created, Assert.IsType<ApiResponse<string>>(created.Value).Message);
        Assert.IsType<NoContentResult>(controller.EmptyResult().Result);
        foreach (var status in new[] { 400, 401, 403, 404, 409, 500 })
        {
            var result = Assert.IsType<ObjectResult>(controller.FailureResult(status).Result);
            Assert.Equal(status, result.StatusCode);
            if (status == 500) Assert.Equal(MessageConstant.UnexpectedError, Assert.IsType<ApiResponse<string>>(result.Value).Message);
        }
    }

    [Fact]
    public void PageAndServerErrorHelpersCarryMetadataAndRedactDetails()
    {
        var controller = new Controller { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        controller.HttpContext.TraceIdentifier = "trace";
        var result = Assert.IsType<OkObjectResult>(controller.PageResult().Result);
        var page = Assert.IsType<ApiResponse<IReadOnlyList<int>>>(result.Value);
        Assert.Equal(7, Assert.Single(page.Data!));
        Assert.Equal(1, page.Paging!.TotalItems);
        Assert.Equal("trace", page.TraceId);
        foreach (var failure in new[] { controller.ServerErrorResult(), controller.DetailedFailureResult() })
        {
            var error = Assert.IsType<ObjectResult>(failure.Result);
            Assert.Equal(500, error.StatusCode);
            var response = Assert.IsType<ApiResponse<string>>(error.Value);
            Assert.Equal(MessageConstant.UnexpectedError, response.Message);
            Assert.Null(response.Errors);
            Assert.Equal("trace", response.TraceId);
        }
    }

    [Theory]
    [InlineData(true, 404)]
    [InlineData(false, 500)]
    public async Task MiddlewareReturnsSafeEnvelope(bool expectedFailure, int status)
    {
        var context = new DefaultHttpContext { TraceIdentifier = "trace" };
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var middleware = new ExceptionHandlingMiddleware(_ =>
            throw (expectedFailure ? new NotFoundException("safe") : new Exception("Password=secret")),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(context);
        Assert.Equal(status, context.Response.StatusCode);
        body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiResponse<object?>>(body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.False(response!.Success);
        Assert.Equal("trace", response.TraceId);
        Assert.DoesNotContain("secret", JsonSerializer.Serialize(response));
    }

    [Fact]
    public async Task AbortedRequestDoesNotWriteAnErrorBody()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        await using var body = new MemoryStream();
        context.Response.Body = body;
        var middleware = new ExceptionHandlingMiddleware(_ => throw new OperationCanceledException(cancellation.Token),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        await middleware.InvokeAsync(context);
        Assert.Equal(499, context.Response.StatusCode);
        Assert.Equal(0, body.Length);
    }

    [Fact]
    public async Task SqlHelpersHonorCancellationBeforeReadingConfigurationOrOpeningConnections()
    {
        ISqlDataAccess sql = new SqlDataAccess(new ConfigurationBuilder().Build());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var token = cancellation.Token;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.ExecuteAsync("select @id", new { id = 1 }, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.LoadData<int, object>("select @id", new { id = 1 }, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.LoadSingleData<int, object>("select @id", new { id = 1 }, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.ExecuteScalarAsync<int, object>("select @id", new { id = 1 }, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.LoadMultipleData<int, int, object>("select @id", new { id = 1 }, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.LoadMultipleData<int, int, int, object>("select @id", new { id = 1 }, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.ExecuteTransactionAsync((_, _) => Task.CompletedTask, cancellationToken: token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sql.ExecuteTransactionAsync((_, _, _) => Task.CompletedTask, cancellationToken: token));
    }

    [Fact]
    public async Task MissingConnectionConfigurationFailsWithoutDatabaseAccess()
    {
        var sql = new SqlDataAccess(new ConfigurationBuilder().Build());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => sql.ExecuteAsync("select @id", new { id = 1 }));
        Assert.Contains("DefaultConnection", error.Message);
        Assert.Throws<ArgumentNullException>(() => new SqlDataAccess(null!));
    }
}
