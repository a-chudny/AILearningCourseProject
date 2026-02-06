# Volunteer Portal — Frontend

React 19 + TypeScript single-page application with Tailwind CSS, built with Vite.

## Tech Stack

| Technology | Purpose |
|---|---|
| React 19.2 | UI framework |
| TypeScript 5.9 | Type safety (strict mode) |
| Vite 7.3 | Build tool & dev server |
| Tailwind CSS 4 | Utility-first styling |
| React Router 7 | Client-side routing |
| TanStack Query 5 | Server state & caching |
| React Hook Form | Form handling & validation |
| Axios | HTTP client |
| Vitest + RTL | Testing |

## Project Structure

```
frontend/src/
├── components/
│   ├── admin/           # Admin panel components
│   ├── events/          # Event cards, lists, forms
│   ├── layout/          # Header, Footer, Sidebar
│   ├── modals/          # Modal dialogs
│   ├── registrations/   # Registration management
│   ├── skills/          # Skill selector components
│   ├── skeletons/       # Loading skeleton placeholders
│   ├── ErrorBoundary    # Error boundary wrapper
│   ├── ImageUpload      # Image upload component
│   ├── ProtectedRoute   # Auth-protected route wrapper
│   └── RoleGuard        # Role-based access control
├── pages/
│   ├── admin/           # Admin dashboard, user management
│   ├── auth/            # Login, register pages
│   ├── public/          # Public event browsing
│   └── user/            # User profile, my events
├── layouts/             # Page layout templates (Main, Auth, Admin)
├── hooks/               # Custom React hooks (useAuth, useEvents, etc.)
├── services/            # API client functions
├── types/               # TypeScript type definitions
├── context/             # React context providers
├── routes/              # Route configuration
├── utils/               # Helper utilities
└── test/                # Test setup & utilities
```

## Quick Start

```bash
npm install
npm run dev
```

App available at **http://localhost:5173** — proxies API requests to `http://localhost:5000`.

## Available Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start dev server with HMR |
| `npm run build` | Production build |
| `npm run preview` | Preview production build |
| `npm run test` | Run tests in watch mode |
| `npm run test:coverage` | Generate coverage report |
| `npm run lint` | ESLint check |
| `npm run lint:fix` | Auto-fix lint issues |
| `npm run format` | Prettier formatting |
| `npm run typecheck` | TypeScript type checking |

## Key Patterns

### Data Fetching

Server state managed with TanStack Query hooks in `src/hooks/`:

```typescript
// Example: useEvents hook with query params
const { data, isLoading, error } = useEvents({ page: 1, search: 'cleanup' });
```

### Authentication

JWT-based auth via `useAuth` hook and `AuthContext`:

```typescript
const { user, login, logout, isAuthenticated } = useAuth();
```

Protected routes use `<ProtectedRoute>` and `<RoleGuard>` components.

### API Services

Centralized in `src/services/` with typed request/response models from `src/types/`.

### Component Patterns

- **Functional components** with TypeScript props interfaces
- **Custom hooks** for reusable stateful logic
- **Skeleton loaders** for loading states
- **Error boundaries** for error handling
- **Role guards** for authorization

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/frontend-ci.yml`) runs on:
- **Push** to any branch when `frontend/**` files change
- **Pull Request** to `main` branch when `frontend/**` files change

### Pipeline Jobs

| Job | Description | Runs On |
|-----|-------------|--------|
| **Lint and TypeCheck** | ESLint + TypeScript validation | All pushes |
| **Build** | Production build | main branch & PRs |
| **Test with Coverage** | Vitest with v8 coverage | main branch & PRs |

### Features
- ESLint code quality checks
- TypeScript strict mode validation  
- Production build verification
- Coverage report posted as PR comment (vitest-coverage-report-action)
- Codecov integration for coverage tracking
- Build artifacts uploaded (7 days retention)
- Test results uploaded (30 days retention)

## Testing

```bash
npm run test              # Watch mode
npm run test:coverage     # Coverage report (v8 provider)
```

- **Framework**: Vitest + React Testing Library
- **Coverage**: 68%+ statement coverage
- **Test files**: Co-located `*.test.tsx` and `src/__tests__/`

## Environment Variables

See [Environment Variables](../Docs/ENVIRONMENT.md) for full reference.

The frontend uses Vite's `import.meta.env` for configuration. API base URL is configured via Vite proxy in `vite.config.ts`.
