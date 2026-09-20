# Product Catalog API

A Web API for managing a product catalog — categories and products — built with ASP.NET Core, Entity Framework Core, and a layered architecture with JWT authentication. This is the Task 03 evolution of the project: business logic now lives in a service layer behind interfaces, endpoints are protected with JWT bearer authentication and role-based authorization, errors are handled centrally, and the service layer is covered by automated tests.

## Tech Stack

- **ASP.NET Core Web API** (.NET 10)
- **Entity Framework Core** (Code-First, SQL Server)
- **JWT Bearer Authentication** with refresh tokens
- **Serilog** — structured logging to console and daily rolling file (`Logs/`)
- **xUnit** — unit and integration tests (EF Core InMemory provider)

## Project Structure

```
ProductCatalogApi/
├── Program.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── CategoriesController.cs
│   ├── ProductsController.cs
│   └── ReportsController.cs
├── Interfaces/
│   ├── IAuthService.cs
│   ├── ICategoryService.cs
│   ├── IProductService.cs
│   └── IReportService.cs
├── Services/
│   ├── AuthService.cs
│   ├── CategoryService.cs
│   ├── ProductService.cs
│   ├── ReportService.cs
│   └── ServiceResult.cs
├── Models/
│   ├── Product.cs
│   ├── Category.cs
│   ├── User.cs
│   └── RefreshToken.cs
├── DTOs/
│   ├── AuthDtos.cs
│   ├── CategoryDtos.cs
│   ├── PageResultDtos.cs
│   └── ProductDtos.cs
│   └── ReportDtos.cs
├── Data/
│   └── AppDbContext.cs
├── Extensions/
│   └── MappingExtensions.cs
├── Middleware/
│   └── GlobalExceptionHandler.cs
├── Migrations/
├── ProductCatalogApi.Tests/
├── ProductCatalogApi.http
└── README.md
```

## Architecture

- **Controllers** only handle HTTP: read the request, call a service, translate the result into a status code. No `DbContext` is injected into any controller.
- **Services** hold all business logic and database access, behind interfaces (`IProductService`, `ICategoryService`, `IReportService`, `IAuthService`) registered via dependency injection.
- **`ServiceResult<T>`** is how services communicate outcomes back to controllers — `Success`/`Data` on success, `ErrorMessage`/`ErrorType` (`NotFound`, `ValidationError`, `Conflict`) on failure. Services never return HTTP-specific types; the controller maps `ErrorType` to the right status code (404, 400, etc.).
- **Mapping** between entities and DTOs is centralized in `Extensions/MappingExtensions.cs` (`ToDto()`, `ToEntity()`, `UpdateEntity()`) — no duplicated conversion logic across actions.

## How to Run

### 1. Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (Windows) or SQLite (Mac/Linux)
- `dotnet-ef` global tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### 2. Configure the connection string

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductCatalogDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

On Mac/Linux, swap `UseSqlServer(...)` for `UseSqlite(...)` in `Program.cs` and adjust the connection string.

### 3. Configure JWT secrets (required — the app will not start without these)

The JWT signing key is **never** stored in `appsettings.json` or committed to git. It's stored locally with .NET User Secrets:

```bash
cd ProductCatalogApi
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "a-long-random-secret-at-least-32-characters-here"
dotnet user-secrets set "Jwt:Issuer" "ProductCatalogApi"
dotnet user-secrets set "Jwt:Audience" "ProductCatalogApiUsers"
```

Verify they're set:

```bash
dotnet user-secrets list
```

### 4. Apply migrations

```bash
dotnet ef database update
```

This creates `Products`, `Categories`, `Users`, and `RefreshTokens` tables, including the `Products`→`Categories` foreign key and the unique index on `Username`.

### 5. Run the project

```bash
dotnet run
```

The API starts on the address shown in the console (e.g. `http://localhost:5212`).

## Authentication Flow

All `GET` endpoints on Products and Categories are open to anyone. All `POST`/`PUT` require a valid JWT (any logged-in user). All `DELETE` endpoints, and all Reports endpoints, additionally require the `Admin` role.

### 1. Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "myuser",
  "password": "MySecurePass123"
}
```

New users are created with the `User` role by default.

### 2. Promote a user to Admin (manual, for testing)

There's no self-service "become Admin" endpoint by design. To test Admin-only routes, promote a user directly in the database:

```sql
UPDATE Users SET Role = 'Admin' WHERE Username = 'myuser';
```

### 3. Log in

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "myuser",
  "password": "MySecurePass123"
}
```

