# SHARED repair and migration notes

## Controller and repository completion

The current solution restores and builds with 0 warnings and 0 errors; all 58 unit tests pass. This supersedes the earlier 52-test verification recorded below.

- Updated API/Base/Controller/BaseController.cs with SuccessPage and InternalServerErrorResponse helpers. Server-error responses now discard caller-supplied error details as well as masking the message. Existing signatures and both controller namespaces are retained.
- Added INFRASTRUCTURE/Base/Repository/BaseRepository.cs as an optional abstract implementation of IBaseRepository<TEntity>. It provides validated bounded paging, existence checks, bulk-add prevalidation, cancellation propagation and removal delegation. Derived implementations supply provider queries and staged mutations. GetPagedAsync is overridable for snapshot consistency; it does not begin or commit transactions itself.
- Added TEST/BaseRepositoryTests.cs and extended TEST/HttpAndSqlTests.cs. No packages or project references were added, and no files were deleted in this completion.

SHARED keeps its existing Base/Constant, Base/DataAccess, Base/Entity, Base/Exceptions, Base/Http, Base/Requests and Base/Services folders. Controllers belong to API; repository implementation helpers belong to INFRASTRUCTURE. The legacy entity, constants, DTOs, exceptions and controller names are retained as compatibility adapters. The old misspelled BaseRepotory is absent from the current source tree and has no consumers.

The new repository base deliberately requires persistence methods to be implemented by a derived provider repository; it is not an in-memory database or a generic SQL generator. Mutable repositories must enforce retention policies and should not be used for ledger or audit records. Bulk additions do not commit: providers must use one unit of work and discard staged changes or roll back if an operation fails. No live SQL integration tests were run.

All projects target net9.0. The solution selects installed SDK 9.0.313 through global.json.

## Inspection and original errors

The checkout contained six projects, no TEST project, and no CORE entity classes, repository implementations, service consumers, or inheriting controllers. Only API/Program.cs contained application code (the weather example). Existing staged/unstaged work was retained.

| Reported error | Repair |
| --- | --- |
| CS0246 Dapper; CS0234 Microsoft.Data; CS0246 SqlConnection | The working tree already had Dapper 2.1.89 and Microsoft.Data.SqlClient 7.1.1 references. Their implementation and references now belong to INFRASTRUCTURE, where compilation verifies resolution. |
| CS0234 POS_SYSTEM.COMMAND.Constants | StructureRS uses Base.Constant.MessageConstant. The nonexistent MessageConstants.SUCCESSFULLY reference was removed. |
| CS0246 DomainException | ForbiddenException now has the correct CambodiaDigitalBankingSystem namespace; legacy exceptions inherit the canonical status-aware base. |
| CS0246 KeyAttribute/Key, DatabaseGeneratedAttribute/DatabaseGenerated; CS0103 DatabaseGeneratedOption | Added standard DataAnnotations/Schema imports and preserved the legacy int identity mapping. No EF Core dependency was added. |

The initial build reproduced the POS namespace, DomainException and annotation errors. It also hit a sandbox output-write denial; subsequent approved builds used the existing build locations.

## Final source organization

```text
CambodiaDigitalBankingSystem.SHARED/
  Base/
    Constant/
      ApiResponse.cs
      MessageConstant.cs
      PagedResult.cs
      PagingResponse.cs
    DataAccess/
      IBaseRepository.cs
      IReadOnlyRepository.cs
      IAppendOnlyRepository.cs
      IRepository.cs
      IUnitOfWork.cs
    Entity/BaseEntity.cs
    Exceptions/
      DomainException.cs
      BusinessException.cs
      NotFoundException.cs
      ConflictException.cs
      ForbiddenException.cs
      UnauthorizedException.cs
      RequestValidationException.cs
      UnavailableException.cs
      ExceptionResponseMapper.cs
    Http/HttpStatusCodeConstant.cs
    Requests/PagedRequest.cs
    Services/BaseService.cs
  Constant/MessageConstant.cs
  DataAccess/ISqlDataAccess.cs
  Dto/
    BaseListingRQ.cs
    PagingRS.cs               (also retains PagedResultBase)
    StructureRS.cs
  Entity/BaseEntity.cs
  Exceptions/                (legacy compatibility exceptions)
  CambodiaDigitalBankingSystem.SHARED.csproj
  README.md

CambodiaDigitalBankingSystem.INFRASTRUCTURE/
  DataAccess/SqlDataAccess.cs
CambodiaDigitalBankingSystem.API/
  Base/Controller/BaseController.cs
  Controller/BaseController.cs
  Middleware/ExceptionHandlingMiddleware.cs
CambodiaDigitalBankingSystem.TEST/
  ResponseAndExceptionTests.cs
  PaginationTests.cs
  EntityAndServiceTests.cs
  RepositoryContractTests.cs
  HttpAndSqlTests.cs
```

## Contracts and compatibility

