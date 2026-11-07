using System.Net;
using System.Text.Json;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;
using CambodiaDigitalBankingSystem.SHARED.Dto;
using Xunit;
using Legacy = CambodiaDigitalBankingSystem.SHARED.Exceptions;

namespace CambodiaDigitalBankingSystem.TEST;

public class ResponseAndExceptionTests
{
    [Fact]
    public void SuccessCreatedAndFailureCarryConsistentFields()
    {
        var ok = ApiResponse<string>.Ok("payload", traceId: "trace");
        Assert.True(ok.Success);
        Assert.Equal("payload", ok.Data);
        Assert.Equal("trace", ok.TraceId);
        Assert.Null(ok.Errors);
        Assert.Equal(MessageConstant.Created, ApiResponse<string>.Created("payload").Message);
        var failure = ApiResponse<string>.Fail("invalid", new Dictionary<string, string[]> { ["Page"] = ["invalid"] }, "trace");
        Assert.False(failure.Success);
        Assert.Null(failure.Data);
        Assert.Single(failure.Errors!);
    }

    [Fact]
    public void LegacyWireAdapterPreservesMetadataAndChecksStatus()
    {
        var response = ApiResponse<int>.Ok(12, paging: PagingResponse.From(1, 20, 1), traceId: "trace");
        var legacy = StructureRS.From(response, HttpStatusCode.Created);
        Assert.Equal(201, legacy.Status);
        var canonical = legacy.ToApiResponse();
        Assert.True(canonical.Success);
        Assert.Equal(12, canonical.Data);
        Assert.Equal(response.Paging, canonical.Paging);
        Assert.Equal("trace", canonical.TraceId);
        Assert.Throws<ArgumentException>(() => StructureRS.From(ApiResponse<int>.Fail("failed")));
    }

    public static IEnumerable<object[]> MappedExceptions()
    {
        yield return [new BusinessException("safe"), 400];
        yield return [new ConflictException("safe"), 409];
        yield return [new ForbiddenException(), 403];
        yield return [new UnauthorizedException(), 401];
        yield return [new NotFoundException("safe"), 404];
        yield return [new UnavailableException("private"), 503];
        yield return [new Legacy.BadRequestException("safe"), 400];
        yield return [new Legacy.ForbiddenException("safe"), 403];
        yield return [new Legacy.GoneException("safe"), 410];
        yield return [new Legacy.InternalServerErrorException("private"), 500];
        yield return [new Legacy.NotFoundException("safe"), 404];
        yield return [new Legacy.ProxyException("private"), 502];
        yield return [new Legacy.ReadyException("safe"), 409];
        yield return [new Legacy.TemporaryException("private"), 503];
        yield return [new Legacy.UnauthorizedException("safe"), 401];
        yield return [new InvalidOperationException("private"), 500];
    }

    [Theory]
    [MemberData(nameof(MappedExceptions))]
    public void EveryExceptionMapsSafely(Exception exception, int expected)
    {
        var mapped = ExceptionResponseMapper.Map(exception, "trace");
        Assert.Equal(expected, mapped.StatusCode);
        Assert.False(mapped.Response.Success);
        Assert.Null(mapped.Response.Data);
        Assert.Equal("trace", mapped.Response.TraceId);
        if (expected >= 500) Assert.Equal(MessageConstant.UnexpectedError, mapped.Response.Message);
    }

    [Fact]
    public void ErrorSerializationOmitsDetailsAndStackTraces()
    {
        const string secret = "Password=secret;Account=123456789";
        var exception = new BusinessException("safe", new { Connection = secret });
        var json = JsonSerializer.Serialize(ExceptionResponseMapper.Map(exception).Response);
        Assert.DoesNotContain(secret, json);
        Assert.DoesNotContain("StackTrace", json);
        Assert.DoesNotContain(secret, JsonSerializer.Serialize(ExceptionResponseMapper.Map(new Exception(secret)).Response));
        Assert.DoesNotContain("sensitive-key", NotFoundException.ForEntity("Account", "sensitive-key").Message);
    }

    [Fact]
    public void ValidationErrorsAreDefensivelyCopied()
    {
        var messages = new[] { "required" };
        var source = new Dictionary<string, string[]> { ["Name"] = messages };
        var exception = new RequestValidationException(source);
        messages[0] = "modified";
        source.Clear();
        exception.Errors["Name"][0] = "modified again";
        var mapped = ExceptionResponseMapper.Map(exception);
        Assert.Equal(400, mapped.StatusCode);
        Assert.Equal("required", mapped.Response.Errors!["Name"][0]);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(600)]
    public void DomainExceptionRejectsNonErrorStatus(int status) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new CustomException(status));

    private sealed class CustomException(int status) : DomainException("safe", status);
}