Response:

```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "buX3YlR+Qq5e4c..."
}
```

- **Access token**: a JWT (HS256), valid for **1 hour**. Contains the user's Id, Username, and Role as claims — nothing sensitive (JWTs are signed, not encrypted; anyone can decode the payload at jwt.io).
- **Refresh token**: a random opaque string, valid for **7 days**, stored server-side. Used only to obtain a new access token without re-entering credentials.

### 4. Call a protected endpoint

```http
POST /api/products
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Laptop",
  "price": 999.99,
  "stock": 10,
  "categoryId": 1
}
```

Without a token: `401 Unauthorized`. With a valid token but the wrong role on an Admin-only route: `403 Forbidden`.

### 5. Refresh the access token

Once the access token expires, use the refresh token instead of logging in again:

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "buX3YlR+Qq5e4c..."
}
```

Returns a new access token **and** a new refresh token; the old refresh token is revoked (rotation) and cannot be reused.

## Endpoints

### Auth

| Method | Route | Auth required |
|---|---|---|
| POST | `/api/auth/register` | No |
| POST | `/api/auth/login` | No |
| POST | `/api/auth/refresh` | No |

### Categories

| Method | Route | Auth required |
|---|---|---|
| GET | `/api/categories` | No |
| GET | `/api/categories/{id}` | No |
| GET | `/api/categories/{id}/products` | No |
| POST | `/api/categories` | Yes |
| PUT | `/api/categories/{id}` | Yes |
| DELETE | `/api/categories/{id}` | Yes (Admin) |

### Products

| Method | Route | Auth required |
|---|---|---|
| GET | `/api/products` (supports `page`, `pageSize`, `search`, `categoryId`, `minPrice`) | No |
| GET | `/api/products/{id}` | No |
| POST | `/api/products` | Yes |
| PUT | `/api/products/{id}` | Yes |
| DELETE | `/api/products/{id}` | Yes (Admin) |

### Reports

| Method | Route | Auth required |
|---|---|---|
| GET | `/api/reports/inventory-value` | Yes (Admin) |
| GET | `/api/reports/most-expensive-product` | Yes (Admin) |
| GET | `/api/reports/out-of-stock` | Yes (Admin) |

## Testing the API

Use `ProductCatalogApi.http` (VS Code REST Client, Visual Studio, or Rider). It covers the full auth flow (register → login → use token on a protected endpoint), success and failure cases (400/401/403/404), and chains the access token automatically from the login response — no manual copy-pasting needed.

## Error Handling

Unhandled exceptions are caught by a single central handler (`Middleware/GlobalExceptionHandler.cs`, implementing `IExceptionHandler`) and returned as [RFC 7807](https://tools.ietf.org/html/rfc7807) `ProblemDetails` JSON — a consistent shape for every error, regardless of cause:

```json
{
  "status": 500,
  "title": "An unexpected error occurred",
  "detail": "Something went wrong on our end. Please try again later.",
  "instance": "/api/products"
}
```

Full exception details (including the stack trace) are logged server-side via `ILogger`; nothing internal is ever returned to the client.

## Automated Tests

```bash
cd ProductCatalogApi.Tests
dotnet test
```

The test project uses the EF Core **InMemory** provider (not a real database — it doesn't enforce foreign keys or most constraints, but it's enough to validate service-layer logic in isolation) and covers:

- `ProductService`: create (valid/invalid category), update (valid/invalid category, non-existent product), delete (existing/non-existent), get by id (existing/non-existent), get all with pagination
- `AuthService`: login with correct credentials
- Integration tests exercising the real HTTP pipeline via `WebApplicationFactory`


## Migrations

When you change a model or the relationship configuration in `AppDbContext`, create a new migration rather than editing an existing one by hand:

```bash
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```

## Security Notes

- Passwords are never stored or logged in plaintext — they're hashed with ASP.NET Core's `PasswordHasher<User>` (PBKDF2, salted, ~100k iterations).
- The JWT signing key lives only in User Secrets locally (or environment variables / a secrets manager in production) — never in source control.
- JWTs are signed, not encrypted: don't rely on the payload being secret. Only non-sensitive claims (Id, Username, Role) are included.
- Refresh tokens are single-use — each refresh call revokes the old token and issues a new one.