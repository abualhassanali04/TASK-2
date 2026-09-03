# Product Catalog API

A Web API for managing a product catalog — categories and products — built with ASP.NET Core and Entity Framework Core, backed by a real SQL Server database. This is the Web API rebuild of the Task 01 console catalog project, using the same domain (products, categories, reports) but exposed as HTTP endpoints with persistent storage.

## Tech Stack

- **ASP.NET Core Web API** (.NET 10)
- **Entity Framework Core** (Code-First, SQL Server)
- **Serilog** — logging to console and daily rolling file (`Logs/`)

## Project Structure

```
ProductCatalogApi/
├── Program.cs
├── Controllers/
│   ├── CategoriesController.cs
│   └── ProductsController.cs
├── Models/
│   ├── Product.cs
│   └── Category.cs
├── DTOs/
│   ├── ProductDtos.cs
│   └── CategoryDtos.cs
├── Data/
│   └── AppDbContext.cs
├── Migrations/
├── ProductCatalogApi.http
└── README.md
```

## How to Run

### 1. Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (comes with Visual Studio's ASP.NET workload) — or SQLite if on Mac/Linux
- `dotnet-ef` global tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2. Configure the connection string

Open `appsettings.json` and set `ConnectionStrings:DefaultConnection`. For SQL Server LocalDB (Windows, default setup used in this project):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductCatalogDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

If you're on Mac/Linux and using SQLite instead, swap `UseSqlServer(...)` for `UseSqlite(...)` in `Program.cs` and adjust the connection string accordingly — everything else stays the same.

### 3. Apply migrations (creates the database and tables)

```bash
dotnet ef database update
```

This creates the `ProductCatalogDb` database with `Products` and `Categories` tables, including the foreign key relationship between them.

### 4. Run the project

```bash
dotnet run
```

The API will start on the address shown in the console (e.g. `http://localhost:5212`).

## Testing the Endpoints

Use the included `ProductCatalogApi.http` file (works directly in VS Code with the REST Client extension, or in Visual Studio / Rider natively). It covers every endpoint, including failure cases (400 and 404 responses).

Alternatively, use Postman or any HTTP client against the base URL shown when the project runs.

## Endpoints

### Categories

| Method | Route | Description |
|---|---|---|
| GET | `/api/categories` | Get all categories |
| GET | `/api/categories/{id}` | Get one category |
| POST | `/api/categories` | Create a category |
| PUT | `/api/categories/{id}` | Update a category |
| DELETE | `/api/categories/{id}` | Delete a category |
| GET | `/api/categories/{id}/products` | Get all products under a category |

### Products

| Method | Route | Description |
|---|---|---|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get one product |
| POST | `/api/products` | Create a product |
| PUT | `/api/products/{id}` | Update a product |
| DELETE | `/api/products/{id}` | Delete a product |

## Design Notes

- **DTOs everywhere:** Controllers never expose EF entities directly. Requests use `CreateProductDto`/`UpdateProductDto` (no `Id`, no `CreatedAt` — the server sets those), and responses use `ProductDto`/`CategoryDto`, which include a flattened `CategoryName` instead of a full navigation object. This avoids leaking the database shape and avoids circular-reference serialization issues between `Product` and `Category`.
- **Real foreign key relationship:** `Product.CategoryId` is a real FK constraint enforced by the database (`FK_Products_Categories_CategoryId`), not just an in-memory number. Adding a product with a non-existent `CategoryId` is rejected with `400 Bad Request` before it ever reaches the database.
- **Cascade delete:** Deleting a category deletes its products automatically (`DeleteBehavior.Cascade`), configured explicitly in `AppDbContext.OnModelCreating` — chosen deliberately over restricting deletion, since a product with no category has no independent meaning in this simple catalog.
- **Validation via Data Annotations:** `[Required]`, `[MaxLength]`, `[MinLength]`, `[Range]` on the DTOs reject invalid input (empty names, negative prices, negative stock, short names/descriptions) with a `400` and a clear message before any business logic runs.
- **Fully async:** Every database call (`ToListAsync`, `FindAsync`, `AnyAsync`, `SaveChangesAsync`) is awaited — no blocking calls.
- **Logging:** Both controllers use `ILogger<T>`, logging successful creates/updates/deletes at `Information` level and failed lookups/validation at `Warning` level. Logs are written to the console and to a daily rolling file under `Logs/` (gitignored) via Serilog.
- **Price precision:** `Product.Price` uses `HasPrecision(18, 2)` in `OnModelCreating` to avoid EF Core's default-precision truncation warning.

## Migrations

If you change a model (`Product.cs`, `Category.cs`) or the relationship configuration in `AppDbContext`, create a new migration rather than editing existing migration files by hand:

```bash
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```