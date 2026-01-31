# Volunteer Event Portal - User Stories & Tasks

This document contains all user stories and development tasks for the Volunteer Event Portal project, organized in logical implementation order following the development phases defined in REQUIREMENTS.md.

---

## Story Format

Each story follows this format:
- **ID**: Unique identifier (PHASE-NUMBER)
- **Title**: Brief description
- **User Story**: As a [role], I want [feature], so that [benefit]
- **Acceptance Criteria**: Specific conditions that must be met
- **Technical Tasks**: Implementation details
- **Dependencies**: Stories that must be completed first
- **Estimate**: T-shirt size (XS, S, M, L, XL)

---

# Phase 1: Foundation

## FOUND-001: Backend Project Setup

**Title**: Initialize .NET 10 Web API Project with PostgreSQL

**User Story**:
As a developer, I want a properly configured .NET 10 Web API project with Entity Framework Core and PostgreSQL, so that I have a solid foundation for building the API.

**Acceptance Criteria**:
- [ ] .NET 10 Web API project created with proper folder structure
- [ ] Entity Framework Core configured with PostgreSQL provider
- [ ] Connection string configured via appsettings.json and user secrets
- [ ] Project builds and runs successfully
- [ ] Swagger/OpenAPI documentation enabled
- [ ] CORS configured for local frontend development
- [ ] Basic health check endpoint working (`GET /api/health`)

**Technical Tasks**:
1. Create solution file and API project structure:
   ```
   backend/
   ├── src/VolunteerPortal.API/
   └── tests/VolunteerPortal.Tests/
   ```
2. Install NuGet packages:
   - `Npgsql.EntityFrameworkCore.PostgreSQL`
   - `Microsoft.EntityFrameworkCore.Design`
   - `Swashbuckle.AspNetCore`
   - `FluentValidation.AspNetCore`
3. Create `ApplicationDbContext` class
4. Configure dependency injection in `Program.cs`
5. Add PostgreSQL connection string configuration
6. Enable Swagger with XML documentation
7. Configure CORS policy for `http://localhost:5173`
8. Create health check endpoint

**Dependencies**: None

**Estimate**: M

---

## FOUND-002: Frontend Project Setup

**Title**: Initialize React 19 + TypeScript Project with Vite

**User Story**:
As a developer, I want a properly configured React project with TypeScript, Vite, and essential libraries, so that I can build the frontend application efficiently.

**Acceptance Criteria**:
- [ ] React 19+ project created with Vite and TypeScript
- [ ] TypeScript configured in strict mode
- [ ] ESLint and Prettier configured
- [ ] React Router v7 installed and basic routing working
- [ ] React Query (TanStack Query) installed
- [ ] Axios installed and configured with base URL
- [ ] Project runs successfully on `http://localhost:5173`
- [ ] Basic folder structure created

**Technical Tasks**:
1. Create Vite project with React + TypeScript template
2. Configure `tsconfig.json` with strict mode and path aliases
3. Install and configure dependencies:
   - `react-router-dom` v7
   - `@tanstack/react-query`
   - `axios`
4. Set up ESLint with TypeScript and React rules
5. Configure Prettier with consistent formatting
6. Create folder structure:
   ```
   src/
   ├── components/
   ├── pages/
   ├── layouts/
   ├── hooks/
   ├── services/
   ├── context/
   ├── types/
   ├── utils/
   └── routes/
   ```
7. Create Axios instance with base URL configuration
8. Set up React Query provider in `App.tsx`
9. Create basic route configuration

**Dependencies**: None

**Estimate**: M

---

## FOUND-003: Database Schema and Entities

**Title**: Create Domain Entities and Database Migrations

**User Story**:
As a developer, I want all domain entities defined with proper relationships and database migrations, so that the data layer is ready for feature development.

**Acceptance Criteria**:
- [ ] All entities created: User, Event, Registration, Skill
- [ ] Many-to-many relationships configured (UserSkills, EventSkills)
- [ ] Enums created: UserRole, RegistrationStatus
- [ ] Entity configurations with proper constraints (max length, required, unique)
- [ ] Initial migration created and can be applied
- [ ] Database schema matches the design in REQUIREMENTS.md

**Technical Tasks**:
1. Create enums in `Models/Enums/`:
   - `UserRole` (Volunteer, Organizer, Admin)
   - `RegistrationStatus` (Confirmed, Cancelled)
   - `EventStatus` (Active, Cancelled)
2. Create entities in `Models/Entities/`:
   - `User` with validation attributes (include `IsDeleted` for soft delete)
   - `Event` with validation attributes (include `StartTime`, `DurationMinutes`, `ImageUrl`, `RegistrationDeadline`, `Status`, `IsDeleted`)
   - `Registration` with composite relationship
   - `Skill` with category
3. Configure entity relationships in `ApplicationDbContext`:
   - User → Events (one-to-many via OrganizerId)
   - User → Registrations (one-to-many)
   - Event → Registrations (one-to-many)
   - User ↔ Skills (many-to-many)
   - Event ↔ Skills (many-to-many)
4. Add entity configurations with Fluent API:
   - Unique constraint on User.Email
   - Max lengths for string properties
   - Required properties
   - Cascade delete rules
5. Create and apply initial migration
6. Test migration with empty database

**Dependencies**: FOUND-001

**Estimate**: L

---

## FOUND-004: Seed Predefined Skills Data

**Title**: Create Database Seeding for Skills

**User Story**:
As a developer, I want predefined skills seeded into the database, so that users can select from a consistent list of skills.

**Acceptance Criteria**:
- [ ] All 15 predefined skills seeded with categories
- [ ] Seeding runs automatically on application startup (if empty)
- [ ] Seeding is idempotent (can run multiple times safely)
- [ ] Skills match the list in REQUIREMENTS.md

**Technical Tasks**:
1. Create `DataSeeder` class in `Data/` folder
2. Implement skill seeding with all 15 skills:
   - First Aid / CPR (Medical)
   - Teaching / Tutoring (Education)
   - Driving (Transportation)
   - Cooking / Food Preparation (Food Service)
   - Event Setup / Cleanup (General Labor)
   - Photography / Videography (Media)
   - Social Media / Marketing (Communications)
   - Translation / Interpretation (Languages)
   - IT / Technical Support (Technology)
   - Childcare (Care)
   - Senior Care (Care)
   - Gardening / Landscaping (Outdoor)
   - Construction / Repair (Skilled Trade)
   - Counseling / Mentoring (Support)
   - Administrative / Office Work (Office)
3. Register seeder in `Program.cs` to run on startup
4. Ensure seeding checks for existing data before inserting
5. Add unit test for seeder

**Dependencies**: FOUND-003

**Estimate**: S

---

## FOUND-005: TypeScript Type Definitions

**Title**: Create Shared TypeScript Types for API Contracts

