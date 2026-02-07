# Volunteer Event Portal

A full-stack web application for managing volunteer events, built as a learning project for the **GitHub Copilot AI training course**. The goal is to achieve ~90% AI-generated code through effective prompting.

> **AI Workflow Review**: See the [AI Workflow Log](Docs/ai-workflow-log.md) for a detailed record of the development process, prompts used, and AI generation percentages.

## Features

### User Roles
| Role | Capabilities |
|------|-------------|
| **Volunteer** | Browse events, register/cancel registrations, manage profile & skills |
| **Organizer** | All Volunteer features + create/edit/cancel own events, view registrations |
| **Admin** | All features + manage users, change roles, delete users, export reports |

### Business Features
- **Event Management**: Create, edit, cancel events with images, required skills, capacity limits, registration deadlines
- **Registration System**: Register for events, cancel registrations, view registration history
- **Skill Matching**: 15 predefined skills (First Aid, Teaching, Driving, etc.) for users and events
- **Admin Dashboard**: Statistics overview, user management, role assignments
- **Excel Reports**: Export users, events, registrations, and skills data
- **Soft Delete**: Users and events can be deactivated without data loss

## Tech Stack

| Backend | Frontend |
|---------|----------|
| .NET 10 Web API | React 19.2 + TypeScript 5.9 |
| PostgreSQL 16 + EF Core 9 | Vite 7.3 + Tailwind CSS 4 |
| JWT Authentication | TanStack Query 5 |
| FluentValidation + MediatR | React Router 7 + React Hook Form |
| Swagger / OpenAPI | Vitest + React Testing Library |
| xUnit | Axios |

## Prerequisites

- [Node.js 20+](https://nodejs.org/) · [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) · [Docker Desktop](https://www.docker.com/products/docker-desktop/) · [Git](https://git-scm.com/downloads)

## Quick Start

```bash
# 1. Clone
git clone <repository-url> && cd AILearningCourseProject

# 2. Start database
cd backend && docker compose up -d

# 3. Run backend (new terminal)
cd backend/src/VolunteerPortal.API
dotnet restore && dotnet ef database update && dotnet run
# → API: http://localhost:5000  |  Swagger: http://localhost:5000/swagger

# 4. Run frontend (new terminal)
cd frontend && npm install && npm run dev
# → App: http://localhost:5173
```

**Verify**: Open http://localhost:5173 — the "Backend Connection Status" card should show "Connected Successfully!"

### Seeded Test Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@volunteer-portal.com | Admin123! |
| Organizer | organizer@volunteer-portal.com | Organizer123! |
| Volunteer | volunteer@volunteer-portal.com | Volunteer123! |

The database is also seeded with 15 predefined skills.

## Demo

See the application in action:

![Demo](Docs/Demo/demo.gif)

## Running Tests

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend
npm run test              # Watch mode
npm run test:coverage     # With coverage report
```

## Project Structure

```
AILearningCourseProject/
├── backend/                    → See backend/README.md
│   ├── src/VolunteerPortal.API/   # Controllers, Services, Models, Migrations
│   ├── tests/                     # xUnit tests
│   └── docker-compose.yml         # PostgreSQL + pgAdmin
├── frontend/                   → See frontend/README.md
│   ├── src/                       # Components, Pages, Hooks, Services, Types
│   └── vitest.config.ts           # Test configuration
├── Docs/
│   ├── REQUIREMENTS.md            # Feature specifications
│   ├── USER-STORIES.md            # Implementation roadmap
│   ├── ENVIRONMENT.md             # Environment variables reference
│   └── ai-workflow-log.md         # AI development log
└── .github/
    ├── workflows/                 # CI pipelines (backend + frontend)
    ├── copilot-instructions.md    # AI prompting guidelines
    └── instructions/              # Coding standards
```

## Documentation

| Document | Description |
|----------|-------------|
| [AI Workflow Log](Docs/ai-workflow-log.md) | Development history & AI usage tracking |
| [Requirements](Docs/REQUIREMENTS.md) | Feature specifications |
| [User Stories](Docs/USER-STORIES.md) | Implementation roadmap |
| [Environment Variables](Docs/ENVIRONMENT.md) | Configuration reference |
| [Backend README](backend/README.md) | API details, endpoints, database setup |
| [Frontend README](frontend/README.md) | Component architecture, scripts, patterns |
| [Copilot Instructions](.github/copilot-instructions.md) | AI prompting guidelines |
| [Swagger UI](http://localhost:5000/swagger) | Interactive API documentation (when running) |

## CI/CD Pipelines

GitHub Actions workflows run automatically:

| Pipeline | Triggers | Jobs |
|----------|----------|------|
| **Backend CI** | Push/PR to `backend/**` | Build, Unit Tests, Integration Tests (with PostgreSQL), Coverage |
| **Frontend CI** | Push/PR to `frontend/**` | Lint, TypeCheck, Build, Tests with Coverage |

See [backend/README.md](backend/README.md#cicd-pipeline) and [frontend/README.md](frontend/README.md#cicd-pipeline) for details.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Docker not starting | Ensure Docker Desktop is running, then `docker compose ps` |
| Backend won't start | Check PostgreSQL is running, verify connection string, run `dotnet ef database update` |
| Frontend can't reach API | Ensure backend runs on port 5000, check Vite proxy config |
| Database errors | Try `docker compose down -v && docker compose up -d` to reset data |

## 🤝 Contributing

This is a training project focused on AI-assisted development. See `.github/copilot-instructions.md` for development guidelines.