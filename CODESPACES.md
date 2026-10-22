# Running ProductCatalogApi in GitHub Codespaces

`appsettings.json` points at LocalDB, which only exists on Windows. Inside the
Codespace, SQL Server runs in Docker and the connection string comes from `.env`.

## First time
1. On GitHub: **Code → Codespaces → Create codespace on main**. The first build takes a few minutes.
2. Start SQL Server and wait until it reports `healthy`:
   ```bash
   docker compose up -d sql
   docker compose ps
   ```
3. Load the connection string and create the database:
   ```bash
   set -a; source .env; set +a
   dotnet ef database update
   ```
4. Run the API:
   ```bash
   dotnet run --launch-profile http
   ```
   Open the forwarded port 5212 from the **Ports** tab, or use `ProductCatalogApi.http`.

## Connect to the database
Open the **SQL Server** extension (left sidebar) → Add connection:
server `localhost,1433`, auth **SQL Login**, user `sa`, password from `.env`,
tick **Trust server certificate**.

## Every later session
```bash
docker compose up -d sql
set -a; source .env; set +a
dotnet run --launch-profile http
```

## Tests
Testcontainers starts its own SQL Server container, so `dotnet test` works here as-is.

## Saving your free hours
- Stop the Codespace when you finish: **github.com/codespaces → … → Stop codespace**.
- Lower the idle timeout in GitHub Settings → Codespaces (default 30 min).
- Delete Codespaces you no longer need; storage counts against the quota.

## If the sql container keeps stopping
Run `docker logs sql`. The usual cause is a weak `MSSQL_SA_PASSWORD`.
