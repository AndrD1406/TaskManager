# TaskManager API (Local Run Guide)

This guide explains how to run the application locally.

Solution projects:
- TaskManager.DataAccess
- TaskManager.BusinessLogic
- TaskManager.WebApi

## Prerequisites
- .NET 8 SDK
- SQL Server instance you can reach locally
  - On Windows you can use SQL Server LocalDB
  - On macOS/Linux use a full SQL Server instance (LocalDB is not supported)
- Optional: Visual Studio 2022 or VS Code

## Configure the database (connection string)
By default, the app is set up for LocalDB on Windows:
"ConnectionStrings": {
  "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TaskManager;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"
}

If you are NOT on Windows or do not have LocalDB:
- Replace the connection string in TaskManager.WebApi/appsettings.Development.json with one that points to a reachable SQL Server.

## Apply Entity Framework Core migrations
Install EF tools if you do not have them:
dotnet tool install -g dotnet-ef

From the solution root, run:
dotnet ef database update --project TaskManager.DataAccess --startup-project TaskManager

Notes:
- --project points to the assembly that contains the Migrations (DataAccess).
- --startup-project points to the entrypoint that holds Program.cs (WebApi or the folder hosting it), so configuration is loaded correctly.

## Run the API
Option A: command line
dotnet run --project TaskManager/TaskManager.WebApi

Option B: Visual Studio
- Set TaskManager.WebApi as Startup project
- Press F5 or Ctrl+F5

The console output will show the listening URLs.
- https://localhost:7108

Open /swagger on that base URL (e.g., https://localhost:7108/swagger).

## JWT configuration
Edit TaskManager.WebApi/appsettings.Development.json:
"Jwt": {
  "Issuer": "https://localhost:7108",
  "Audience": "https://localhost:4200",
  "Key": "bW9yZXJhbmRvbXNhbXBsZXJhbmRvbXNhbXBsZXJhbmRvbXM=",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationMinutes": 60
}

Ensure Issuer and Audience match your environment needs. Key must remain a valid Base64 string if the code expects Base64.

## Using the API
Register:
POST /users/register
Body:
{
  "username": "demo",
  "email": "demo@example.com",
  "password": "P@ssw0rd!"
}

Login:
POST /users/login
Body:
{
  "usernameOrEmail": "demo",
  "password": "P@ssw0rd!"
}
Copy the returned access token and send it in the Authorization header:
Authorization: Bearer <token>

Tasks endpoints (require Authorization header):
- POST /tasks
- GET /tasks
- GET /tasks/{id}
- PUT /tasks/{id}
- DELETE /tasks/{id}
Filtering: status, dueDate, priority (query parameters)
Sorting: dueDate, priority
Pagination: page, pageSize

## Architecture
- Repository pattern in TaskManager.DataAccess (IEntityRepository<TEntity, TKey>, EntityRepository<TEntity, TKey>)
- Business services in TaskManager.BusinessLogic (interfaces and implementations)
- Web API wiring in TaskManager.WebApi (DI, authentication, controllers, Swagger)
- AutoMapper for DTO mapping

## Authentication and security
- Passwords are stored hashed
- JWT authentication using Microsoft.AspNetCore.Authentication.JwtBearer
- Configure Jwt section in appsettings for your environment
- Protect all task endpoints with [Authorize]

## Tests
Test framework: NUnit
Mocking: Moq
Data generation: Bogus

Run all tests:
dotnet test

Note for non‑Windows environments/CI: LocalDB is Windows-only. If your tests use LocalDB in their connection string, they will fail on Linux/macOS. Either point tests to a reachable SQL Server instance or modify test setup to use an in-memory provider (e.g., SQLite) for CI.

## Troubleshooting
- 500 on /users/register or /tasks: check that migrations were applied and the configured database exists.
- "LocalDB is not supported on this platform": switch your connection string away from LocalDB to a real SQL Server instance.
- Swagger not showing in non-development: enable it by configuration or remove the environment check in Program.cs if you need it always on.
