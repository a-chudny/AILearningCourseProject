# Volunteer Portal API

ASP.NET Core 10 Web API for the Volunteer Event Portal.

## Prerequisites

- .NET 10 SDK
- PostgreSQL 15+
- Visual Studio 2022+ or VS Code

## Getting Started

### 1. Open in Visual Studio

Open `VolunteerPortal.sln` in Visual Studio.

### 2. Configure Database Connection

Update the connection string in `appsettings.Development.json` or use User Secrets:

```bash
cd src/VolunteerPortal.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=volunteer_portal_dev;Username=postgres;Password=your_password"
```

### 3. Create Database

Ensure PostgreSQL is running, then create the database:

```sql
CREATE DATABASE volunteer_portal_dev;
```

### 4. Run Migrations

```bash
cd src/VolunteerPortal.API
dotnet ef database update
```

### 5. Run the API

```bash
dotnet run --project src/VolunteerPortal.API
```

Or press F5 in Visual Studio.

### 6. Access Swagger UI

Navigate to: https://localhost:5001/swagger

## Project Structure

```
backend/
├── src/
│   └── VolunteerPortal.API/
│       ├── Controllers/        # API Controllers
│       ├── Data/               # DbContext and migrations
│       ├── Models/             # Entities, DTOs, Enums
│       ├── Services/           # Business logic
│       └── Program.cs          # Application entry point
└── tests/
    └── VolunteerPortal.Tests/  # Unit and integration tests
```

## Running Tests

```bash
dotnet test
```

## Health Check

The API exposes a health check endpoint:

- `GET /api/health` - Returns API health status
- `GET /api/health` (built-in) - Returns health check with database status

## Configuration

| Setting | Description | Default |
|---------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | localhost |
| `Cors:AllowedOrigins` | Allowed CORS origins | http://localhost:5173 |
| `Jwt:Issuer` | JWT token issuer | VolunteerPortal |
| `Jwt:Audience` | JWT token audience | VolunteerPortalUsers |
| `Jwt:ExpirationInHours` | Token expiration time | 24 |
