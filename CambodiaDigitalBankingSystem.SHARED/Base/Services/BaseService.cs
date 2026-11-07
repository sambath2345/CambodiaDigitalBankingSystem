using System.ComponentModel.DataAnnotations;
using CambodiaDigitalBankingSystem.SHARED.Base.Constant;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Services;

/// <summary>Optional helpers. Concrete services receive dependencies via constructor injection.</summary>
public abstract class BaseService
{
    protected static ApiResponse<T> Respond<T>(T? data, string message = MessageConstant.Success) =>
        ApiResponse<T>.Ok(data, message);
    protected static ApiResponse<IReadOnlyList<T>> RespondPage<T>(IReadOnlyList<T> items, int page, int pageSize,
        long totalItems, string message = MessageConstant.Success)
    {
        var result = PagedResult<T>.Create(items, page, pageSize, totalItems);
        return ApiResponse<IReadOnlyList<T>>.Ok(result.Items, message, result.Paging);
    }
    protected static ApiResponse<T> Reject<T>(string message,
        IReadOnlyDictionary<string, string[]>? errors = null) => ApiResponse<T>.Fail(message, errors);

    protected static void ValidateInput(object input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(input);
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true))
            return;
        var errors = results.SelectMany(result =>
                result.MemberNames.DefaultIfEmpty(string.Empty).Select(member =>
                    (Member: member, Message: result.ErrorMessage ?? MessageConstant.BadRequest)))
            .GroupBy(item => item.Member)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Message).Distinct().ToArray());
        throw new RequestValidationException(errors);
    }
}
