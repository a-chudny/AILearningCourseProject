# Volunteer Event Portal - Project Requirements

## Project Summary
A lightweight full-stack web application for managing volunteer events. Volunteers can browse and register for events, while organizers can create and manage events. The project demonstrates AI-assisted development with ~90% generated code.

---

## Business Context

### Problem Statement
Non-profit organizations and community groups struggle to efficiently coordinate volunteer activities. Current challenges include:
- **Manual coordination**: Event organizers rely on spreadsheets, emails, and phone calls to manage volunteers
- **Skill mismatch**: Volunteers are assigned to events without considering their skills or preferences
- **Limited visibility**: Volunteers don't have an easy way to discover opportunities that match their interests
- **No centralized tracking**: Organizations lack a unified view of volunteer participation and engagement
- **Administrative burden**: Generating reports and tracking participation requires significant manual effort

### Solution
The Volunteer Event Portal provides a centralized platform that:
- Enables organizers to create and publish volunteer opportunities
- Allows volunteers to discover events matching their skills and interests
- Automates registration and capacity management
- Provides administrators with reporting and oversight capabilities
- Streamlines communication between organizers and volunteers

### Target Users

| User Type | Description | Primary Goals |
|-----------|-------------|---------------|
| **Volunteers** | Community members looking to contribute their time | Find relevant opportunities, register easily, track participation |
| **Organizers** | Non-profit staff or community leaders | Create events, manage registrations, find skilled volunteers |
| **Administrators** | Platform managers | Oversee all activities, manage users, generate reports |

### Business Value
- **For Volunteers**: Easy discovery of opportunities, skill-based matching, participation history
- **For Organizers**: Reduced administrative work, better volunteer matching, streamlined communication
- **For Organizations**: Increased volunteer engagement, data-driven insights, scalable coordination

### Key Metrics
- Number of active volunteers
- Event registration rate
- Volunteer retention (repeat participation)
- Skill coverage across volunteer pool
- Event capacity utilization

---

## Technology Stack

### Backend
- **.NET 10** with ASP.NET Core Web API
- **Entity Framework Core** with PostgreSQL database
- **JWT Authentication** for secure API access
- **xUnit** for unit testing
- **FluentValidation** for request validation
- **ClosedXML** for CSV export

### Frontend
- **React 19+** with TypeScript (strict mode)
- **Vite** as build tool
- **React Router v7** for routing
- **React Query (TanStack Query)** for server state management
- **Axios** for HTTP client
- **Vitest + React Testing Library** for unit testing

---

## User Roles

| Role | Description |
|------|-------------|
| **Guest** | Unauthenticated user, can browse public events |
| **Volunteer** | Registered user who can sign up for events |
| **Organizer** | Can create and manage their own events |
| **Admin** | Full access to manage all users, events, and reports |

---

## Functional Requirements

### Authentication & Authorization

| ID | Requirement | Priority |
|----|-------------|----------|
| AUTH-01 | Users can register with email and password | High |
| AUTH-02 | Users can log in and receive JWT token | High |
| AUTH-03 | Users can log out (client-side token removal) | High |
| AUTH-04 | Protected routes require valid JWT | High |
| AUTH-05 | Role-based access control (Volunteer, Organizer, Admin) | High |

### Event Management

| ID | Requirement | Priority |
|----|-------------|----------|
| EVT-01 | Organizers can create new events | High |
| EVT-02 | Organizers can edit their own events | High |
| EVT-03 | Organizers can cancel their own events (soft delete) | Medium |
| EVT-04 | All users can view list of upcoming active events | High |
| EVT-05 | All users can view event details | High |
| EVT-06 | Events have: title, description, start time, duration, location, capacity | High |
| EVT-07 | Events show current registration count | Medium |
| EVT-08 | Admins can manage all events | Medium |
| EVT-09 | Events can optionally require specific skills | Medium |
| EVT-10 | Users can filter events by required skills | Medium |
| EVT-11 | Events have status: Active or Cancelled | High |
| EVT-12 | Events can have an optional image/banner | Medium |
| EVT-13 | Events have a registration deadline | Medium |

