# ECommerce API

E-commerce REST API built with **.NET 10** and **Clean Architecture**, backed by **PostgreSQL**.

## Features
- Categories, products (with image upload), customers and orders
- Rich domain model: stock control, order lifecycle `Pending → Paid → Shipped → Delivered` / `Cancelled`
- JWT authentication with refresh-token rotation; roles `Admin` and `Customer`
- Customers can only see and manage their own orders
- Request validation with FluentValidation, errors returned as RFC 9110 `ProblemDetails`
- OpenAPI with Swagger UI (`/swagger`) and Scalar (`/scalar`)

## Architecture
```
src/
├── ECommerce.Domain          Entities, enums, business rules (no dependencies)
├── ECommerce.Application     Use cases, DTOs, validators, abstractions (depends on Domain)
├── ECommerce.Infrastructure  EF Core + PostgreSQL, JWT, password hashing, file storage
└── ECommerce.Api             Controllers, auth setup, exception handling, OpenAPI
```

## Getting started

**Requirements:** .NET 10 SDK, PostgreSQL 14+

1. Set the connection string in `src/ECommerce.Api/appsettings.json` (default: `postgres/postgres@localhost:5432/ecommerce_db`).

2. Configure secrets (kept out of source control):
   ```bash
   cd src/ECommerce.Api
   dotnet user-secrets set "Jwt:Secret" "<random string, at least 32 characters>"
   dotnet user-secrets set "Seed:Admin:Email" "admin@ecommerce.local"
   dotnet user-secrets set "Seed:Admin:Password" "<admin password>"
   ```
   The admin account is created automatically on first run in the Development environment.

3. Create the database and run:
   ```bash
   dotnet tool restore
   dotnet ef database update -p src/ECommerce.Infrastructure -s src/ECommerce.Api
   dotnet run --project src/ECommerce.Api
   ```

4. Open http://localhost:5194/swagger, call `POST /api/auth/login`, then click **Authorize** and paste the `accessToken`.

## Main endpoints
| Area | Endpoints |
|---|---|
| Auth | `POST /api/auth/register`, `login`, `refresh`, `logout`, `GET /api/auth/me` |
| Categories | `GET` (public), `POST/PUT/DELETE` (admin) |
| Products | `GET` with search & paging (public), `POST/PUT/DELETE`, `PUT /{id}/image`, stock & activation (admin) |
| Customers | admin only |
| Orders | `POST`, `GET /mine`, `GET /{id}`, `pay`, `cancel` (owner or admin), `ship`, `deliver` (admin) |
