# Run, debug and call the API

## Start from Git Bash

From the solution root:

```bash
dotnet restore
dotnet build
dotnet run --project CambodiaDigitalBankingSystem.API --launch-profile http
```

The http profile selects Development and http://localhost:5211. Keep this terminal running; use another terminal for requests. Press Ctrl+C to stop the API. If the port is already occupied, stop the existing instance or select another port and update the request URL.

The http profile is useful for this local demonstration. Production still needs the TLS/proxy setup described in ARCHITECTURE_REVIEW.md. The weather endpoint does not access SQL Server, so no database credential is needed to exercise these examples.

## Debug in Rider

1. Open CambodiaDigitalBankingSystem.sln.
2. Select or create a .NET launch configuration for CambodiaDigitalBankingSystem.API using the http launch profile in Properties/launchSettings.json.
3. Set a breakpoint inside the /weatherforecast handler in API/Program.cs, on the forecast creation statement.
4. Start Debug, then request /weatherforecast using curl or the .http file below.
5. When the breakpoint stops, inspect locals and the call stack. Step Over advances execution; Resume lets the request finish and returns its response to the client.

Do not leave a previous dotnet run process on the same port when starting Rider Debug. Requests will wait while execution is paused. HttpContext.TraceIdentifier identifies the request; generic 404 responses contain the same trace identifier in the body.

Open API/CambodiaDigitalBankingSystem.API.http and run individual requests through Rider's HTTP client. The file contains GET weather, missing customer route, unsupported weather method, and Development OpenAPI examples.

## Requests and observed responses

```bash
curl -i http://localhost:5211/weatherforecast
```

Observed status: 200 OK. The body is a five-item array. Dates and random temperatures change; this is one item from the observed response:

```json
{"date":"2026-11-08","temperatureC":19,"summary":"Sweltering","temperatureF":66}
```

This template endpoint returns raw JSON rather than the ApiResponse success envelope. The base-controller helpers exist, but no feature controller currently invokes them.

```bash
curl -i http://localhost:5211/api/customers
```

Observed status: 404 Not Found:

```json
{
  "success": false,
  "message": "The requested resource was not found.",
  "data": null,
  "errors": null,
  "traceId": "0HNPSJMUNLQFS:00000001",
  "paging": null
}
```

The trace identifier changes for each request. This response comes from status-code middleware in Program.cs, not from a customer controller. To debug it, place a breakpoint inside the UseStatusCodePages callback. Missing routes do not throw, so they do not enter the exception middleware catch block.

```bash
curl -i -X POST http://localhost:5211/weatherforecast
```

The GET-only endpoint should return 405 Method Not Allowed. The current fallback message is the generic unexpected-error message because Program.cs has no dedicated 405 message. The response status remains 405.

```bash
curl -i http://localhost:5211/openapi/v1.json
```

OpenAPI is mapped only in Development. A launch with --no-launch-profile defaults to the ambient environment (Production when unset), so that launch may return 404 for OpenAPI.

## Verify base features

```bash
dotnet test --filter FullyQualifiedName~HttpAndSqlTests
dotnet test --filter FullyQualifiedName~BaseRepositoryTests
```

In Rider, debug an individual test and set breakpoints in API/Base/Controller/BaseController.cs or INFRASTRUCTURE/Base/Repository/BaseRepository.cs. These tests demonstrate pagination helpers, safe error responses and repository behavior without a real banking database. No public demonstration error endpoint or banking operation is needed.
