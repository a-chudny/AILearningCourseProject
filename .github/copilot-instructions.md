# GitHub Copilot Instructions - Volunteer Event Portal

## Project Overview
A full-stack learning project for GitHub Copilot AI training course demonstrating maximum AI-assisted code generation.

- **Backend**: ASP.NET Core 9 REST API
- **Frontend**: React 19+ with TypeScript
- **Goal**: Achieve ~90% AI-generated code through effective prompting

## AI-First Development Principles

### Maximize Code Generation
- Generate complete files, not snippets
- Include all imports, types, and boilerplate automatically
- Generate tests alongside implementations
- Create documentation as part of code generation
- Suggest related files that need to be created or modified

### Prompt Engineering for Developers
- Interpret brief descriptions and expand into full implementations
- Ask clarifying questions only when truly ambiguous
- Infer patterns from existing codebase and apply consistently
- Generate production-ready code, not placeholder examples

### Code Generation Standards
- Always generate complete, runnable code
- Include error handling and edge cases by default
- Add inline comments for complex logic
- Generate TypeScript types and C# models automatically
- Create unit tests with each new feature

## Technology Stack

### Backend (.NET)
- ASP.NET Core 9 Web API
- Entity Framework Core with SQL Server
- JWT Authentication
- Minimal APIs preferred for new endpoints
- xUnit for testing

### Frontend (React)
- React 19+ with TypeScript (strict mode)
- Vite as build tool
- React Query for server state
- React Router v6
- Vitest + React Testing Library

## Code Generation Patterns

### When Creating Backend Endpoints
Generate in this order:
1. Domain model/entity with validation attributes
2. DTO classes (request/response)
3. Service interface and implementation
4. Repository interface and implementation (if needed)
5. Controller or Minimal API endpoint
6. Unit tests for service layer
7. Integration tests for API endpoint

### When Creating Frontend Features
Generate in this order:
1. TypeScript types/interfaces matching API contracts
2. API service functions with proper error handling
3. Custom hooks for data fetching (useQuery/useMutation)
4. Presentational components with props interfaces
5. Container/page components with state management
6. Component tests

### When Creating Full Features
Generate both backend and frontend together:
1. Start with API contract (OpenAPI-style types)
2. Generate backend implementation
3. Generate frontend types from API contract
4. Generate frontend components and hooks
5. Generate tests for both layers

## File Templates

### Backend Entity Pattern
```csharp
// Include: validation, navigation properties, audit fields
public class Entity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Frontend Component Pattern
```typescript
// Include: props interface, proper hooks, error/loading states
interface ComponentProps {
  // typed props
}

export function Component({ ...props }: ComponentProps) {
  // implementation with hooks
}
```

## Generation Commands

### Quick Patterns (Use These Phrases)
- "Create CRUD for [entity]" → Full backend + frontend implementation
- "Add endpoint for [action]" → Controller/Minimal API + service + tests
- "Create component for [feature]" → React component + types + tests
- "Add form for [entity]" → Form component + validation + API integration
- "Create hook for [feature]" → Custom hook with proper typing

### Quick Workflow Logging Commands
- "Log it" → Add current work to workflow log at `Docs/ai-workflow-log.md`
- "Log this step" → Add detailed entry to workflow log
- "Update workflow log" → Add entry for completed work with all details

### Expected AI Behavior
When asked to create a feature:
1. Generate ALL necessary files without being asked
2. Follow existing patterns in the codebase
3. Include proper error handling
4. Add TypeScript types / C# models
5. Generate basic tests
6. Update related files (routes, exports, etc.)

## Project Structure

```
backend/
├── src/
│   ├── API/              # Controllers, Minimal APIs, Middleware
│   ├── Application/      # Services, DTOs, Interfaces
│   ├── Domain/           # Entities, Enums, Value Objects
│   └── Infrastructure/   # DbContext, Repositories, External Services
└── tests/

frontend/
├── src/
│   ├── components/       # Reusable UI components
│   ├── pages/            # Route page components
│   ├── hooks/            # Custom React hooks
│   ├── services/         # API client functions
│   ├── types/            # TypeScript type definitions
│   └── utils/            # Helper functions
└── tests/
```

## Domain: Volunteer Event Portal

### Core Entities
- **Event**: id, title, description, date, location, capacity, organizerId
- **User**: id, email, name, role (Volunteer/Organizer), passwordHash
- **Registration**: id, eventId, userId, status, registeredAt
- **Organization**: id, name, description, contactEmail

### Common Features to Generate
- User authentication (register, login, JWT)
- Event CRUD operations
- Volunteer registration for events
- Event search and filtering
- User profile management
- Dashboard views

## Quality Standards

### Always Include
- Input validation (backend + frontend)
- Error handling with user-friendly messages
- Loading states for async operations
- Proper HTTP status codes
- TypeScript strict mode compliance
- Async/await patterns

### Security by Default
- Parameterized queries (EF Core handles this)
- JWT token validation
- Input sanitization
- CORS configuration
- Authorization attributes on protected endpoints

## Testing Generation

### Backend Tests
- Unit tests for each service method
- Integration tests for API endpoints
- Use mocking for dependencies
- Test both success and error cases

### Frontend Tests
- Component rendering tests
- User interaction tests
- API integration tests with MSW
- Hook testing with renderHook

## Workflow for Maximum AI Generation

### Step 1: Describe the Feature
"Create a complete event management feature with CRUD operations"

### Step 2: AI Generates
- Backend: Entity, DTOs, Service, Controller, Tests
- Frontend: Types, API service, Hooks, Components, Tests
- Documentation: API docs, README updates

### Step 3: Review and Refine
- Run generated tests
- Check type safety
- Verify API contracts match
- Request adjustments if needed

### Step 4: Iterate
"Add pagination to event list"
"Add image upload to event creation"
"Add email notification on registration"

## Development Workflow Logging

### Log Each Significant Development Step
Document every major development activity in `ai-workflow-log.md` located in the /Docs folder. This creates a learning trail and helps track AI-generated code progress.

### CRITICAL: Always Add New Entries at the END of the File
**⚠️ IMPORTANT:** New workflow log entries must ALWAYS be appended to the END of the `Docs/ai-workflow-log.md` file, NOT at the beginning. This maintains chronological order with the oldest entries at the top and newest at the bottom.

### What Qualifies as "Significant"
- New feature implementations
- Major refactoring efforts
- Bug fixes that required investigation
- Architecture decisions
- Integration of new libraries or tools
- Database migrations
- API endpoint additions
- New component creations

### Workflow Log Entry Structure
Each entry should follow this standardized format:

```markdown
## [YYYY-MM-DD HH:MM] - Feature/Task Name

