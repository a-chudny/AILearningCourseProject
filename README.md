# Volunteer Event Portal

A full-stack web application for managing volunteer events, built as a learning project for GitHub Copilot AI training. The goal is to achieve ~90% AI-generated code through effective prompting.

## 🏗️ Tech Stack

### Backend
- **.NET 10** - Web API
- **PostgreSQL** - Database
- **Entity Framework Core 9** - ORM
- **JWT Authentication** - Security
- **FluentValidation** - Input validation
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing

### Frontend
- **React 19.2** - UI framework
- **TypeScript 5.9** - Type safety
- **Vite 7.3** - Build tool
- **Tailwind CSS 4** - Styling
- **React Router 7** - Navigation
- **TanStack Query 5** - Server state
- **Axios** - HTTP client
- **React Hook Form** - Form handling
- **Vitest** - Testing

## 📋 Prerequisites

Before running the project locally, ensure you have the following installed:

- **Node.js 20+** and npm - [Download](https://nodejs.org/)
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)
- **Git** - [Download](https://git-scm.com/downloads)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd AILearningCourseProject
```

### 2. Database Setup with Docker

Start PostgreSQL using Docker Compose:

```bash
cd backend
docker compose up -d
```

This will:
- Pull and start PostgreSQL 16 in a Docker container
- Create a database named `volunteer_portal`
- Expose PostgreSQL on port `5432`
- Use credentials: `postgres/postgres`
- Persist data in a Docker volume

Verify the database is running:

```bash
docker compose ps
```

To stop the database:

```bash
docker compose down
```

To stop and remove all data:

```bash
docker compose down -v
```

**Alternative:** If you prefer a local PostgreSQL installation instead of Docker:

Create a PostgreSQL database manually:

```sql
-- Connect to PostgreSQL (use psql or pgAdmin)
CREATE DATABASE volunteer_portal;
```

Update the connection string in `backend/src/VolunteerPortal.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=volunteer_portal;Username=postgres;Password=your_password"
  }
}
```

### 3. Backend Setup

Navigate to the backend API directory:

```bash
cd backend/src/VolunteerPortal.API
```

Restore dependencies:

```bash
dotnet restore
```

Run Entity Framework migrations to create the database schema:

```bash
dotnet ef database update
```

Run the backend API:

```bash
dotnet run
```

The API will start at:
- **HTTP**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

To run backend tests:

```bash
cd backend
dotnet test
```

### 4. Frontend Setup

Open a new terminal and navigate to the frontend directory:

```bash
cd frontend
```

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```

The frontend will start at:
- **Local**: http://localhost:5173

The frontend is configured to proxy API requests to the backend at `http://localhost:5000`.

### 5. Verify Setup

1. Open http://localhost:5173 in your browser
2. You should see the Volunteer Event Portal homepage
3. The "Backend Connection Status" card should show "Connected Successfully!" if the backend is running

## 🧪 Running Tests

### Backend Tests

```bash
cd backend
dotnet test
```

### Frontend Tests

```bash
cd frontend
npm run test        # Run tests in watch mode
npm run test:ui     # Run tests with UI
npm run coverage    # Generate coverage report
```

## 📝 Available Scripts

### Backend

| Command | Description |
|---------|-------------|
| `dotnet run` | Start the API server |
| `dotnet build` | Build the project |
| `dotnet test` | Run all tests |
| `dotnet watch run` | Run with hot reload |
| `dotnet ef migrations add <name>` | Create a new migration |
| `dotnet ef database update` | Apply migrations |

### Frontend

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server |
| `npm run build` | Build for production |
| `npm run preview` | Preview production build |
| `npm run test` | Run tests |
| `npm run coverage` | Generate coverage report |
| `npm run lint` | Check code quality |
| `npm run lint:fix` | Auto-fix linting issues |
| `npm run format` | Format code with Prettier |
| `npm run typecheck` | Check TypeScript types |

## 🏗️ Project Structure

```
AILearningCourseProject/
├── backend/
│   ├── src/
│   │   └── VolunteerPortal.API/        # Main API project
│   │       ├── Controllers/            # API controllers
│   │       ├── Data/                   # DbContext
│   │       ├── Program.cs              # App entry point
│   │       └── appsettings.json        # Configuration
│   └── tests/
│       └── VolunteerPortal.Tests/      # Unit tests
├── frontend/
│   ├── src/
│   │   ├── components/                 # Reusable components
│   │   ├── pages/                      # Page components
│   │   ├── services/                   # API client
│   │   ├── types/                      # TypeScript types
│   │   └── routes/                     # Route configuration
│   ├── vite.config.ts                  # Vite configuration
│   └── package.json                    # Dependencies
└── Docs/
    ├── REQUIREMENTS.md                 # Project requirements
    ├── USER-STORIES.md                 # User stories
    └── ai-workflow-log.md              # Development log
```

## 🔧 Development Workflow

This project follows an AI-first development approach:

1. **Start with User Stories** - See `Docs/USER-STORIES.md`
2. **Generate Code with AI** - Use GitHub Copilot for implementation
3. **Track Progress** - Log work in `Docs/ai-workflow-log.md`
4. **Test & Verify** - Ensure quality with automated tests

## 📚 Documentation

- **[Requirements](Docs/REQUIREMENTS.md)** - Feature specifications
- **[User Stories](Docs/USER-STORIES.md)** - Implementation roadmap
- **[AI Workflow Log](Docs/ai-workflow-log.md)** - Development history
- **[GitHub Copilot Instructions](.github/copilot-instructions.md)** - AI prompting guidelines

## 🐛 Troubleshooting

### Docker issues
- Ensure Docker Desktop is running
- Check container status: `docker compose ps`
- View logs: `docker compose logs postgres`
- Restart containers: `docker compose restart`

### Backend won't start
- Ensure PostgreSQL container is running: `docker compose ps`
- Check database connection string in `appsettings.Development.json`
- Verify .NET 10 SDK is installed: `dotnet --version`
- Ensure migrations are applied: `dotnet ef database update`

### Frontend can't connect to API
- Ensure backend is running on port 5000
- Check Vite proxy configuration in `vite.config.ts`
- Clear browser cache and reload

### Database migration errors
- Ensure PostgreSQL container is running: `docker compose ps`
- Drop and recreate database if needed
- Verify EF Core tools: `dotnet tool install --global dotnet-ef`
- Check connection string matches Docker Compose credentials (postgres/postgres)

### Node/npm issues
- Clear npm cache: `npm cache clean --force`
- Delete `node_modules` and `package-lock.json`, then run `npm install`
- Ensure Node.js version is 20+: `node --version`

## 📄 License

This is a learning project for educational purposes.

## 🤝 Contributing

This is a training project focused on AI-assisted development. See `.github/copilot-instructions.md` for development guidelines.