**User Story**:
As a frontend developer, I want TypeScript interfaces matching the API contracts, so that I have type safety when working with backend data.

**Acceptance Criteria**:
- [ ] All entity types defined (User, Event, Registration, Skill)
- [ ] All enum types defined (UserRole, RegistrationStatus)
- [ ] Request/Response DTO types defined
- [ ] API error response type defined
- [ ] Types are exported from a central location

**Technical Tasks**:
1. Create type files in `src/types/`:
   - `entities.ts` - User, Event, Registration, Skill interfaces
   - `enums.ts` - UserRole, RegistrationStatus enums
   - `api.ts` - API request/response types
   - `auth.ts` - Auth-related types (LoginRequest, RegisterRequest, AuthResponse)
   - `index.ts` - Re-export all types
2. Define interfaces matching backend entities:
   ```typescript
   interface User {
     id: number;
     email: string;
     name: string;
     role: UserRole;
     createdAt: string;
     skills?: Skill[];
   }
   ```
3. Define API response wrapper types
4. Define form input types for validation

**Dependencies**: FOUND-002

**Estimate**: S

---

# Phase 2: Authentication

## AUTH-001: User Registration Endpoint

**Title**: Implement User Registration API

**User Story**:
As a new user, I want to register with my email and password, so that I can create an account and access the platform.

**Acceptance Criteria**:
- [ ] `POST /api/auth/register` endpoint working
- [ ] Email validation (valid format, unique)
- [ ] Password validation (min 8 characters, complexity rules)
- [ ] Name validation (required, max 100 characters)
- [ ] Password hashed with BCrypt before storing
- [ ] New users assigned "Volunteer" role by default
- [ ] Returns 201 Created on success with user info (no password)
- [ ] Returns 400 Bad Request with validation errors
- [ ] Returns 409 Conflict if email already exists

**Technical Tasks**:
1. Create DTOs in `Models/DTOs/Auth/`:
   - `RegisterRequest` (Email, Password, Name)
   - `AuthResponse` (Id, Email, Name, Role, Token)
2. Create `IAuthService` interface with `RegisterAsync` method
3. Implement `AuthService`:
   - Validate email uniqueness
   - Hash password with BCrypt
   - Create user with Volunteer role
   - Return user info
4. Create `RegisterRequestValidator` with FluentValidation
5. Create `AuthController` with register endpoint
6. Add unit tests for `AuthService.RegisterAsync`
7. Add integration test for register endpoint

**Dependencies**: FOUND-003

**Estimate**: M

---

## AUTH-002: User Login Endpoint with JWT

**Title**: Implement User Login with JWT Token Generation

**User Story**:
As a registered user, I want to log in with my email and password, so that I can receive a JWT token for authenticated requests.

**Acceptance Criteria**:
- [ ] `POST /api/auth/login` endpoint working
- [ ] Validates email and password against stored credentials
- [ ] Generates JWT token with user claims (id, email, role)
- [ ] Token expires after 24 hours
- [ ] Returns 200 OK with token and user info
- [ ] Returns 401 Unauthorized for invalid credentials
- [ ] JWT secret stored securely in configuration

**Technical Tasks**:
1. Create `LoginRequest` DTO (Email, Password)
2. Add `LoginAsync` method to `IAuthService`
3. Configure JWT settings in `appsettings.json`:
   - Secret key (use user secrets for development)
   - Issuer and Audience
   - Expiration time
4. Create `JwtService` for token generation:
   - Include claims: UserId, Email, Role
   - Set expiration to 24 hours
   - Sign with secret key
5. Implement login logic in `AuthService`:
   - Find user by email
   - Verify password hash
   - Generate and return token
6. Add login endpoint to `AuthController`
7. Configure JWT authentication in `Program.cs`
8. Add unit tests for login and token generation

**Dependencies**: AUTH-001

**Estimate**: M

---

## AUTH-003: Get Current User Endpoint

**Title**: Implement Get Current User Info API

**User Story**:
As an authenticated user, I want to retrieve my profile information using my JWT token, so that the frontend can display my details and verify my session.

**Acceptance Criteria**:
- [ ] `GET /api/auth/me` endpoint working
- [ ] Requires valid JWT token
- [ ] Returns current user info (id, email, name, role)
- [ ] Returns 401 Unauthorized without valid token
- [ ] User info includes skills list

**Technical Tasks**:
1. Create `UserResponse` DTO with user details
2. Add `GetCurrentUserAsync` method to `IAuthService`
3. Extract user ID from JWT claims in controller
4. Implement endpoint in `AuthController` with `[Authorize]` attribute
5. Include user's skills in response
6. Add unit test for the method
7. Add integration test for protected endpoint

**Dependencies**: AUTH-002

**Estimate**: S

---

## AUTH-004: Frontend Auth Context

**Title**: Implement Authentication Context and State Management

**User Story**:
As a frontend developer, I want a centralized authentication state, so that components can access user info and auth status consistently.

**Acceptance Criteria**:
- [ ] `AuthContext` provides: user, token, isAuthenticated, isLoading
- [ ] `login()` function stores token and fetches user
- [ ] `logout()` function clears token and user state
- [ ] `register()` function creates account and logs in
- [ ] Token persisted in localStorage
- [ ] Token automatically included in API requests
- [ ] Auth state restored on page refresh

**Technical Tasks**:
1. Create `AuthContext` in `src/context/AuthContext.tsx`:
   ```typescript
   interface AuthContextType {
     user: User | null;
     token: string | null;
     isAuthenticated: boolean;
     isLoading: boolean;
     login: (email: string, password: string) => Promise<void>;
     register: (data: RegisterRequest) => Promise<void>;
     logout: () => void;
   }
   ```
2. Create `AuthProvider` component:
   - Initialize state from localStorage
   - Fetch user on mount if token exists
   - Provide context values
3. Create `useAuth` hook for consuming context
4. Configure Axios interceptor to include token in headers
5. Handle token expiration (401 responses)
6. Add `AuthProvider` to `App.tsx`
7. Create unit tests for auth logic

**Dependencies**: FOUND-005, AUTH-003

**Estimate**: M

---

## AUTH-005: Login Page

**Title**: Create Login Page Component

**User Story**:
As a user, I want a login page where I can enter my credentials, so that I can authenticate and access protected features.

**Acceptance Criteria**:
- [ ] Login form with email and password fields
- [ ] Form validation with error messages
- [ ] Loading state during submission
- [ ] Error display for failed login attempts
- [ ] Redirect to home page on successful login
- [ ] Link to registration page
- [ ] Uses AuthLayout
- [ ] Redirects authenticated users away from login page

**Technical Tasks**:
1. Create `LoginPage` component in `src/pages/auth/`
2. Create login form with controlled inputs:
   - Email field with validation
   - Password field
   - Submit button with loading state