### Prompt
The exact or paraphrased prompt/request given to the AI assistant.

### Context
- Current state of the project
- Related existing files or features
- Why this task was needed
- Dependencies or prerequisites

### Files Added/Modified
- `path/to/file1.cs` - Created: Brief description
- `path/to/file2.tsx` - Modified: What changed
- `path/to/file3.test.ts` - Created: Test coverage added

### Generated Code Summary
- Brief overview of what code was generated
- Key components, services, or utilities created
- Patterns or technologies used

### Result
- ✅ Success / ⚠️ Partial / ❌ Failed
- What works as expected
- Any issues encountered
- Manual adjustments needed (if any)

### AI Generation Percentage
Estimate: ~X% (e.g., ~85% generated, ~15% manual adjustments)

### Learnings/Notes
- Insights gained during implementation
- What prompts worked well
- What could be improved
- Unexpected behaviors or edge cases discovered

---
```

### Logging Guidelines

1. **Log Immediately**: Document right after completing the work while context is fresh
2. **Be Honest**: Accurately report AI vs manual code percentages
3. **Include Failures**: Failed attempts are valuable learning opportunities
4. **Link Files**: Use relative paths for easy navigation
5. **Quantify Generation**: Track percentage of AI-generated code per task
6. **Note Prompt Effectiveness**: Record which prompts produced best results
7. **Use Quick Commands**: Simply say "Log it" or "Log this step" to trigger logging
8. **⚠️ APPEND TO END**: Always add new entries at the END of the file, maintaining chronological order (oldest→newest)

### Benefits of Workflow Logging
- Track progress toward 90% AI-generation goal
- Identify which prompts work best
- Create reusable prompt patterns
- Document learning journey for course content
- Analyze AI strengths and limitations
- Share knowledge with other learners

### Example Log Entry

```markdown
## [2026-01-31 14:30] - Event CRUD API Implementation

### Prompt
"Create complete CRUD operations for Event entity including domain model, DTOs, service layer, minimal API endpoints, and unit tests. Event should have: id, title, description, date, location, capacity, organizerId."

### Context
- Starting backend implementation for volunteer portal
- No existing event-related code
- Following clean architecture pattern
- Using Minimal APIs for endpoints

### Files Added/Modified
- `backend/src/Domain/Entities/Event.cs` - Created: Event entity with validation
- `backend/src/Application/DTOs/EventDto.cs` - Created: Request/response DTOs
- `backend/src/Application/Services/EventService.cs` - Created: Business logic
- `backend/src/Application/Services/IEventService.cs` - Created: Service interface
- `backend/src/API/Endpoints/EventEndpoints.cs` - Created: Minimal API endpoints
- `backend/tests/Application.Tests/EventServiceTests.cs` - Created: 12 unit tests

### Generated Code Summary
- Complete Event entity with EF Core navigation properties
- CreateEventDto, UpdateEventDto, EventResponseDto
- Full CRUD service with validation and error handling
- 5 RESTful endpoints (GET all, GET by id, POST, PUT, DELETE)
- Comprehensive unit tests with Moq

### Result
✅ Success
- All endpoints working correctly
- Tests passing (12/12)
- Proper validation and error messages
- Manual adjustment: Added custom date validation (5 lines)

### AI Generation Percentage
Estimate: ~92% (AI generated ~210 lines, manual added ~18 lines)

### Learnings/Notes
- Detailed prompt with entity properties produced excellent results
- AI correctly inferred navigation properties
- Generated tests covered edge cases automatically
- Needed to manually add business rule: events can't be in the past
- Prompt pattern "Create complete [feature] including [list]" is highly effective

---
```

## Tips for 90% AI-Generated Code

1. **Be Specific**: "Create Event entity with title, description, date, location, capacity, and organizer relationship"

2. **Request Complete Files**: "Generate the complete EventService.cs file"

3. **Chain Requests**: "Now create the React component that uses this API"

4. **Include Context**: "Following the pattern in UserService, create EventService"

5. **Ask for Tests**: "Include unit tests for this service"

6. **Request Updates**: "Update the API routes to include this new endpoint"

7. **Generate in Batches**: "Create all DTOs needed for the Event feature"

## Response Format Preferences

### For Code Generation
- Provide complete, copy-paste ready code
- Include file paths as comments
- Show all imports and dependencies
- Add brief inline comments for complex logic

### For Explanations
- Keep explanations concise
- Focus on "why" for architectural decisions
- Link to relevant docs only when helpful

### For Debugging
- Analyze the full error context
- Suggest specific fixes with code
- Explain the root cause briefly

---

**Remember**: This project aims to demonstrate that ~90% of code can be AI-generated with proper prompting. Focus on generating complete, production-quality code rather than examples or placeholders.
