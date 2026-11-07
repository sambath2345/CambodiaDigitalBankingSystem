using System.ComponentModel.DataAnnotations;

namespace CambodiaDigitalBankingSystem.SHARED.Base.Entity;

/// <summary>Provider-independent key and UTC audit metadata. Unspecified stored timestamps represent UTC.</summary>
public abstract class BaseEntity<TKey> where TKey : notnull
{
    private DateTime _createdAtUtc = DateTime.UtcNow;
    private DateTime? _updatedAtUtc;
    private DateTime? _deletedAtUtc;

    [Key]
    public virtual TKey Id { get; set; } = default!;
    public virtual DateTime CreatedAtUtc { get => _createdAtUtc; set => _createdAtUtc = AsUtc(value); }
    public string? CreatedBy { get; set; }
    public virtual DateTime? UpdatedAtUtc { get => _updatedAtUtc; set => _updatedAtUtc = AsUtc(value); }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public virtual DateTime? DeletedAtUtc { get => _deletedAtUtc; set => _deletedAtUtc = AsUtc(value); }
    public string? DeletedBy { get; set; }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Local => value.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value
    };
    private static DateTime? AsUtc(DateTime? value) => value.HasValue ? AsUtc(value.Value) : null;
}