3. Use `useAuth` hook for login function
4. Implement form submission with error handling
5. Add client-side validation (email format, required fields)
6. Display API error messages
7. Add navigation after successful login
8. Create route guard to redirect authenticated users
9. Style with AuthLayout
10. Add component tests

**Dependencies**: AUTH-004

**Estimate**: M

---

## AUTH-006: Registration Page

**Title**: Create Registration Page Component

**User Story**:
As a new user, I want a registration page where I can create an account, so that I can join the platform as a volunteer.

**Acceptance Criteria**:
- [ ] Registration form with email, password, confirm password, and name
- [ ] Password strength indicator
- [ ] Form validation with error messages
- [ ] Loading state during submission
- [ ] Error display for failed registration (e.g., email exists)
- [ ] Redirect to home page on successful registration
- [ ] Link to login page
- [ ] Uses AuthLayout

**Technical Tasks**:
1. Create `RegisterPage` component in `src/pages/auth/`
2. Create registration form with fields:
   - Name (required, max 100)
   - Email (required, valid format)
   - Password (required, min 8 chars)
   - Confirm Password (must match)
3. Implement password match validation
4. Add password strength indicator (optional)
5. Use `useAuth` hook for register function
6. Display API error messages (email already exists)
7. Add navigation after successful registration
8. Style with AuthLayout
9. Add component tests

**Dependencies**: AUTH-004

**Estimate**: M

---

## AUTH-007: Auth Layout Implementation

**Title**: Create Authentication Layout Component

**User Story**:
As a user, I want auth pages (login, register) to have a clean, centered layout, so that the experience is focused and professional.

**Acceptance Criteria**:
- [ ] Centered card layout for auth content
- [ ] Application logo/title displayed
- [ ] Clean background (subtle color or pattern)
- [ ] Responsive design for mobile
- [ ] Consistent styling across auth pages

**Technical Tasks**:
1. Create `AuthLayout` component in `src/layouts/`
2. Design layout structure:
   - Full-page centered container
   - Card component for content
   - Logo/title header
   - Children render area
3. Add responsive styles
4. Apply to Login and Register pages via routing
5. Add component test

**Dependencies**: FOUND-002

**Estimate**: S

---

## AUTH-008: Protected Route Component

**Title**: Create Route Guards for Authentication

**User Story**:
As a developer, I want route guards that protect pages based on authentication and roles, so that unauthorized access is prevented.

**Acceptance Criteria**:
- [ ] `ProtectedRoute` redirects unauthenticated users to login
- [ ] `RoleGuard` checks user role and redirects if unauthorized
- [ ] Loading state shown while auth status is being determined
- [ ] Return URL preserved for post-login redirect

**Technical Tasks**:
1. Create `ProtectedRoute` component:
   - Check `isAuthenticated` from `useAuth`
   - Show loading spinner while `isLoading`
   - Redirect to `/login` if not authenticated
   - Preserve intended URL in redirect state
2. Create `RoleGuard` component:
   - Accept `allowedRoles` prop
   - Check user's role against allowed roles
   - Redirect to home or show forbidden message
3. Update `LoginPage` to redirect to return URL after login
4. Add components to route configuration
5. Add unit tests for guard logic

**Dependencies**: AUTH-004

**Estimate**: S

---

# Phase 3: Events Core

## EVT-001: Event CRUD Service

**Title**: Implement Event Service with Business Logic

**User Story**:
As a developer, I want a service layer for event operations, so that business logic is separated from controllers.

**Acceptance Criteria**:
- [ ] `IEventService` interface with CRUD methods
- [ ] Create event with organizer assignment
- [ ] Update event with ownership validation
- [ ] Delete event with ownership validation
- [ ] Get all upcoming events (future dates only)
- [ ] Get single event by ID with details
- [ ] Pagination support for list

**Technical Tasks**:
1. Create DTOs in `Models/DTOs/Events/`:
   - `CreateEventRequest` (includes StartTime, DurationMinutes, ImageUrl?, RegistrationDeadline?)
   - `UpdateEventRequest` (includes StartTime, DurationMinutes, ImageUrl?, RegistrationDeadline?, Status)
   - `EventResponse` (includes organizer info, registration count, required skills, all new fields)
   - `EventListResponse` (paginated)
2. Create `IEventService` interface:
   ```csharp
   Task<EventResponse> CreateAsync(CreateEventRequest request, int organizerId);
   Task<EventResponse> UpdateAsync(int id, UpdateEventRequest request, int userId);
   Task DeleteAsync(int id, int userId);
   Task<EventResponse?> GetByIdAsync(int id);
   Task<EventListResponse> GetAllAsync(EventQueryParams queryParams);
   ```
3. Implement `EventService`:
   - Validate organizer exists
   - Check ownership for update/delete
   - Filter by future dates
   - Include registration count
   - Include required skills
4. Create `EventRequestValidator` for FluentValidation
5. Add comprehensive unit tests

**Dependencies**: FOUND-003

**Estimate**: L

---

## EVT-002: Event API Endpoints

**Title**: Create Event CRUD API Endpoints

**User Story**:
As an API consumer, I want RESTful endpoints for event operations, so that I can manage events through the API.

**Acceptance Criteria**:
- [ ] `GET /api/events` - List upcoming events (public)
- [ ] `GET /api/events/{id}` - Get event details (public)
- [ ] `POST /api/events` - Create event (Organizer+)
- [ ] `PUT /api/events/{id}` - Update event (Owner/Admin)
- [ ] `DELETE /api/events/{id}` - Delete event (Owner/Admin)
- [ ] Proper HTTP status codes
- [ ] Pagination parameters (page, pageSize)
- [ ] Authorization enforced

**Technical Tasks**:
1. Create `EventsController` with endpoints
2. Implement authorization:
   - Public: GET list, GET by id
   - `[Authorize(Roles = "Organizer,Admin")]`: POST
   - `[Authorize]` + ownership check: PUT, DELETE
3. Add pagination query parameters
4. Return proper status codes:
   - 200: Success
   - 201: Created
   - 400: Validation error
   - 401: Unauthorized
   - 403: Forbidden (not owner)
   - 404: Not found
5. Add integration tests for all endpoints

**Dependencies**: EVT-001, AUTH-002

**Estimate**: M

---

## EVT-003: Event List Page

**Title**: Create Event Listing Page

**User Story**:
As a user, I want to see a list of upcoming volunteer events, so that I can discover opportunities to participate.

**Acceptance Criteria**:
- [ ] Displays list of upcoming events as cards
- [ ] Shows event title, date, location, and capacity
- [ ] Shows registration count / capacity
- [ ] Shows required skills as badges
- [ ] Pagination controls
- [ ] Loading state while fetching
- [ ] Empty state when no events
- [ ] Click on event navigates to details

**Technical Tasks**:
1. Create `EventListPage` component in `src/pages/public/`
2. Create `useEvents` hook with React Query:
   - Fetch events with pagination
   - Handle loading and error states