### Volunteer Registration

| ID | Requirement | Priority |
|----|-------------|----------|
| REG-01 | Volunteers can register for an event | High |
| REG-02 | Volunteers can cancel their registration | High |
| REG-03 | Volunteers can view their registered events | High |
| REG-04 | Registration prevented when event is full | High |
| REG-05 | Registration prevented for past events | Medium |
| REG-06 | Registration prevented after registration deadline | Medium |
| REG-07 | Registration prevented for cancelled events | High |

### User Management

| ID | Requirement | Priority |
|----|-------------|----------|
| USR-01 | Users can view their profile | Medium |
| USR-02 | Users can update their profile (name) | Medium |
| USR-03 | Admins can view all users | Medium |
| USR-04 | Admins can change user roles | Low |

### Skill Preferences

| ID | Requirement | Priority |
|----|-------------|----------|
| SKL-01 | System has a predefined list of skills | High |
| SKL-02 | Volunteers can select skills they possess | High |
| SKL-03 | Volunteers can update their skills from profile | Medium |
| SKL-04 | Organizers can assign required skills to events | Medium |
| SKL-05 | Events can have zero or multiple required skills | High |
| SKL-06 | Event list shows skill requirements | Medium |
| SKL-07 | Volunteers see skill match indicator on events | Low |

### Admin Reports (CSV Export)

| ID | Requirement | Priority |
|----|-------------|----------|
| RPT-01 | Admins can export list of all events to CSV | Medium |
| RPT-02 | Admins can export all registrations to CSV | Medium |
| RPT-03 | Admins can export volunteer skills summary to CSV | Low |
| RPT-04 | Exported files include relevant metadata (date, filters) | Low |

---

## Predefined Skills List

| Skill ID | Skill Name | Category |
|----------|------------|----------|
| 1 | First Aid / CPR | Medical |
| 2 | Teaching / Tutoring | Education |
| 3 | Driving | Transportation |
| 4 | Cooking / Food Preparation | Food Service |
| 5 | Event Setup / Cleanup | General Labor |
| 6 | Photography / Videography | Media |
| 7 | Social Media / Marketing | Communications |
| 8 | Translation / Interpretation | Languages |
| 9 | IT / Technical Support | Technology |
| 10 | Childcare | Care |
| 11 | Senior Care | Care |
| 12 | Gardening / Landscaping | Outdoor |
| 13 | Construction / Repair | Skilled Trade |
| 14 | Counseling / Mentoring | Support |
| 15 | Administrative / Office Work | Office |

---

## Frontend Requirements

### Pages

| Page | Route | Layout | Access |
|------|-------|--------|--------|
| Home | `/` | Main | Public |
| Event List | `/events` | Main | Public |
| Event Details | `/events/:id` | Main | Public |
| Login | `/login` | Auth | Guest only |
| Register | `/register` | Auth | Guest only |
| My Events | `/my-events` | Main | Volunteer+ |
| Create Event | `/events/new` | Main | Organizer+ |
| Edit Event | `/events/:id/edit` | Main | Organizer+ |
| Profile | `/profile` | Main | Authenticated |
| Admin Dashboard | `/admin` | Admin | Admin only |
| Admin Users | `/admin/users` | Admin | Admin only |
| Admin Events | `/admin/events` | Admin | Admin only |
| Admin Reports | `/admin/reports` | Admin | Admin only |

### Layouts

| Layout | Description | Components |
|--------|-------------|------------|
| **MainLayout** | Standard user-facing layout | Header, Navigation, Footer, Content area |
| **AuthLayout** | Minimal layout for auth pages | Centered card, Logo, Content area |
| **AdminLayout** | Admin panel layout | Sidebar navigation, Header, Content area |

