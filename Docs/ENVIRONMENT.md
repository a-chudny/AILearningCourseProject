# Environment Variables Reference

Complete configuration reference for the Volunteer Event Portal.

---

## Backend (ASP.NET Core)

Configuration is loaded from `appsettings.json`, `appsettings.{Environment}.json`, environment variables, and user-secrets.

### Database

| Variable | Description | Default (Dev) |
|----------|-------------|---------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=volunteer_portal;Username=postgres;Password=postgres` |

### JWT Authentication

| Variable | Description | Default |
|----------|-------------|---------|
| `Jwt:Secret` | Signing key (min 32 chars). **Use user-secrets in dev.** | — (required) |
| `Jwt:Issuer` | Token issuer claim | `VolunteerPortal` |
| `Jwt:Audience` | Token audience claim | `VolunteerPortalUsers` |
| `Jwt:ExpirationInHours` | Token lifetime in hours | `24` |

> **Security Note**: Never commit `Jwt:Secret` to source control. Use `dotnet user-secrets` for local development:
> ```bash
> cd backend/src/VolunteerPortal.API
> dotnet user-secrets set "Jwt:Secret" "YourSuperSecretKeyThatIsAtLeast32Characters"
> ```

### CORS

| Variable | Description | Default (Dev) |
|----------|-------------|---------------|
| `Cors:AllowedOrigins` | Allowed frontend origins (array) | `["http://localhost:5173", "http://localhost:3000"]` |

### Logging

| Variable | Description | Default |
|----------|-------------|---------|
| `Logging:LogLevel:Default` | Default log level | `Information` (prod) / `Debug` (dev) |
| `Logging:LogLevel:Microsoft.AspNetCore` | Framework log level | `Warning` (prod) / `Information` (dev) |
| `Logging:LogLevel:Microsoft.EntityFrameworkCore` | EF Core log level | `Warning` |

---

## Frontend (Vite / React)

Vite environment variables must be prefixed with `VITE_` to be exposed to client code.

### API Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| — | API base URL | Configured via Vite proxy → `http://localhost:5000` |

The API proxy is configured in `vite.config.ts`. In production, configure the reverse proxy (nginx, etc.) to forward `/api` requests to the backend.

---

## Docker Services

Defined in `backend/docker-compose.yml`:

### PostgreSQL

| Variable | Value |
|----------|-------|
| `POSTGRES_USER` | `postgres` |
| `POSTGRES_PASSWORD` | `postgres` |
| `POSTGRES_DB` | `volunteer_portal` |
| Port | `5432` |

### pgAdmin

| Variable | Value |
|----------|-------|
| `PGADMIN_DEFAULT_EMAIL` | `admin@admin.com` |
| `PGADMIN_DEFAULT_PASSWORD` | `admin` |
| Port | `5050` |

---

## Environment-Specific Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Base configuration (all environments) |
| `appsettings.Development.json` | Development overrides |
| `appsettings.Production.json` | Production overrides (create as needed) |

### ASP.NET Core Environment

Set the environment via:

```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development

# Linux / macOS
export ASPNETCORE_ENVIRONMENT=Development
```

Environments: `Development`, `Production`, `Testing`
