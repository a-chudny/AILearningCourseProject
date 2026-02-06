# Volunteer Portal — Backend API

ASP.NET Core 10 REST API with PostgreSQL, JWT authentication, and Swagger documentation.

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 10 | Web API framework |
| PostgreSQL 16 | Database |
| Entity Framework Core 9 | ORM & migrations |
| FluentValidation | Request validation |
| JWT Bearer | Authentication |
| Swagger / OpenAPI | API documentation |
| xUnit | Unit & integration testing |

## Project Structure

```
backend/
├── src/VolunteerPortal.API/
│   ├── Controllers/         # API endpoints
│   │   ├── AdminController        # User management (admin)
│   │   ├── AuthController         # Login, register, profile
│   │   ├── EventsController       # Event CRUD
│   │   ├── HealthController       # Health check
│   │   ├── RegistrationsController # Event registration
│   │   ├── ReportsController      # Export reports
│   │   ├── SkillsController       # Skill management
│   │   └── StatisticsController   # Dashboard stats
│   ├── Models/
│   │   ├── Entities/        # Domain entities (User, Event, Registration, Skill)
│   │   ├── DTOs/            # Request/response models
│   │   └── Enums/           # UserRole, EventStatus, RegistrationStatus
│   ├── Services/            # Business logic layer
│   ├── Data/                # DbContext & data seeding
│   ├── Middleware/          # Global exception handling
│   ├── Validators/          # FluentValidation rules
│   ├── Swagger/             # Swagger operation filters
│   └── Migrations/          # EF Core migrations
├── tests/
│   ├── VolunteerPortal.Tests/       # Unit tests
│   └── VolunteerPortal.API.Tests/   # Integration tests
├── docker-compose.yml       # PostgreSQL + pgAdmin
└── VolunteerPortal.sln      # Solution file
```

## Quick Start

```bash
# 1. Start PostgreSQL
docker compose up -d

# 2. Run the API
cd src/VolunteerPortal.API
dotnet restore
dotnet ef database update
dotnet run
```

API available at **http://localhost:5000** · Swagger UI at **http://localhost:5000/swagger**

## API Endpoints

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | — | Register new user |
| POST | `/api/auth/login` | — | Login, returns JWT |
| GET | `/api/auth/me` | ✅ | Current user profile |

### Events
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/events` | — | List events (paginated, filterable) |
| GET | `/api/events/{id}` | — | Event details |
| POST | `/api/events` | ✅ Organizer | Create event |
| PUT | `/api/events/{id}` | ✅ Organizer | Update event |
| DELETE | `/api/events/{id}` | ✅ Organizer | Delete event |
| POST | `/api/events/{id}/image` | ✅ Organizer | Upload event image |
| DELETE | `/api/events/{id}/image` | ✅ Organizer | Delete event image |
| PUT | `/api/events/{id}/cancel` | ✅ Organizer | Cancel event |

### Registrations
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/events/{id}/register` | ✅ | Register for event |
| DELETE | `/api/events/{id}/register` | ✅ | Cancel registration |
| GET | `/api/users/me/registrations` | ✅ | User's registrations |
| GET | `/api/events/{id}/registrations` | ✅ Organizer | Event's registrations |

### Skills
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/skills` | — | List all skills |
| GET | `/api/skills/me` | ✅ | Get user's skills |
| PUT | `/api/skills/me` | ✅ | Update user's skills |

### Admin
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/admin/stats` | ✅ Admin | Dashboard statistics |
| GET | `/api/admin/users` | ✅ Admin | List all users |
| PUT | `/api/admin/users/{id}/role` | ✅ Admin | Change user role |
| DELETE | `/api/admin/users/{id}` | ✅ Admin | Delete user (soft delete) |

### Reports (Excel Export)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/reports/users/export` | ✅ Admin | Export users |
| GET | `/api/reports/events/export` | ✅ Admin | Export events |
| GET | `/api/reports/registrations/export` | ✅ Admin | Export registrations |
| GET | `/api/reports/skills/export` | ✅ Admin | Export skills |

### Health
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/health` | — | Health check |

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/backend-ci.yml`) runs on:
- **Push** to any branch when `backend/**` files change
- **Pull Request** to `main` branch when `backend/**` files change

### Pipeline Jobs

| Job | Description | Runs On |
|-----|-------------|--------|
| **Build and Test** | Restore, build, run unit tests with coverage | All pushes |
| **Integration Tests** | Run with PostgreSQL service container | main branch & PRs |

### Features
- Code coverage collection with Cobertura format
- Coverage report posted as PR comment
- Codecov integration for coverage tracking
- Test results uploaded as artifacts (30 days retention)
- PostgreSQL 16 service container for integration tests

## Authentication

The API uses JWT Bearer tokens. Include the token in request headers:

```
Authorization: Bearer <your-jwt-token>
```

**Roles**: `Volunteer` (default), `Organizer`, `Admin`

## Seeded Test Data

On first run, the database is seeded with test accounts and 15 predefined skills:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@volunteer-portal.com | Admin123! |
| Organizer | organizer@volunteer-portal.com | Organizer123! |
| Volunteer | volunteer@volunteer-portal.com | Volunteer123! |

## Environment Variables

See [Environment Variables](../Docs/ENVIRONMENT.md) for full configuration reference.

Key settings in `appsettings.json` / `appsettings.Development.json`:

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Secret` | JWT signing key (set via user-secrets in dev) |
| `Jwt:Issuer` | Token issuer |
| `Jwt:Audience` | Token audience |
| `Jwt:ExpirationInHours` | Token lifetime |
| `Cors:AllowedOrigins` | Allowed frontend origins |

## Database

### Docker (recommended)

```bash
docker compose up -d      # Start PostgreSQL + pgAdmin
docker compose ps          # Check status
docker compose down        # Stop containers
docker compose down -v     # Stop and delete data
```

pgAdmin available at **http://localhost:5050** (admin@admin.com / admin)

### Migrations

```bash
cd src/VolunteerPortal.API
dotnet ef migrations add <MigrationName>   # Create migration
dotnet ef database update                   # Apply migrations
dotnet ef database drop                     # Drop database
```

## Testing

```bash
cd backend
dotnet test                     # Run all tests
dotnet test --verbosity normal  # With detailed output
dotnet test --collect:"XPlat Code Coverage"  # With coverage
```

## Key Commands

| Command | Description |
|---------|-------------|
| `dotnet run` | Start API server |
| `dotnet watch run` | Start with hot reload |
| `dotnet build` | Build project |
| `dotnet test` | Run all tests |
| `dotnet ef database update` | Apply migrations |