3. Create `EventCard` component:
   - Display event summary info
   - Show skill badges
   - Show capacity status
   - Clickable to event details
4. Implement pagination component
5. Add empty state component
6. Add loading skeleton
7. Configure route at `/events`
8. Add component tests

**Dependencies**: FOUND-005, EVT-002

**Estimate**: M

---

## EVT-004: Event Details Page

**Title**: Create Event Details Page

**User Story**:
As a user, I want to see full details of an event, so that I can decide whether to register.

**Acceptance Criteria**:
- [ ] Displays full event information including start time and duration
- [ ] Shows event image/banner if available
- [ ] Shows organizer name
- [ ] Shows required skills with descriptions
- [ ] Shows registration status (spots remaining)
- [ ] Shows registration deadline if set
- [ ] Displays event status (Active/Cancelled)
- [ ] Register button disabled if event is cancelled or past deadline
- [ ] Register button for authenticated volunteers
- [ ] Shows "Already registered" if user is registered
- [ ] Cancel registration option if registered
- [ ] Cancel Event button for event owner/admin (sets status to Cancelled)
- [ ] Edit button for event owner/admin
- [ ] Back to list navigation

**Technical Tasks**:
1. Create `EventDetailsPage` component in `src/pages/public/`
2. Create `useEvent` hook for fetching single event
3. Display all event fields:
   - Title, description, date/time
   - Location with potential map link
   - Capacity and current registrations
   - Organizer name
   - Required skills
4. Conditional rendering based on auth state:
   - Guest: Prompt to login to register
   - Volunteer: Register/Cancel button
   - Owner: Edit button
5. Add registration action (calls registration API)
6. Configure route at `/events/:id`
7. Add component tests

**Dependencies**: EVT-003

**Estimate**: M

---

## EVT-005: Main Layout Implementation

**Title**: Create Main Application Layout

**User Story**:
As a user, I want a consistent layout with navigation, so that I can easily move between pages.

**Acceptance Criteria**:
- [ ] Header with logo and navigation links
- [ ] Navigation shows different links based on auth state
- [ ] User menu with profile and logout for authenticated users
- [ ] Role-specific navigation items (Organizer: Create Event, Admin: Admin Panel)
- [ ] Footer with basic info
- [ ] Responsive mobile navigation (hamburger menu)
- [ ] Content area with consistent padding

**Technical Tasks**:
1. Create `MainLayout` component in `src/layouts/`
2. Create `Header` component:
   - Logo/brand link to home
   - Navigation links: Home, Events
   - Auth-specific: My Events, Profile
   - Role-specific: Create Event (Organizer+), Admin (Admin)
   - User dropdown menu
   - Login/Register buttons for guests
3. Create `Footer` component
4. Create mobile navigation (responsive)
5. Apply layout to public and user routes
6. Add component tests

**Dependencies**: AUTH-004

**Estimate**: M

---

## EVT-006: Create Event Page

**Title**: Create Event Creation Page for Organizers

**User Story**:
As an organizer, I want to create new volunteer events, so that I can recruit volunteers for my activities.

**Acceptance Criteria**:
- [ ] Form with all event fields (title, description, date, location, capacity)
- [ ] Date picker that only allows future dates
- [ ] Time picker for event start time
- [ ] Duration input in minutes (with preset options: 1h, 2h, 4h, 8h)
- [ ] Optional image upload for event banner
- [ ] Optional registration deadline date picker
- [ ] Skill selector for required skills (optional)
- [ ] Form validation with error messages
- [ ] Loading state during submission
- [ ] Success message and redirect to event details
- [ ] Only accessible to Organizer and Admin roles

**Technical Tasks**:
1. Create `CreateEventPage` component in `src/pages/user/`
2. Create `EventForm` component (reusable for create/edit):
   - Title input (required, max 200)
   - Description textarea (required, max 2000)
   - Date picker (required, future only)
   - Time picker for start time (required)
   - Duration selector in minutes (required, preset options: 60, 120, 240, 480)
   - Location input (required, max 300)
   - Capacity number input (required, min 1)
   - Image upload component (optional, max 5MB, jpg/png)
   - Registration deadline date picker (optional, must be before event date)
   - SkillSelector for required skills
3. Create `useCreateEvent` mutation hook
4. Create `useUploadEventImage` mutation hook for image handling
4. Implement form validation
5. Handle submission with error display
6. Redirect to event details on success
7. Protect route with RoleGuard (Organizer+)
8. Add component tests

**Dependencies**: EVT-002, AUTH-008

**Estimate**: M

---

## EVT-007: Edit Event Page

**Title**: Create Event Editing Page

**User Story**:
As an organizer, I want to edit my events, so that I can update event details as needed.

**Acceptance Criteria**:
- [ ] Pre-populated form with current event data
- [ ] Same validation as create form
- [ ] Loading state while fetching and submitting
- [ ] Success message and redirect on save
- [ ] Cancel button returns to event details
- [ ] Only accessible to event owner and admins
- [ ] 404 handling for non-existent events

**Technical Tasks**:
1. Create `EditEventPage` component in `src/pages/user/`
2. Reuse `EventForm` component with initial data
3. Create `useUpdateEvent` mutation hook
4. Fetch existing event data on mount
5. Handle ownership validation (redirect if not owner)
6. Implement save with API call
7. Configure route at `/events/:id/edit`
8. Add component tests

**Dependencies**: EVT-006

**Estimate**: M

---

## EVT-009: Event Image Upload

**Title**: Implement Event Image Upload Functionality

**User Story**:
As an organizer, I want to upload a banner image for my events, so that they look more attractive and professional.

**Acceptance Criteria**:
- [ ] `POST /api/events/{id}/image` - Upload event image (Organizer+)
- [ ] `DELETE /api/events/{id}/image` - Remove event image (Organizer+)
- [ ] Accepts JPG and PNG formats only
- [ ] Maximum file size: 5MB
- [ ] Image stored in configurable location (local filesystem or cloud)
- [ ] Image URL returned in event response
- [ ] Image displayed on event cards and details page
- [ ] Placeholder image shown when no image uploaded

**Technical Tasks**:
1. Create `IFileStorageService` interface:
   ```csharp
   Task<string> UploadAsync(IFormFile file, string folder);
   Task DeleteAsync(string filePath);
   ```
2. Implement `LocalFileStorageService`:
   - Save to wwwroot/uploads/events/
   - Generate unique filename (GUID)
   - Validate file type and size
3. Create `EventImageController` or add to `EventsController`:
   - POST endpoint with `[FromForm]` IFormFile
   - DELETE endpoint
   - Authorization (owner or admin)
4. Update `EventResponse` to include `imageUrl`
5. Create frontend `ImageUpload` component:
   - Drag-and-drop support
   - Preview before upload
   - Progress indicator
   - Remove button