### Reusable Components

| Component | Description |
|-----------|-------------|
| `Button` | Primary, secondary, danger variants |
| `Input` | Text input with label and error state |
| `Card` | Content container with optional header |
| `Modal` | Dialog overlay for confirmations |
| `EventCard` | Event summary for list display |
| `LoadingSpinner` | Loading indicator |
| `ErrorMessage` | Error display component |
| `Pagination` | Page navigation for lists |
| `ProtectedRoute` | Route guard for authenticated routes |
| `RoleGuard` | Route guard for role-based access |
| `SkillBadge` | Display skill tag with styling |
| `SkillSelector` | Multi-select component for skills |
| `ExportButton` | Button to trigger CSV export |

### State Management

| State Type | Solution |
|------------|----------|
| Server State | React Query (events, registrations, users, skills) |
| Auth State | React Context (user, token, isAuthenticated) |
| UI State | Local component state (useState) |

---

## Backend Requirements

### API Endpoints

#### Authentication
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login and get JWT | No |
| GET | `/api/auth/me` | Get current user info | Yes |

#### Events
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/events` | List all upcoming events | No |
| GET | `/api/events/{id}` | Get event details | No |
| POST | `/api/events` | Create new event | Organizer+ |
| PUT | `/api/events/{id}` | Update event | Owner/Admin |
| DELETE | `/api/events/{id}` | Delete event | Owner/Admin |

#### Registrations
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/events/{id}/registrations` | List event registrations | Organizer+ |
| POST | `/api/events/{id}/register` | Register for event | Volunteer+ |
| DELETE | `/api/events/{id}/register` | Cancel registration | Volunteer+ |
| GET | `/api/users/me/registrations` | My registrations | Volunteer+ |

#### Skills
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/skills` | List all available skills | No |
| GET | `/api/users/me/skills` | Get current user's skills | Volunteer+ |
| PUT | `/api/users/me/skills` | Update current user's skills | Volunteer+ |

#### Users (Admin)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/users` | List all users | Admin |
| GET | `/api/users/{id}` | Get user details | Admin |
| PUT | `/api/users/{id}/role` | Update user role | Admin |

#### Reports (Admin)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/reports/users/export` | Export users to CSV | Admin |
| GET | `/api/reports/events/export` | Export events to CSV | Admin |
| GET | `/api/reports/registrations/export` | Export registrations to CSV | Admin |
| GET | `/api/reports/skills/export` | Export skills summary to CSV | Admin |

### Database Schema

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│     Users       │       │  Registrations  │       │     Events      │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │       │ Id (PK)         │
│ Email           │───┐   │ UserId (FK)     │   ┌───│ OrganizerId(FK) │
│ PasswordHash    │   └──►│ EventId (FK)    │◄──┘   │ Title           │
│ Name            │       │ Status          │       │ Description     │
│ Role            │       │ RegisteredAt    │       │ StartTime       │
│ IsDeleted       │       │ CreatedAt       │       │ DurationMinutes │
│ CreatedAt       │       └─────────────────┘       │ Location        │
│ UpdatedAt       │                                 │ Capacity        │
└───────┬─────────┘                                 │ ImageUrl        │
        │                                           │ RegDeadline     │
        │                                           │ Status          │
        │                                           │ IsDeleted       │
        │                                           │ CreatedAt       │
        │                                           │ UpdatedAt       │
        │                                           └───────┬─────────┘
        │                                                   │
        │         ┌─────────────────┐                       │
        │         │     Skills      │                       │
        │         ├─────────────────┤                       │
        │         │ Id (PK)         │                       │
        │         │ Name            │                       │
        │         │ Category        │                       │
        │         └────────┬────────┘                       │
        │                  │                                │
        ▼                  ▼                                ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   UserSkills    │  │                 │  │   EventSkills   │
