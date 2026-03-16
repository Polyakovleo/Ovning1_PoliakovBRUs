
## BankRUs – sample banking API

Sample .NET 10 application that implements a simplified banking backend: managing customers and their bank accounts, validation, logging, and documentation via OpenAPI/Scalar.

The solution follows Clean Architecture / DDD ideas and is split into layers:

- `BankRUs.Domain` – domain entities (`Customer`, `BankAccount`).
- `BankRUs.Application` – use cases (CQS), repository/service interfaces.
- `BankRUs.Intrastructure` – EF Core `DbContext`, repositories, migrations, services.
- `BankRUs.Api` – ASP.NET Core Web API (presentation layer).

---

## Tech stack

- .NET 10 (`net10.0`)
- ASP.NET Core Web API
- Entity Framework Core (SQL Server / LocalDB)
- Serilog (console + file)
- OpenAPI + Scalar

---

## How to run

### Prerequisites

- .NET SDK 10.x
- SQL Server LocalDB (default is `MSSQLLocalDB`)

### Configuration

Main settings live in `BankRUs.Api/appsettings.json`:

- `ConnectionStrings:DefaultConnection` – database connection string.
- `Email` – SMTP settings (for homework, `localhost:25` is enough).
- `QueryParams:MaxPageSize` – max allowed `pageSize` for pagination.

### Database migrations

From repository root:

```bash
dotnet ef database update --project BankRUs.Intrastructure --startup-project BankRUs.Api
```

### Run the API

From repository root:

```bash
dotnet run --project BankRUs.Api
```

Defaults (from `launchSettings.json`):

- HTTP: `http://localhost:5201`
- HTTPS: `https://localhost:8000`

---

## Authentication & roles (for testing)

For simplicity, header‑based auth is used (`HeaderAuthenticationHandler`):

- `X-UserId` – customer `Guid` (used as `NameIdentifier`).
- `X-Roles` or `X-Role` – comma‑separated roles (`Customer`, `CustomerService`, etc.).

Examples:

- **CustomerService**:

  ```http
  X-UserId: 00000000-0000-0000-0000-000000000001
  X-Roles: CustomerService
  ```

- **Customer**:

  ```http
  X-UserId: {real customer Guid}
  X-Roles: Customer
  ```

---

## Main endpoints

### Customers (`CustomersController`)

#### Create customer with first account

- `POST /api/customers`
- Role: none required.
- Body:

```json
{
  "name": "Jane Doe",
  "email": "jane@example.com",
  "personalNumber": "19900101-1234",
  "initialBalance": 1000.0
}
```

Response `201 Created` with `customerId`, `accountId`, `accountNumber`.

#### Create additional account for existing customer

- `POST /api/customers/{customerId}/accounts`
- Body:

```json
{
  "initialBalance": 500.0
}
```

#### Get paged list of customers (with search)

- `GET /api/customers`
- Role: `CustomerService`
- Query parameters:
  - `page` – page number (1…)
  - `pageSize` – page size (1…MaxPageSize)
  - `ssn` – personal number prefix filter (matches beginning of `personalNumber`).

Response:

```json
{
  "data": [
    {
      "id": "...",
      "name": "...",
      "email": "...",
      "personalNumber": "...",
      "accountsCount": 1
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 8,
  "totalPages": 1
}
```

#### Get single customer (with all accounts)

- `GET /api/customers/{id}`
- Role: `CustomerService`

Response:

```json
{
  "id": "...",
  "firstName": "Jane Doe",
  "email": "jane@example.com",
  "personalNumber": "19900101-1234",
  "bank-accounts": [
    {
      "id": "...",
      "bank-account-number": "123...",
      "balance": 1000.0
    }
  ]
}
```

#### Update current customer profile

- `PATCH /api/customers`
- Role: `Customer`
- Auth: `X-UserId = customerId`
- Body (any combination of fields):

```json
{
  "firstName": "New Name",
  "email": "new@example.com",
  "personalNumber": "19900101-9999"
}
```

Response: `204 No Content` (validation errors -> `400` with `ProblemDetails`).

### Accounts (`BankAccountsController`)

#### Open account for customer

- `POST /api/bank-accounts`
- Body:

```json
{
  "userId": "{customerId}",
  "initialBalance": 0.0
}
```

Response `200 OK` with `accountId`, `accountNumber`, `balance`.

### Current user (`MeController`)

#### Delete current customer

- `DELETE /api/me`
- Role: `Customer`
- Auth: `X-UserId = customerId`
- Deletes the customer and all related bank accounts.

Response: `204 No Content` (if customer does not exist – `404 Not Found`).

---

## Logging

**Serilog** is used:

- Logs are written:
  - to console;
  - to files `Logs/bankrus-YYYYMMDD.log` (one file per day, 7 days retained).
- Logged events:
  - business events:
    - customer and account creation;
    - additional account opening;
    - customer profile updates;
    - customer deletion;
  - unhandled exceptions in the global exception handler (`UseExceptionHandler`).

Example business log entry:

```text
Information Created additional account {AccountId} ({AccountNumber}) for existing customer {CustomerId} with initial balance {InitialBalance}
```

---

## API documentation (OpenAPI / Scalar)

When running in `Development`:

- OpenAPI JSON: `https://localhost:8000/openapi/v1.json`
- Scalar UI: `https://localhost:8000/scalar`

Attributes used in controllers:

- `[Consumes]`, `[Produces]`
- `[ProducesResponseType]`, `[ProducesErrorResponseType]`
- `[EndpointSummary]`, `[EndpointDescription]`

These improve schemas, status codes and descriptions in Scalar/Swagger.

---

## Repository structure (short)

- `BankRUs.Domain` – domain entities.
- `BankRUs.Application`
  - `Interfaces` – ports (repositories, services).
  - `UseCases` – use cases (`CreateCustomerWithAccount`, `GetCustomersPage`, `OpenBankAccount`, `UpdateCustomerDetails`, `CloseCustomerAccount`, etc.).
  - `Common` – shared types (`Exceptions`, `Paging`).
- `BankRUs.Intrastructure`
  - `Persistence` – `BankDbContext`, EF Core migrations.
  - `Repositories` – repository implementations.
  - `Email`, `Services` – infrastructure services.
- `BankRUs.Api`
  - `Controllers` – Web API controllers.
  - `Auth` – header‑based authentication.
  - `Program.cs` – DI setup, Serilog, OpenAPI/Scalar, exception handling.
