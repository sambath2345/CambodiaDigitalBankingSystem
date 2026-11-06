# Database connection

The development Compose stack runs SQL Server 2019 Express and persists its data in the `sql_data` Docker volume.

## Start the development stack

From the repository root, copy `Deployment/.env.example` to `Deployment/.env`, set a strong `DB_SA_PASSWORD`, then run:

```powershell
docker compose --env-file Deployment/.env -f Deployment/compose.dev.yaml up --build
```

## Connect from a database client on the host

- Server/host: `localhost,1434`
- Database: value of `DB_NAME` (default `CambodiaDigitalBanking`)
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

Compose runs a one-shot initializer after SQL Server becomes healthy. It creates `CambodiaDigitalBanking` if it does not already exist, including when reusing an existing data volume. The current API project does not include a SQL Server driver, DbContext, or migrations, so the API receives the connection string but does not yet open database connections or create tables.