├─────────────────┤  │    (Many-to-    │  ├─────────────────┤
│ UserId (FK)     │  │      Many)      │  │ EventId (FK)    │
│ SkillId (FK)    │  │                 │  │ SkillId (FK)    │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

### Entity Definitions

#### User
```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }        // Required, unique
    public string PasswordHash { get; set; } // Required
    public string Name { get; set; }         // Required, max 100
    public UserRole Role { get; set; }       // Enum: Volunteer, Organizer, Admin
    public bool IsDeleted { get; set; }      // Soft delete flag
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<Event> OrganizedEvents { get; set; }
    public ICollection<Registration> Registrations { get; set; }
    public ICollection<Skill> Skills { get; set; }  // Many-to-many
}
```

#### Event
```csharp
public class Event
{
    public int Id { get; set; }
    public string Title { get; set; }        // Required, max 200
    public string Description { get; set; }  // Required, max 2000
    public DateTime StartTime { get; set; }  // Required, future date/time
    public int DurationMinutes { get; set; } // Required, min 15
    public string Location { get; set; }     // Required, max 300
    public int Capacity { get; set; }        // Required, min 1
    public int OrganizerId { get; set; }     // FK to User
    public string? ImageUrl { get; set; }    // Optional, event banner image
    public DateTime RegistrationDeadline { get; set; } // Required, before StartTime
    public EventStatus Status { get; set; }  // Enum: Active, Cancelled
    public bool IsDeleted { get; set; }      // Soft delete flag
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public User Organizer { get; set; }
    public ICollection<Registration> Registrations { get; set; }
    public ICollection<Skill> RequiredSkills { get; set; }  // Many-to-many (optional)
}
```

#### Registration
```csharp
public class Registration
{
    public int Id { get; set; }
    public int UserId { get; set; }          // FK to User
    public int EventId { get; set; }         // FK to Event
    public RegistrationStatus Status { get; set; } // Enum: Confirmed, Cancelled
    public DateTime RegisteredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public User User { get; set; }
    public Event Event { get; set; }
}
```

#### Skill
```csharp
public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; }         // Required, max 100
    public string Category { get; set; }     // Required, max 50
    
    // Navigation
    public ICollection<User> Volunteers { get; set; }      // Many-to-many
    public ICollection<Event> RequiringEvents { get; set; } // Many-to-many
}
```

### Enums

```csharp
public enum UserRole
{
    Volunteer = 0,
    Organizer = 1,
    Admin = 2
}

public enum RegistrationStatus
{
    Confirmed = 0,
    Cancelled = 1
}

public enum EventStatus
{
    Active = 0,
    Cancelled = 1
}
```

---

## Non-Functional Requirements

### Performance
- API response time < 500ms for standard operations
- Frontend initial load < 3 seconds
- Pagination for lists (10-20 items per page)
- CSV exports handled efficiently for up to 10,000 records

### Security
- Password hashing with BCrypt
- JWT tokens with expiration (24 hours)
- Input validation on all endpoints
- CORS configuration for frontend origin
- SQL injection prevention via EF Core parameterized queries
- Admin endpoints protected with role authorization

### Code Quality
- Backend unit test coverage: >70%
- Frontend component test coverage: >70%
- TypeScript strict mode enabled
- ESLint and Prettier for frontend
- EditorConfig for consistent formatting

---

## Testing Requirements

### Backend Unit Tests
| Area | Test Coverage |
|------|---------------|
| Services | All business logic methods |
| Validators | Validation rule tests |
| Controllers | Request/response handling |
| Export Service | CSV generation tests |

### Frontend Unit Tests
| Area | Test Coverage |
|------|---------------|
| Components | Rendering and user interactions |
| Hooks | Custom hook behavior |
| Utils | Helper function tests |

---

## Project Structure