6. Integrate into `EventForm` component
7. Add placeholder image component
8. Add unit tests for file validation
9. Add integration tests for upload endpoint

**Dependencies**: EVT-002

**Estimate**: L

---

## EVT-010: Event Cancellation

**Title**: Implement Event Cancellation Functionality

**User Story**:
As an organizer, I want to cancel my events, so that I can notify volunteers when an event can no longer proceed.

**Acceptance Criteria**:
- [ ] `PUT /api/events/{id}/cancel` - Cancel event (Organizer+)
- [ ] Only Active events can be cancelled
- [ ] Cancelled events remain visible but marked as cancelled
- [ ] Registration disabled for cancelled events
- [ ] Cancellation reason optional
- [ ] Existing registrations remain (for history)
- [ ] UI shows prominent cancelled status

**Technical Tasks**:
1. Add cancel endpoint to `EventsController`:
   - PUT `/api/events/{id}/cancel`
   - Optional body with cancellation reason
   - Authorization (owner or admin)
2. Update `EventService` with cancel logic:
   - Validate event exists and is Active
   - Set Status to Cancelled
   - Optionally store cancellation reason
3. Update frontend `EventDetailsPage`:
   - Show cancel button for owner/admin (if Active)
   - Confirmation modal with optional reason
   - Display cancelled banner prominently
4. Create `useCancelEvent` mutation hook
5. Update `EventCard` to show cancelled badge
6. Disable registration UI for cancelled events
7. Add unit and integration tests

**Dependencies**: EVT-001, EVT-004

**Estimate**: M

---

## EVT-008: Home Page

**Title**: Create Landing Home Page

**User Story**:
As a visitor, I want an attractive home page, so that I understand the platform's purpose and can quickly access key features.

**Acceptance Criteria**:
- [ ] Hero section with welcome message and call-to-action
- [ ] Brief description of the platform
- [ ] Featured/upcoming events section (3-4 events)
- [ ] Quick links: Browse Events, Register, Login
- [ ] Statistics or impact numbers (optional)
- [ ] Responsive design

**Technical Tasks**:
1. Create `HomePage` component in `src/pages/public/`
2. Design hero section:
   - Headline and subheadline
   - CTA buttons (Browse Events, Sign Up)
3. Add "How it works" or feature highlights section
4. Add upcoming events preview (fetch 4 upcoming)
5. Add responsive styling
6. Configure as index route `/`
7. Add component tests

**Dependencies**: EVT-003, FOUND-002

**Estimate**: S

---

# Phase 4: Registrations

## REG-001: Registration API Endpoints

**Title**: Implement Event Registration Endpoints

**User Story**:
As a volunteer, I want to register and unregister from events through the API, so that I can participate in volunteer activities.

**Acceptance Criteria**:
- [ ] `POST /api/events/{id}/register` - Register for event
- [ ] `DELETE /api/events/{id}/register` - Cancel registration
- [ ] `GET /api/users/me/registrations` - Get my registrations
- [ ] `GET /api/events/{id}/registrations` - List registrations (Organizer+)
- [ ] Prevent registration if event is full
- [ ] Prevent registration for past events
- [ ] Prevent registration if past registration deadline
- [ ] Prevent registration for cancelled events
- [ ] Prevent duplicate registrations
- [ ] Returns proper status codes

**Technical Tasks**:
1. Create DTOs:
   - `RegistrationResponse` (id, event summary, status, registeredAt)
   - `EventRegistrationResponse` (id, user summary, status, registeredAt)
2. Create `IRegistrationService`:
   ```csharp
   Task<RegistrationResponse> RegisterAsync(int eventId, int userId);
   Task CancelAsync(int eventId, int userId);
   Task<List<RegistrationResponse>> GetUserRegistrationsAsync(int userId);
   Task<List<EventRegistrationResponse>> GetEventRegistrationsAsync(int eventId);
   ```
3. Implement `RegistrationService`:
   - Check event exists and is in future
   - Check event status is Active (not Cancelled)
   - Check registration deadline not passed (if set)
   - Check capacity not exceeded
   - Check not already registered
   - Create registration with Confirmed status
   - Cancel sets status to Cancelled
4. Create `RegistrationsController` with endpoints
5. Add authorization (Volunteer+ for user actions, Organizer+ for event list)
6. Add unit tests and integration tests

**Dependencies**: EVT-001

**Estimate**: L

---

## REG-002: Register for Event Functionality

**Title**: Implement Frontend Registration Flow

**User Story**:
As a volunteer, I want to register for events from the event details page, so that I can participate.

**Acceptance Criteria**:
- [ ] Register button on event details page
- [ ] Confirmation modal before registering
- [ ] Loading state during registration
- [ ] Success message after registration
- [ ] Button changes to "Cancel Registration" after registering
- [ ] Error handling (event full, already registered)
- [ ] Updates registration count in UI

**Technical Tasks**:
1. Create `useRegisterForEvent` mutation hook
2. Create `useCancelRegistration` mutation hook
3. Create `useMyRegistrations` query hook
4. Add registration button to `EventDetailsPage`:
   - Check if user is already registered
   - Show appropriate button state
5. Create confirmation modal component
6. Handle optimistic updates for better UX
7. Display error messages for failures
8. Invalidate queries after mutation
9. Add component tests

**Dependencies**: REG-001, EVT-004

**Estimate**: M

---

## REG-003: My Events Page

**Title**: Create My Registered Events Page

**User Story**:
As a volunteer, I want to see all events I've registered for, so that I can manage my participation.

**Acceptance Criteria**:
- [ ] List of events user is registered for
- [ ] Shows registration status (Confirmed/Cancelled)
- [ ] Upcoming and past events separated
- [ ] Cancel registration action for upcoming events
- [ ] Link to event details
- [ ] Empty state when no registrations
- [ ] Only accessible to authenticated users

**Technical Tasks**:
1. Create `MyEventsPage` component in `src/pages/user/`
2. Use `useMyRegistrations` hook to fetch data
3. Group events by upcoming/past
4. Create registration card component:
   - Event summary
   - Registration date
   - Status badge
   - Cancel button (for upcoming only)
5. Implement cancel functionality
6. Add empty state for no registrations
7. Configure route at `/my-events`
8. Protect with `ProtectedRoute`
9. Add component tests

**Dependencies**: REG-002

**Estimate**: M

---

## REG-004: Capacity Validation

**Title**: Implement Event Capacity Enforcement

**User Story**:
As an organizer, I want event capacity limits enforced, so that events aren't overbooked.

**Acceptance Criteria**:
- [ ] Registration rejected when event is at capacity
- [ ] Capacity count includes only Confirmed registrations
- [ ] Cancelled registrations free up spots
- [ ] Real-time capacity display on event details
- [ ] Visual indicator when event is nearly full (>80%)
- [ ] Clear "Event Full" message when capacity reached

