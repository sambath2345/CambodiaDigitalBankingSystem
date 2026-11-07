using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CambodiaDigitalBankingSystem.SHARED.Entity;

/// <summary>Compatibility model retaining the existing int identity and column names.</summary>
public class BaseEntity : Base.Entity.BaseEntity<int>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override int Id { get => base.Id; set => base.Id = value; }

    [Column("CreatedAt")]
    public override DateTime CreatedAtUtc { get => base.CreatedAtUtc; set => base.CreatedAtUtc = value; }
    [Column("UpdatedAt")]
    public override DateTime? UpdatedAtUtc { get => base.UpdatedAtUtc; set => base.UpdatedAtUtc = value; }
    [Column("DeletedAt")]
    public override DateTime? DeletedAtUtc { get => base.DeletedAtUtc; set => base.DeletedAtUtc = value; }

    [NotMapped]
    public DateTime CreatedAt { get => CreatedAtUtc; set => CreatedAtUtc = value; }
    [NotMapped]
    public DateTime? UpdatedAt { get => UpdatedAtUtc; set => UpdatedAtUtc = value; }
    [NotMapped]
    public DateTime? DeletedAt { get => DeletedAtUtc; set => DeletedAtUtc = value; }
}
