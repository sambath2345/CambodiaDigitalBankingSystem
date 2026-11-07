# Project code and configuration review

Reviewed the current working tree, including pre-existing uncommitted SHARED repairs. All seven projects target .NET 9 with nullable reference types enabled. global.json selects SDK 9.0.313. No application database commands or migrations were executed during this review.

## Findings requiring follow-up

0. **High: an environment credential file is already tracked.** git ls-files confirms Deployment/.env is tracked. Its contents were not displayed or changed during this review. Treat any committed credentials as exposed and rotate them, then remove the file from the index in a reviewed cleanup. The example also contained a concrete password; this review replaced it with an explicit placeholder. Ignore rules do not protect files that are already tracked or remove secrets from Git history.
1. **High: production database privileges and certificate validation.** Deployment/compose.yaml supplies the SQL administrator account to the API and disables certificate verification with TrustServerCertificate=True. Provision a restricted application login and trusted certificate before a real deployment. No credentials or database permissions were changed in this review.
2. **High before protected endpoints are introduced: authentication is absent.** API/Program.cs configures neither an authentication scheme nor an authorization policy. The only current endpoint is the public weather template. Authentication and authorization must be implemented and tested with the future feature endpoints.
3. **Medium: SQL transaction integration coverage is missing.** Existing tests exercise preflight cancellation and missing configuration, but never open SQL Server connections. Commit, rollback, nested transaction rejection, callback cancellation, and concurrent execution need integration tests against a disposable database. Passing unit tests does not validate these database behaviors.
4. **Medium: HTTP integration coverage is limited.** Tests invoke middleware and controller helpers directly. They do not exercise the complete hosted pipeline, model binding, CreatedAtAction URL generation, or authentication challenges.
5. **Medium: HTTP deployment needs an explicit TLS strategy.** Compose exposes HTTP on port 8080 while Program.cs enables HTTPS redirection. Configure TLS termination and trusted proxy handling, or a container HTTPS listener, for deployment; the current files do not define them.
6. **Medium: tracked generated artifacts.** Git already tracks bin/obj files and the working tree has extensive changes to them. The new root .gitignore prevents additional untracked build output and local environment files from being added accidentally. It cannot untrack existing files. A separate reviewed index cleanup is still required.
7. **Low: compatibility names conceal assembly moves.** HTTP controllers retain SHARED namespaces but compile into API; SqlDataAccess retains its SHARED namespace but compiles into INFRASTRUCTURE. In-repository references build, but external consumers require the new assembly references and recompilation. Legacy entity, response and exception classes remain compatibility adapters.

## Architecture and shared behavior

- CORE references SHARED. APPLICATION references CORE and SHARED. INFRASTRUCTURE references APPLICATION, CORE and SHARED. API composes those layers. No circular references were found.
- SHARED has no Dapper, SQL Server or ASP.NET Core dependency. SQL packages belong to INFRASTRUCTURE; middleware and controller implementations belong to API.
- Generic audit metadata, bounded pagination, response envelopes, cancellation-aware repository contracts and safe exception mappings have working implementations and unit tests. Legacy names delegate to canonical implementations where applicable.
- Mutable repository interfaces expose deletion by explicit opt-in; ledger and audit consumers must use the read-only or append-only contracts. No banking entities or use cases exist yet to verify those policies against.
- Generic response envelopes cannot redact arbitrary DTO payloads. Expected 4xx exception messages and validation messages must be safe for callers. Unexpected and 5xx exception text is masked by the mapper.
- SQL query values use Dapper parameters. The helper accepts caller-provided query text, so feature callers must continue to parameterize values and allow-list dynamic identifiers.

## Configuration changes made during this review

- Added root .gitignore for build/test output, local IDE state and environment secrets.
- Excluded .build-temp and test output from the Docker build context.
- Corrected Deployment/DATABASE.md: the database name is fixed, SQL access is registered, and local API launches require their own connection configuration. Added the production database configuration requirements above.
- Replaced the concrete password in Deployment/.env.example with a placeholder and removed its unused DB_NAME setting.
- Preserved existing source edits, package versions, project references and database state.

## Validation

- dotnet restore: passed.
- dotnet build --no-restore: passed, 0 warnings and 0 errors.
- dotnet test --no-build --no-restore: 52 passed, 0 failed, 0 skipped.

These results describe the current checkout, including repairs that existed before this review. Docker deployment and live SQL behavior require separate validation. Build success is not a production readiness assessment.
