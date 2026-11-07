using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using CambodiaDigitalBankingSystem.SHARED.Base.Entity;
using CambodiaDigitalBankingSystem.SHARED.Base.Exceptions;
using CambodiaDigitalBankingSystem.SHARED.Base.Requests;
using CambodiaDigitalBankingSystem.SHARED.Base.Services;
using Xunit;

namespace CambodiaDigitalBankingSystem.TEST;

public class EntityAndServiceTests
{
    private sealed class Entity : BaseEntity<Guid>;
    private sealed class Request : PagedRequest;
    private sealed class Service : BaseService
    {
        public void Validate(object input, CancellationToken token = default) => ValidateInput(input, token);
    }

    [Fact]
    public void MetadataSupportsGenericKeysNullableActorsAndUtc()
    {
        var entity = new Entity { Id = Guid.NewGuid() };
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAtUtc.Kind);
        Assert.Null(entity.CreatedBy);
        Assert.Null(entity.UpdatedAtUtc);
        Assert.Null(entity.DeletedAtUtc);
        Assert.False(entity.IsDeleted);
        var unspecified = new DateTime(2026, 1, 1);
        entity.UpdatedAtUtc = unspecified;
        entity.DeletedAtUtc = DateTime.Now;
        Assert.Equal(DateTimeKind.Utc, entity.UpdatedAtUtc.Value.Kind);
        Assert.Equal(unspecified.Ticks, entity.UpdatedAtUtc.Value.Ticks);
        Assert.Equal(DateTimeKind.Utc, entity.DeletedAtUtc.Value.Kind);
        entity.IsDeleted = true;
        entity.DeletedBy = "actor";
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public void LegacyIdentityAndColumnMappingsArePreserved()
    {
        var entity = new CambodiaDigitalBankingSystem.SHARED.Entity.BaseEntity { Id = 23 };
        entity.CreatedAt = new DateTime(2026, 1, 1);
        Assert.Equal(23, ((BaseEntity<int>)entity).Id);
        Assert.Equal(entity.CreatedAt, entity.CreatedAtUtc);
        var type = entity.GetType();
        Assert.NotNull(type.GetProperty("Id")!.GetCustomAttribute<KeyAttribute>());
        Assert.Equal(DatabaseGeneratedOption.Identity,
            type.GetProperty("Id")!.GetCustomAttribute<DatabaseGeneratedAttribute>()!.DatabaseGeneratedOption);
        Assert.Equal("CreatedAt", type.GetProperty("CreatedAtUtc")!.GetCustomAttribute<ColumnAttribute>()!.Name);
        Assert.NotNull(type.GetProperty("CreatedAt")!.GetCustomAttribute<NotMappedAttribute>());
    }

    [Fact]
    public void ServiceValidationHonorsAnnotationsAndCancellation()
    {
        var service = new Service();
        var exception = Assert.Throws<RequestValidationException>(() => service.Validate(new Request { PageSize = 0 }));
        Assert.Contains("PageSize", exception.Errors.Keys);
        service.Validate(new Request());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => service.Validate(new Request(), cancellation.Token));
    }

    [Fact]
    public void SharedAssemblyHasNoProviderOrHttpDependency()
    {
        var references = typeof(BaseEntity<>).Assembly.GetReferencedAssemblies().Select(name => name.Name).ToArray();
        Assert.DoesNotContain(references, name => name!.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain("Dapper", references);
        Assert.DoesNotContain("Microsoft.Data.SqlClient", references);
    }
}