**Technical Tasks**:
1. Update `RegistrationService` to check capacity:
   - Count confirmed registrations
   - Compare against event capacity
   - Throw exception if full
2. Update event response to include:
   - `registrationCount`
   - `availableSpots`
   - `isFull` boolean
3. Update frontend EventDetailsPage:
   - Show capacity status prominently
   - Disable register button if full
   - Show warning when nearly full
4. Handle race condition (optimistic locking or transaction)
5. Add unit tests for capacity logic

**Dependencies**: REG-001

**Estimate**: S

---

# Phase 5: Skills Feature

## SKL-001: Skills API Endpoints

**Title**: Implement Skills Management Endpoints

**User Story**:
As a user, I want to manage my skills and see event skill requirements, so that I can find relevant volunteer opportunities.

**Acceptance Criteria**:
- [ ] `GET /api/skills` - List all available skills (public)
- [ ] `GET /api/users/me/skills` - Get my skills (authenticated)
- [ ] `PUT /api/users/me/skills` - Update my skills (authenticated)
- [ ] Skills include name and category
- [ ] Update accepts array of skill IDs

**Technical Tasks**:
1. Create DTOs:
   - `SkillResponse` (id, name, category)
   - `UpdateUserSkillsRequest` (skillIds array)
2. Create `ISkillService`:
   ```csharp
   Task<List<SkillResponse>> GetAllSkillsAsync();
   Task<List<SkillResponse>> GetUserSkillsAsync(int userId);
   Task UpdateUserSkillsAsync(int userId, List<int> skillIds);
   ```
3. Implement `SkillService`
4. Create `SkillsController` with endpoints
5. Validate skill IDs exist before updating
6. Add unit tests

**Dependencies**: FOUND-004

**Estimate**: M

---

## SKL-002: User Skill Selection in Profile

**Title**: Add Skill Management to User Profile

**User Story**:
As a volunteer, I want to select my skills in my profile, so that organizers know what I can contribute.

**Acceptance Criteria**:
- [ ] Profile page shows current skills
- [ ] Skill selector allows adding/removing skills
- [ ] Skills grouped by category
- [ ] Changes saved immediately or with save button
- [ ] Success confirmation on save
- [ ] Skills shown as badges

**Technical Tasks**:
1. Create `ProfilePage` component in `src/pages/user/`
2. Create `SkillSelector` component:
   - Multi-select with checkboxes
   - Group skills by category
   - Show selected skills as badges
   - Select/deselect all per category
3. Create `useSkills` query hook (all skills)
4. Create `useUserSkills` query hook
5. Create `useUpdateUserSkills` mutation hook
6. Implement save functionality
7. Add profile info display (name, email, role)
8. Configure route at `/profile`
9. Add component tests

**Dependencies**: SKL-001, AUTH-004

**Estimate**: M

---

## SKL-003: Event Skill Requirements in Form

**Title**: Add Skill Requirements to Event Form

**User Story**:
As an organizer, I want to specify required skills for my events, so that skilled volunteers can find them.

**Acceptance Criteria**:
- [ ] Event form includes optional skill selector
- [ ] Multiple skills can be selected
- [ ] Skills displayed in create and edit forms
- [ ] Skill requirements saved with event
- [ ] Clear "no skills required" option

**Technical Tasks**:
1. Update `CreateEventRequest` DTO to include `requiredSkillIds`
2. Update `UpdateEventRequest` DTO similarly
3. Update `EventService` to handle skill associations
4. Update `EventForm` component:
   - Add SkillSelector for required skills
   - Label as "Required Skills (Optional)"
   - Show selected skills
5. Update create/edit pages to pass skill data
6. Add unit tests for skill assignment

**Dependencies**: SKL-002, EVT-006

**Estimate**: M

---

## SKL-004: Skill Badges on Events

**Title**: Display Skill Requirements on Event Cards

**User Story**:
As a volunteer, I want to see required skills on event listings, so that I can identify opportunities matching my abilities.

**Acceptance Criteria**:
- [ ] Event cards show skill badges
- [ ] Skill badges are color-coded by category
- [ ] Maximum 3 skills shown with "+N more" indicator
- [ ] Tooltip shows full skill list on hover
- [ ] Event details page shows all required skills

**Technical Tasks**:
1. Create `SkillBadge` component:
   - Display skill name
   - Color based on category
   - Consistent sizing
2. Update `EventCard` to show skill badges:
   - Show up to 3 skills
   - Add "+N more" if more exist
3. Update `EventDetailsPage`:
   - Show all required skills
   - Include skill descriptions/categories
4. Add category color mapping utility
5. Add component tests

**Dependencies**: SKL-003, EVT-003

**Estimate**: S

---

## SKL-005: Filter Events by Skills

**Title**: Add Skill Filter to Event List

**User Story**:
As a volunteer, I want to filter events by required skills, so that I can find opportunities matching my abilities.

**Acceptance Criteria**:
- [ ] Filter dropdown/checkboxes for skills
- [ ] Filter by "Events I have skills for" option
- [ ] Multiple skills can be selected
- [ ] Events matching ANY selected skill are shown
- [ ] Clear filters option
- [ ] Filter state preserved in URL (query params)

**Technical Tasks**:
1. Update `GET /api/events` to accept skill filter parameters:
   - `skillIds` - filter by specific skills
   - `matchMySkills` - filter by user's skills (requires auth)
2. Update `EventService.GetAllAsync` to apply skill filters
3. Create `EventFilters` component:
   - Skill multi-select
   - "Match my skills" checkbox (if authenticated)
   - Clear filters button
4. Update `EventListPage` to use filters:
   - Sync filters with URL query params
   - Pass filters to API hook
5. Add unit tests for filtering logic

**Dependencies**: SKL-001, EVT-003

**Estimate**: M

---

# Phase 6: Admin Features

## ADM-001: Admin Layout

**Title**: Create Admin Panel Layout

**User Story**:
As an admin, I want a dedicated admin layout with sidebar navigation, so that I can efficiently manage the platform.

**Acceptance Criteria**:
- [ ] Sidebar with admin navigation links
- [ ] Header with user info and logout
- [ ] Main content area
- [ ] Collapsible sidebar for more space
- [ ] Active link highlighting
- [ ] Responsive design (mobile-friendly)
- [ ] Only accessible to Admin role

**Technical Tasks**:
1. Create `AdminLayout` component in `src/layouts/`
2. Create `AdminSidebar` component:
   - Dashboard link
   - Users link
   - Events link
   - Reports link
3. Create `AdminHeader` component:
   - Admin title
   - User info
   - Back to main site link
4. Implement sidebar collapse toggle
5. Add responsive behavior for mobile
6. Apply layout to admin routes
7. Add component tests

**Dependencies**: AUTH-008

**Estimate**: M

---

## ADM-002: Admin Dashboard

**Title**: Create Admin Dashboard Page

