using System.Collections.ObjectModel;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

public sealed class RequestValidationException : BusinessException
{
    public RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(MessageConstant.ValidationError)
    {
        ArgumentNullException.ThrowIfNull(errors);
        _errors = errors.ToDictionary(pair => pair.Key, pair =>
            pair.Value?.ToArray() ?? throw new ArgumentException("Validation messages cannot be null.", nameof(errors)));
    }
    private readonly Dictionary<string, string[]> _errors;
    public IReadOnlyDictionary<string, string[]> Errors =>
        new ReadOnlyDictionary<string, string[]>(_errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()));
}
