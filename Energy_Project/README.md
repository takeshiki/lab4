# Energy Project

ASP.NET Core 9 Web API with xUnit/Moq tests for the Smart Home energy services. Runs easily via Docker Compose.

## Prerequisites
- Docker and Docker Compose (recommended)
- Optional: .NET 9 SDK if you want to run without containers

## Run the API (Docker)
1. From `Energy_Project/` run:
   ```bash
   docker compose up --build api
   ```
2. Open http://localhost:8080/ to see the health message. Controller routes are served on the same base URL.

## Run the tests (Docker)
1. From `Energy_Project/` run:
   ```bash
   docker compose run --rm tests
   ```
2. Test results are written to `TestResults/test-results.trx`.

## Run locally without Docker (optional)
```bash
dotnet restore Energy_Project.sln
dotnet run --project Energy_Project/Energy_Project.csproj --urls http://localhost:8080
dotnet test Energy_Project.sln
```

## Notes
- `.dockerignore` and the multi-stage `Dockerfile` keep images small and separate API vs test targets.
- The root endpoint `/` returns a simple “Energy Project API is running” response for quick health checks.