**User Story**:
As an admin, I want a dashboard with key platform metrics, so that I can monitor activity at a glance.

**Acceptance Criteria**:
- [ ] Shows total users count
- [ ] Shows total events count
- [ ] Shows total registrations count
- [ ] Shows registrations this month
- [ ] Quick links to admin sections
- [ ] Recent activity feed (optional)

**Technical Tasks**:
1. Create admin stats endpoint `GET /api/admin/stats`:
   ```csharp
   class AdminStatsResponse {
     int TotalUsers;
     int TotalEvents;
     int TotalRegistrations;
     int RegistrationsThisMonth;
     int UpcomingEvents;
   }
   ```
2. Create `AdminDashboardPage` component
3. Create stats cards components
4. Create `useAdminStats` query hook
5. Add quick action links
6. Configure route at `/admin`
7. Add component tests

**Dependencies**: ADM-001

**Estimate**: M

---

## ADM-003: Admin User Management

**Title**: Create Admin User Management Page

**User Story**:
As an admin, I want to view and manage all users, so that I can control access and roles.

**Acceptance Criteria**:
- [ ] Table listing all users (including soft-deleted)
- [ ] Shows: name, email, role, created date, status (active/deleted)
- [ ] Soft-deleted users shown with visual indicator
- [ ] Pagination for large lists
- [ ] Search by name or email
- [ ] Filter by status (active/deleted)
- [ ] Change user role action
- [ ] Soft delete user action (sets IsDeleted=true)
- [ ] Confirmation modal for role changes and deletion
- [ ] Cannot change own role or delete self

**Technical Tasks**:
1. Ensure `GET /api/users` supports pagination, search, and includeDeleted
2. Create `PUT /api/users/{id}/role` endpoint
3. Create `DELETE /api/users/{id}` endpoint (soft delete)
4. Create `AdminUsersPage` component
5. Create user table with columns:
   - Name, Email, Role, Status, Created, Actions
6. Implement search and filter functionality
7. Create role change modal:
   - Role dropdown
   - Confirmation warning
8. Create soft delete confirmation modal
9. Add pagination controls
10. Create `useAdminUsers` query hook
11. Create `useUpdateUserRole` mutation hook
12. Create `useSoftDeleteUser` mutation hook
13. Add component tests

**Dependencies**: ADM-001

**Estimate**: L

---

## ADM-004: Admin Event Management

**Title**: Create Admin Event Management Page

**User Story**:
As an admin, I want to view and manage all events, so that I can moderate platform content.

**Acceptance Criteria**:
- [ ] Table listing all events (including past and soft-deleted)
- [ ] Shows: title, organizer, date, registrations, status (Active/Cancelled)
- [ ] Soft-deleted events shown with visual indicator
- [ ] Pagination and search
- [ ] Filter by status (upcoming/past/cancelled/deleted)
- [ ] Edit event action
- [ ] Soft delete event action with confirmation (sets IsDeleted=true)
- [ ] Cancel event action (sets Status=Cancelled)
- [ ] View registrations action

**Technical Tasks**:
1. Update `GET /api/events` to support:
   - `includeAll` parameter (include past events for admin)
   - `includeDeleted` parameter (admin only)
   - Search by title
   - Filter by Status
2. Create `AdminEventsPage` component
3. Create event table with columns:
   - Title, Organizer, Date, Registrations, Status, Actions
4. Implement search and filters
5. Add action buttons:
   - Edit (navigate to edit page)
   - Cancel Event (sets status to Cancelled with confirmation)
   - Soft Delete (with confirmation modal, sets IsDeleted=true)
   - View Registrations (expand or modal)
6. Create `useAdminEvents` query hook
7. Create `useSoftDeleteEvent` mutation hook
8. Create `useCancelEvent` mutation hook
9. Add pagination controls
10. Add component tests

**Dependencies**: ADM-001, EVT-002

**Estimate**: L

---

# Phase 7: Reports & Export

## RPT-001: CSV Export Service

**Title**: Implement CSV Export Service

**User Story**:
As a developer, I want a reusable CSV export service, so that admins can download data in Excel-compatible format.

**Acceptance Criteria**:
- [ ] Generic CSV generation from data collections
- [ ] Proper handling of special characters (commas, quotes)
- [ ] DateTime formatting
- [ ] UTF-8 encoding with BOM for Excel compatibility
- [ ] Memory-efficient streaming for large datasets

**Technical Tasks**:
1. Install `CsvHelper` NuGet package (or use ClosedXML for Excel)
2. Create `IExportService` interface:
   ```csharp
   Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, string[] columns);
   ```
3. Implement `CsvExportService`:
   - Configure CSV formatting
   - Handle null values
   - Format dates consistently
4. Add proper content-disposition headers helper
5. Add unit tests with sample data

**Dependencies**: FOUND-001

**Estimate**: M

---

## RPT-002: Admin Reports Page

**Title**: Create Admin Reports Page

**User Story**:
As an admin, I want a reports page where I can export platform data, so that I can analyze and share information.

**Acceptance Criteria**:
- [ ] Reports page with export options
- [ ] Export buttons for: Users, Events, Registrations, Skills Summary
- [ ] Loading state during export
- [ ] Download initiated in browser
- [ ] Date filters for registration export (optional)

**Technical Tasks**:
1. Create `AdminReportsPage` component
2. Create export sections:
   - Users Export
   - Events Export
   - Registrations Export
   - Skills Summary Export
3. Create `ExportButton` component:
   - Loading state
   - Download trigger
4. Implement file download logic:
   - Fetch blob from API
   - Create download link
   - Trigger download
5. Configure route at `/admin/reports`
6. Add component tests

**Dependencies**: ADM-001, RPT-001

**Estimate**: M

---

## RPT-003: Export API Endpoints

**Title**: Create Export Endpoints for Admin Reports

**User Story**:
As an admin, I want API endpoints that return CSV files, so that I can download platform data.

**Acceptance Criteria**:
- [ ] `GET /api/reports/users/export` - Export all users
- [ ] `GET /api/reports/events/export` - Export all events
- [ ] `GET /api/reports/registrations/export` - Export all registrations
- [ ] `GET /api/reports/skills/export` - Export skills summary
- [ ] Proper content-type and filename headers
- [ ] All endpoints require Admin role

**Technical Tasks**:
1. Create `ReportsController` with endpoints
2. Create report DTOs for CSV structure:
   - `UserExportDto` (Id, Name, Email, Role, Skills, CreatedAt)
   - `EventExportDto` (Id, Title, Date, Location, Capacity, Registrations, Organizer)
   - `RegistrationExportDto` (EventTitle, VolunteerName, VolunteerEmail, Status, RegisteredAt)
   - `SkillsSummaryDto` (SkillName, Category, VolunteerCount, EventCount)
