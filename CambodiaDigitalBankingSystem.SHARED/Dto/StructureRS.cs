using System.Net;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Dto;

/// <summary>Legacy wire envelope. New endpoints use ApiResponse; adapters retain existing JSON properties.</summary>
public class StructureRS
{
    public StructureRS() { }
    public StructureRS(object? data) => Data = data;
    public StructureRS(object? data, PagingRS paging) { Data = data; Paging = paging; }
    public StructureRS(HttpStatusCode status, string message) => ApplyStatus(status, message);
    public StructureRS(HttpStatusCode status, string message, object? data)
    { ApplyStatus(status, message); Data = data; }

    public int Status { get; set; } = 200;
    public string Message { get; set; } = MessageConstant.Success;
    public string MessageKey { get; set; } = GenerateKey(MessageConstant.Success);
    public object? Data { get; set; }
    public PagingRS? Paging { get; set; }
    public string? TraceId { get; set; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }

    public ApiResponse<object?> ToApiResponse() => new(Status is >= 200 and < 300,
        Message, Data, Errors, TraceId, Paging?.ToPagingResponse());

    public static StructureRS From<T>(ApiResponse<T> response, HttpStatusCode status = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.Success != ((int)status is >= 200 and < 300))
            throw new ArgumentException("HTTP status must agree with response success.", nameof(status));
        return new(status, response.Message, response.Data)
        {
            TraceId = response.TraceId, Errors = response.Errors,
            Paging = response.Paging is null ? null : PagingRS.From(response.Paging)
        };
    }
    private static string GenerateKey(string message) => message.ToLowerInvariant().Replace(" ", "_");
    private void ApplyStatus(HttpStatusCode status, string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Status = (int)status; Message = message; MessageKey = GenerateKey(message);
    }
}