- BaseEntity<TKey> owns the key and eight audit/deletion fields. UTC setters convert local values and tag unspecified stored values as UTC. The legacy BaseEntity inherits BaseEntity<int>, retains identity generation and the CreatedAt/UpdatedAt/DeletedAt columns, and exposes the old property names as nonmapped aliases. No inheriting CORE entities existed to migrate. No schema migration was run.
- ApiResponse<T> provides success, created and failure factories, validation errors, paging and a trace ID. StructureRS retains its original wire property names and offers ToApiResponse/From adapters. Return authorized DTOs: a generic envelope cannot infer which fields in arbitrary payloads are sensitive.
- PagingResponse/PagedResult<T> validate one-based pages, sizes 1–100 and nonnegative counts. Page counts use integer ceiling arithmetic with checked overflow. Payloads are bounded and copied. Empty pages are supported, including pages beyond the last item. The existing int TotalPages contract rejects counts that cannot fit rather than wrapping.
- PagedRequest retains Page and adds a PageNumber alias excluded from JSON. Validate rejects invalid ranges and unsupported offsets. BaseListingRQ retains nullable Page/Size and zero-based GetPage; GetSize is bounded, and ToPagedRequest performs strict validation. Legacy GetSort guards identifier syntax; SQL consumers must use the allow-list overload. SQL values always belong in parameters.
- Existing repository method signatures remain available. Default AddRangeAsync, Remove and GetPagedAsync adapters allow implementations of the previous members to continue working. IReadOnlyRepository has no writes. IAppendOnlyRepository permits additions without update/delete. IRepository/IBaseRepository explicitly opt into policy-controlled mutation/removal and must never be exposed for ledger/audit records. GetPagedAsync composes the existing count/page methods; providers needing a consistent count/page snapshot must override it with an appropriate read transaction. GetAllAsync remains an explicit compatibility operation; list endpoints should use bounded paging and enforce caller access scope.
- IUnitOfWork signatures are unchanged. SaveChangesAsync must not independently commit an enclosing transaction. Repositories participating in one operation must share the same scope/transaction; unsupported and nested transactions must be rejected. No unused concrete repository or unit of work was invented.
- BaseService remains optional and small: response factories and DataAnnotations validation with cancellation. Feature services use constructor injection and propagate tokens to their dependencies.

## Exception and HTTP behavior

Canonical exceptions and all legacy exceptions share one status-aware hierarchy. Expected 4xx messages must be safe for consumers. Details, inner exceptions and stack traces are never included by ExceptionResponseMapper; all 5xx messages are masked. Validation dictionaries are copied. NotFoundException.ForEntity no longer echoes keys.

Legacy mappings: BadRequest 400; Unauthorized 401; Forbidden 403; NotFound 404; Gone 410; Ready 409; InternalServerError 500; Proxy 502; Temporary 503. Ready previously had no defined status and no callers; its compatibility policy is a resource-state conflict (409).

Both controller names now delegate to one HTTP helper implementation in API. The unused empty BaseRepotory was removed. SQL and controller public namespaces are retained, but their assemblies changed: external consumers must add the INFRASTRUCTURE/API reference as appropriate and recompile. Binary compatibility with already compiled external assemblies is not claimed. Pure domain/application code must not reference API.

The ambiguous Success overloads are repaired: Success(data), Success(data, message), and Success(data, message, paging) are supported. Callers supplying paging must also supply the message.

API composes SQL access through scoped dependency injection and registers controllers, safe automatic model-validation responses, exception middleware and empty-error-status envelopes. Existing weather behavior remains. Created helpers use CreatedAtAction, and no-content returns HTTP 204 without a body. Exceptions after a response starts are rethrown. Client-aborted operations produce no response body (499). Logs contain the trace ID and exception type, not exception text or connection strings. Authentication configuration was not changed.

## SQL behavior and dependencies

ISqlDataAccess remains provider-independent in SHARED, with every existing method signature retained. A default-interface transaction overload supplies the cancellation token to callbacks without requiring existing implementers to add a method.

SqlDataAccess keeps parameterized CommandDefinition calls, cancellation, buffered results and async connection/reader disposal. Transactions use one connection and transaction, commit once, and roll back on failure/cancellation. Rollback failure preserves both errors. Calling the same scoped helper to open an independent connection or nested transaction inside its callback throws: issue callback commands directly on the supplied connection and explicitly pass its transaction and cancellation token. Do not resolve a second scope/helper to bypass that boundary.

INFRASTRUCTURE packages: Dapper 2.1.89 and Microsoft.Data.SqlClient 7.1.1 (relocated from SHARED), plus Microsoft.Extensions.Configuration.Abstractions 9.0.11 for the existing configuration constructor.

TEST packages: Microsoft.NET.Test.Sdk 17.14.1, xunit 2.9.3, xunit.runner.visualstudio 3.1.5 (private assets). SHARED has no package or ASP.NET framework dependencies.

APPLICATION now references CORE and SHARED. Existing CORE/INFRASTRUCTURE/API references remain acyclic. TEST references SHARED, INFRASTRUCTURE and API and is included in the solution.

## Validation and limits

Run from the solution root:

```bash
dotnet restore
dotnet build CambodiaDigitalBankingSystem.SHARED/CambodiaDigitalBankingSystem.SHARED.csproj
dotnet build
dotnet test
```

Final verification: restore succeeded; standalone SHARED and full solution builds each succeeded with 0 warnings and 0 errors; dotnet test passed 52 tests with 0 failures and 0 skips on .NET 9. git diff --check reported no whitespace errors (Git emitted its configured LF/CRLF conversion notices).

The 52 unit-test cases cover response factories and adapters, validation and pagination boundaries/overflow/JSON, exception mappings and redaction, metadata/identity mapping, existing repository implementation compatibility, cancellation, HTTP helpers/middleware, and SQL preflight failures without opening a connection.

No existing tests were present. No real database was accessed. SQL Server integration, live commit/rollback and concurrency behavior remain unverified and require a disposable test database. No banking operations, entities, balances, KYC rules or authentication features were implemented. Concrete feature repositories must enforce access, retention and transaction policies.

The repository already tracks bin/obj artifacts and had extensive uncommitted build output changes before this repair. Those files were not reset or removed.