3. Create `IReportService` with methods to generate each report
4. Implement `ReportService`:
   - Query data with includes
   - Map to export DTOs
   - Use `CsvExportService` to generate CSV
5. Return `FileContentResult` with proper headers
6. Add integration tests

**Dependencies**: RPT-001

**Estimate**: L

---

# Phase 8: Polish & Testing

## TST-001: Backend Unit Tests

**Title**: Achieve Backend Test Coverage Target

**User Story**:
As a developer, I want comprehensive unit tests for the backend, so that I can refactor with confidence.

**Acceptance Criteria**:
- [ ] Test coverage >70% for service layer
- [ ] All happy path scenarios tested
- [ ] Edge cases and error handling tested
- [ ] Mocking used for dependencies
- [ ] Tests run in CI pipeline

**Technical Tasks**:
1. Review existing tests and identify gaps
2. Add tests for `AuthService`:
   - Registration validation
   - Login success/failure
   - Password hashing
3. Add tests for `EventService`:
   - CRUD operations
   - Ownership validation
   - Date validation
4. Add tests for `RegistrationService`:
   - Registration flow
   - Capacity enforcement
   - Duplicate prevention
5. Add tests for `SkillService`
6. Add tests for `ReportService`
7. Add tests for validators
8. Configure code coverage reporting

**Dependencies**: All backend features

**Estimate**: L

---

## TST-002: Frontend Component Tests

**Title**: Achieve Frontend Test Coverage Target

**User Story**:
As a developer, I want comprehensive component tests, so that I can maintain UI quality.

**Acceptance Criteria**:
- [ ] Test coverage >70% for components
- [ ] All user interactions tested
- [ ] Loading and error states tested
- [ ] Form validation tested
- [ ] Tests run in CI pipeline

**Technical Tasks**:
1. Set up Vitest with React Testing Library
2. Add tests for common components:
   - Button, Input, Card, Modal
   - SkillBadge, SkillSelector
   - LoadingSpinner, ErrorMessage
3. Add tests for auth pages:
   - LoginPage form submission
   - RegisterPage validation
4. Add tests for event components:
   - EventCard rendering
   - EventForm validation
5. Add tests for hooks:
   - useAuth behavior
   - Data fetching hooks
6. Configure code coverage reporting

**Dependencies**: All frontend features

**Estimate**: L

---

## TST-003: Integration Tests

**Title**: Add End-to-End API Integration Tests

**User Story**:
As a developer, I want integration tests for critical flows, so that I can verify the system works end-to-end.

**Acceptance Criteria**:
- [ ] Auth flow tested (register, login, access protected endpoint)
- [ ] Event CRUD flow tested
- [ ] Registration flow tested
- [ ] Admin operations tested
- [ ] Uses test database

**Technical Tasks**:
1. Set up integration test project with WebApplicationFactory
2. Configure test database (PostgreSQL or in-memory)
3. Create test fixtures and helpers
4. Add integration tests:
   - Complete auth flow
   - Event creation and retrieval
   - Registration with capacity check
   - Admin role operations
5. Add to CI pipeline

**Dependencies**: TST-001

**Estimate**: M

---

## POL-001: Error Handling Improvements

**Title**: Implement Consistent Error Handling

**User Story**:
As a user, I want clear error messages when things go wrong, so that I understand what happened.

**Acceptance Criteria**:
- [ ] Consistent error response format from API
- [ ] User-friendly error messages in UI
- [ ] Network error handling
- [ ] 404 pages for missing resources
- [ ] Global error boundary in React

**Technical Tasks**:
1. Create standard error response DTO:
   ```csharp
   class ApiError {
     string Message;
     string Code;
     Dictionary<string, string[]> Errors;
   }
   ```
2. Create exception middleware for consistent formatting
3. Create custom exceptions (NotFoundException, ValidationException)
4. Create React error boundary component
5. Create 404 page component
6. Improve error display in forms
7. Add error toast notifications

**Dependencies**: All features

**Estimate**: M

---

## POL-002: UI Polish and Responsive Design

**Title**: Improve UI Design and Mobile Experience

**User Story**:
As a user, I want an attractive and mobile-friendly interface, so that I can use the platform on any device.

**Acceptance Criteria**:
- [ ] Consistent styling across all pages
- [ ] Mobile-responsive layouts
- [ ] Touch-friendly buttons and inputs
- [ ] Loading skeletons instead of spinners
- [ ] Smooth transitions and animations
- [ ] Accessible color contrast

**Technical Tasks**:
1. Audit all pages for mobile responsiveness
2. Fix layout issues on small screens
3. Add CSS transitions for better UX
4. Replace spinners with loading skeletons
5. Improve button and input sizing for touch
6. Verify color contrast for accessibility
7. Add hover states and focus indicators
8. Test on various screen sizes

**Dependencies**: All UI features

**Estimate**: M

---

## POL-003: Documentation

**Title**: Create Developer Documentation

**User Story**:
As a developer, I want documentation for setting up and understanding the project, so that I can onboard quickly.

**Acceptance Criteria**:
- [ ] README with setup instructions
- [ ] API documentation (Swagger)
- [ ] Frontend component documentation
- [ ] Environment variable documentation
- [ ] Database migration guide

**Technical Tasks**:
1. Update root README with:
   - Project overview
   - Tech stack
   - Setup instructions
   - Running locally
   - Running tests
2. Ensure Swagger docs are complete:
   - All endpoints documented
   - Request/response examples
   - Authentication explained
3. Create backend README
4. Create frontend README
5. Document environment variables
6. Add architecture decision records (optional)

**Dependencies**: All features

**Estimate**: S

---

# Summary

## Story Count by Phase

| Phase | Stories | Priority |
|-------|---------|----------|
| Phase 1: Foundation | 5 | High |
| Phase 2: Authentication | 8 | High |
| Phase 3: Events Core | 8 | High |
| Phase 4: Registrations | 4 | High |
| Phase 5: Skills | 5 | Medium |
| Phase 6: Admin | 4 | Medium |
| Phase 7: Reports | 3 | Medium |
| Phase 8: Polish & Testing | 6 | Medium |
| **Total** | **43** | |

## Recommended Sprint Breakdown

**Sprint 1 (Foundation + Auth Start)**:
- FOUND-001 to FOUND-005
- AUTH-001 to AUTH-003

**Sprint 2 (Auth Complete + Events Start)**:
- AUTH-004 to AUTH-008
- EVT-001, EVT-002

**Sprint 3 (Events Core)**:
- EVT-003 to EVT-008

**Sprint 4 (Registrations + Skills Start)**:
- REG-001 to REG-004
- SKL-001, SKL-002

**Sprint 5 (Skills Complete + Admin)**:
- SKL-003 to SKL-005
- ADM-001 to ADM-004

**Sprint 6 (Reports + Polish)**:
- RPT-001 to RPT-003
- TST-001 to TST-003
- POL-001 to POL-003
