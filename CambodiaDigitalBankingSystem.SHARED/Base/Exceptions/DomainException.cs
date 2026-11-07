namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

/// <summary>Expected failure. Messages for 4xx statuses must be safe for callers; Details are never serialized by the mapper.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message, int statusCode, object? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        if (statusCode is < 400 or > 599)
            throw new ArgumentOutOfRangeException(nameof(statusCode), "An error status must be between 400 and 599.");
        StatusCode = statusCode;
        Details = details;
    }
    public int StatusCode { get; }
    public object? Details { get; }
}
