# Database connection

The development Compose stack runs SQL Server 2019 Express and persists its data in the `sql_data` Docker volume.

## Start the development stack

From the repository root, copy `Deployment/.env.example` to `Deployment/.env`, set a strong `DB_SA_PASSWORD`, then run:

```powershell
docker compose --env-file Deployment/.env -f Deployment/compose.dev.yaml up --build
```

## Connect from a database client on the host

- Server/host: `localhost,1434`
- Database: `CambodiaDigitalBanking`
- Authentication: SQL Server authentication
- User: `sa`
- Password: value of `DB_SA_PASSWORD` in `Deployment/.env`
- Encrypt: enabled
- Trust server certificate: enabled (local development only)

ADO.NET connection string:

```text
Server=localhost,1434;Database=CambodiaDigitalBanking;User Id=sa;Password=<DB_SA_PASSWORD>;Encrypt=True;TrustServerCertificate=True;
```

From the API container, use host `sqlserver,1433` instead of `localhost,1434`.

Production uses the same database service and persistent volume, but does not publish the SQL Server port to the host. Set `DB_SA_PASSWORD` in the environment or pass `--env-file Deployment/.env` when running the production Compose file.

Compose runs a one-shot initializer after SQL Server becomes healthy. It creates `CambodiaDigitalBanking` if it does not already exist, including when reusing an existing data volume. The database name is currently fixed in both Compose files; `DB_NAME` does not configure it.

The API registers `ISqlDataAccess` with the Dapper/SQL Server implementation in INFRASTRUCTURE. That implementation reads `ConnectionStrings:DefaultConnection` and opens connections on demand. The current weather endpoint does not use it. There is no DbContext, migration, or table initialization in the application.

## Production configuration review

The supplied production Compose file is a deployment scaffold: the API uses `sa`, and its connection string sets `TrustServerCertificate=True`. Before using it with real banking data, provision a restricted application login and a trusted SQL Server certificate, then supply the application connection string through deployment secrets. Keep the administrator credential available only to database administration and initialization. The current initializer creates the database only; it does not provision an application login or certificate.

For a local API launched from Rider or Git Bash, set `ConnectionStrings__DefaultConnection` in the local run configuration or environment. Do not commit a password to appsettings.json. The Compose environment settings apply to containers only.