### Backend
```
backend/
├── src/
│   └── VolunteerPortal.API/
│       ├── Controllers/           # API Controllers
│       ├── Endpoints/             # Minimal API endpoints (optional)
│       ├── Middleware/            # Custom middleware
│       ├── Services/              # Business logic
│       │   └── Reports/           # CSV export services
│       ├── Repositories/          # Data access
│       ├── Models/
│       │   ├── Entities/          # EF entities
│       │   ├── DTOs/              # Request/Response DTOs
│       │   └── Enums/             # Enumerations
│       ├── Data/                  # DbContext, migrations, seed data
│       ├── Validators/            # FluentValidation validators
│       └── Extensions/            # Service extensions
└── tests/
    └── VolunteerPortal.Tests/
        ├── Services/              # Service unit tests
        ├── Controllers/           # Controller tests
        └── Validators/            # Validator tests
```

### Frontend
```
frontend/
├── src/
│   ├── components/
│   │   ├── common/               # Button, Input, Card, SkillBadge, etc.
│   │   ├── events/               # EventCard, EventForm, etc.
│   │   ├── skills/               # SkillSelector, SkillList
│   │   └── layout/               # Header, Footer, Sidebar
│   ├── pages/
│   │   ├── public/               # Home, EventList, EventDetails
│   │   ├── auth/                 # Login, Register
│   │   ├── user/                 # MyEvents, Profile
│   │   └── admin/                # Dashboard, Users, Events, Reports
│   ├── layouts/
│   │   ├── MainLayout.tsx
│   │   ├── AuthLayout.tsx
│   │   └── AdminLayout.tsx
│   ├── hooks/                    # Custom React hooks
│   ├── services/                 # API client functions
│   ├── context/                  # Auth context
│   ├── types/                    # TypeScript interfaces
│   ├── utils/                    # Helper functions
│   └── routes/                   # Route configuration
└── tests/
    ├── components/               # Component tests
    └── hooks/                    # Hook tests
```

---

## Development Phases

### Phase 1: Foundation
- [ ] Backend project setup with EF Core + PostgreSQL
- [ ] Frontend project setup with Vite + TypeScript
- [ ] Database schema and migrations
- [ ] Seed predefined skills data
- [ ] Basic project structure

### Phase 2: Authentication
- [ ] User registration endpoint
- [ ] Login endpoint with JWT
- [ ] Auth context and protected routes
- [ ] Login/Register pages

### Phase 3: Events Core
- [ ] Event CRUD endpoints
- [ ] Event list and details pages
- [ ] Create/Edit event forms
- [ ] Main layout implementation

### Phase 4: Registrations
- [ ] Registration endpoints
- [ ] Register/Cancel functionality
- [ ] My Events page
- [ ] Capacity validation

### Phase 5: Skills Feature
- [ ] Skills API endpoints
- [ ] User skill selection in profile
- [ ] Event skill requirements in form
- [ ] Skill badges display on events
- [ ] Filter events by skills

### Phase 6: Admin Features
- [ ] Admin layout
- [ ] User management page
- [ ] Admin event management
- [ ] Role-based access control

### Phase 7: Reports & Export
- [ ] CSV export service (ClosedXML)
- [ ] Admin reports page
- [ ] Export endpoints for users, events, registrations
- [ ] Skills summary export

### Phase 8: Polish & Testing
- [ ] Unit tests (backend)
- [ ] Unit tests (frontend)
- [ ] Error handling improvements
- [ ] UI polish and responsive design

---

## Success Criteria
- [ ] All high-priority requirements implemented
- [ ] 3 layouts working (Main, Auth, Admin)
- [ ] 12+ pages with routing
- [ ] Authentication with role-based access
- [ ] CRUD operations for events
- [ ] Event registration flow working
- [ ] Skill preferences for users and events
- [ ] Admin CSV export functionality
- [ ] Backend test coverage >70%
- [ ] Frontend test coverage >70%
- [ ] ~90% of code AI-generated (tracked in workflow log)
