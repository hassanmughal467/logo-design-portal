\# Logo Design Portal



\## Project Purpose



Logo Design Portal is an internal administration and order-management system for a logo design agency.



Current business areas include clients, designers, users, orders, projects, invoices, payments, payouts, financial operations, analytics, client intelligence, notifications, messages, gallery, quotes, reviews, settings, and permissions.



The long-term direction is a broader business automation platform that may include CRM, lead management, lead scraping, email marketing, sales automation, QA automation, reporting, and deployment automation.



\## Verified Technology Stack



\### Frontend



\- Angular 15.2.10

\- TypeScript

\- PrimeNG 15.4

\- RxJS 7.5

\- ApexCharts and ng-apexcharts

\- Chart.js

\- Microsoft SignalR client

\- Karma and Jasmine

\- Playwright

\- Stryker mutation testing



\### Backend



\- ASP.NET Core 8

\- C#

\- Clean Architecture

\- Entity Framework Core 8

\- Pomelo.EntityFrameworkCore.MySql 8.0

\- MySQL for application runtime

\- SQLite for integration tests only

\- JWT Bearer authentication

\- SignalR

\- Hangfire

\- Redis

\- Serilog

\- OpenTelemetry

\- Azure Monitor exporter

\- Polly.Extensions

\- Health checks

\- xUnit

\- Stryker mutation testing



\### Continuous Integration



\- GitHub Actions

\- `.github/workflows/test.yml`

\- `.github/workflows/e2e-playwright.yml`



\## Verified Repository Structure



```text

Web Portal/

├── Backend/

│   ├── LogoDesignPortal.sln

│   └── src/

│       ├── LogoDesignPortal.Domain/

│       ├── LogoDesignPortal.Application/

│       ├── LogoDesignPortal.Infrastructure/

│       ├── LogoDesignPortal.API/

│       ├── LogoDesignPortal.Domain.Tests/

│       ├── LogoDesignPortal.Application.Tests/

│       └── LogoDesignPortal.API.IntegrationTests/

├── Frontend/

│   └── src/app/

├── docs/

├── .github/workflows/

└── CLAUDE.md
## Backend Architecture

The backend uses Clean Architecture with the following dependency direction:

```text
Domain
   ↑
Application
   ↑
Infrastructure
   ↑
API
```

More precisely:

- Domain contains entities, enums, value objects, and domain rules.
- Application contains interfaces, DTOs, validators, use cases, and business logic.
- Infrastructure contains Entity Framework Core, MySQL, Redis, email, caching, background jobs, and external-service implementations.
- API contains controllers, middleware, dependency registration, configuration, authentication, authorization, SignalR hubs, and application startup.
- Infrastructure implements interfaces defined by Application.
- Domain must remain independent of every other layer.

## Frontend Architecture

The Angular frontend uses lazy-loaded feature modules and a shared application shell.

Main modules include:

- auth
- dashboard
- orders
- users
- designers
- clients
- invoices
- financial
- analytics
- client-intelligence
- notifications
- messages
- gallery
- quotes
- reviews
- settings
- permissions
- projects
- payments
- core
- shared
- layout

`MainLayoutComponent` is the shared shell and should not be unnecessarily recreated during normal feature navigation.

Preserve existing Angular module, routing, shared-component, core-service, and lazy-loading patterns.

## Runtime Services

### MySQL

- MySQL is the main runtime database.
- Local development currently expects MySQL at `127.0.0.1:3306`.
- MySQL is an external dependency and is not currently started by repository tooling.

### Redis

- Redis is optional.
- Existing functionality should degrade gracefully when Redis is unavailable.
- Redis may support caching, rate limiting, Hangfire, and SignalR scaling.
- Do not make Redis mandatory unless explicitly required.

### SignalR

- Notification hub endpoint: `/hubs/notifications`.
- SignalR may optionally scale through `SignalR.StackExchangeRedis`.
- Preserve authentication, authorization, connection, reconnection, and user-isolation behavior.

### Hangfire

- Hangfire uses Redis where configured.
- An in-memory fallback exists.
- Background jobs should be idempotent where practical.
- Avoid duplicate job execution and duplicate business processing.

### Observability

- Serilog is used for structured logging.
- OpenTelemetry is configured with console and OTLP exporters.
- Azure Monitor export may be enabled through environment configuration.
- Health checks are available.
- Never log credentials, access tokens, connection strings, or private customer data.

## Standard Development Commands

Commands are normally executed from the repository root unless otherwise specified.

### Run Backend

```bash
cd Backend/src/LogoDesignPortal.API
dotnet run
```

Expected local URL:

```text
http://localhost:5000
```

Swagger:

```text
http://localhost:5000/swagger
```

### Run Frontend

```bash
cd Frontend
npm install
npm start
```

Expected local URL:

```text
http://localhost:4200
```

### Backend Build

```bash
dotnet build Backend/LogoDesignPortal.sln
```

### Backend Tests

```bash
dotnet test Backend/LogoDesignPortal.sln
```

### Frontend Unit Tests

```bash
npm run test:ci --prefix Frontend
```

### Playwright End-to-End Tests

```bash
npm run e2e --prefix Frontend
```

Do not assume that MySQL or Redis is running. Check required services before classifying a startup or integration failure as a code defect.

## Architecture Rules

- Preserve Clean Architecture boundaries.
- Domain must not depend on Application, Infrastructure, or API.
- Application must not depend on Infrastructure or API.
- Infrastructure must implement interfaces defined by Application.
- Keep API controllers thin.
- Put application use cases and business logic in the Application layer.
- Put domain rules in the Domain layer.
- Keep infrastructure concerns in the Infrastructure layer.
- Do not place business logic inside Angular components.
- Use dependency injection.
- Follow existing project patterns before adding abstractions.
- Follow SOLID principles.
- Avoid duplicate logic.
- Avoid unnecessary abstractions.
- Do not create duplicate DTOs, services, validators, helpers, or components.

## Required Workflow Before Editing

Before changing any code:

1. Check `git status`.
2. Read the relevant implementation.
3. Search for similar functionality.
4. Locate related services, interfaces, DTOs, validators, entities, controllers, components, routes, guards, and tests.
5. Identify frontend, backend, database, authentication, authorization, cache, SignalR, Hangfire, and regression impact.
6. Present a concise implementation plan.
7. Modify only files necessary for the task.
8. Preserve unrelated local changes.

Do not scan the entire repository when targeted inspection is sufficient.

## Required Workflow During Editing

- Make the smallest safe change.
- Follow existing naming and formatting conventions.
- Preserve public behavior unless a behavior change is explicitly required.
- Validate user-controlled input.
- Preserve authentication and authorization controls.
- Preserve error handling and structured logging.
- Do not suppress exceptions silently.
- Do not weaken or skip tests.
- Do not expose credentials or secrets.
- Do not add placeholder implementations unless explicitly requested.
- Use asynchronous APIs correctly.
- Use cancellation tokens where existing project patterns require them.
- Do not modify unrelated files.
- Do not perform broad refactoring during a focused bug fix.

## Required Workflow After Editing

1. Review the relevant diff.
2. Build the affected project.
3. Run the smallest relevant test set first.
4. Run broader regression tests when shared behavior is affected.
5. Check browser console and network failures for frontend changes.
6. Report files changed.
7. Report commands executed.
8. Report passing tests.
9. Report failing tests.
10. Report skipped or not-run tests.
11. Report assumptions and unresolved risks.
12. Never claim success without verification evidence.

## Backend Coding Rules

- Keep controllers thin.
- Use Application services, handlers, or existing use-case patterns.
- Use DTOs for request and response boundaries.
- Do not expose database entities directly unless the existing endpoint intentionally follows that pattern.
- Preserve existing response formats.
- Use appropriate HTTP status codes.
- Validate input before processing.
- Use async Entity Framework Core APIs.
- Avoid unnecessary queries.
- Avoid N+1 queries.
- Review pagination, filtering, sorting, reporting, dashboard, and analytics query performance.
- Use structured logging.
- Do not log sensitive information.
- Handle expected failures explicitly.
- Do not catch exceptions without meaningful handling or appropriate rethrowing.
- Preserve role, ownership, and data-access boundaries.

## Frontend Coding Rules

- Preserve Angular lazy loading.
- Keep components focused on presentation and user interaction.
- Put reusable logic in Angular services.
- Put reusable UI in shared components.
- Use strongly typed models and interfaces.
- Avoid `any` unless unavoidable and explained.
- Handle loading, success, empty, and error states.
- Handle API failures visibly.
- Preserve route guards and role-based visibility.
- Do not rely on frontend guards as the only security control.
- Clean up subscriptions where required.
- Follow existing RxJS patterns.
- Avoid duplicate API calls.
- Preserve responsive behavior.
- Check browser console errors.
- Check failed network requests.

## Authentication and Authorization Rules

- Never weaken JWT validation.
- Never bypass authorization for convenience.
- Backend authorization is the security source of truth.
- Verify unauthenticated requests.
- Verify unauthorized and forbidden behavior.
- Verify every affected role, not only super-admin.
- Do not expose restricted fields through API responses.
- Preserve token-expiration and refresh behavior.
- Do not place passwords or active credentials in `CLAUDE.md`, reports, source files, logs, or screenshots.

## Database Rules

- Never modify production connection strings.
- Never execute destructive database operations without explicit approval.
- Never drop or rename tables, columns, indexes, constraints, or persisted fields without reviewing migration and compatibility impact.
- Review migrations before applying them.
- Do not automatically apply migrations to production.
- Preserve data integrity.
- Use transactions for appropriate multi-step business operations.
- Use isolated test data.
- Never use real customer data in automated tests.
- Do not expose customer data through logs, screenshots, traces, or QA reports.

## Testing Rules

Every bug fix should include a regression test when practical.

For a bug fix:

1. Reproduce the defect.
2. Record expected behavior.
3. Record actual behavior.
4. Identify the root cause.
5. Add or update a focused regression test when practical.
6. Confirm the test exposes the original defect where practical.
7. Apply the smallest safe fix.
8. Run the focused test.
9. Run related regression tests.
10. Review the diff.

Never delete, disable, weaken, or skip a test merely to make the suite pass.

### API Testing Coverage

Consider:

- Successful requests
- Validation failures
- Missing required fields
- Invalid identifiers
- Missing resources
- Duplicate records
- Unauthenticated requests
- Forbidden requests
- Role-based access
- Pagination
- Filtering
- Sorting
- Concurrency
- Error responses

### UI Testing Coverage

Consider:

- Page loading
- Loading states
- Empty states
- Error states
- Form validation
- API failure
- Create flows
- Edit flows
- Delete flows
- Confirmation dialogs
- Search
- Filtering
- Sorting
- Pagination
- Navigation
- Browser refresh
- File upload
- File download
- Role-based visibility
- Unauthorized navigation
- Responsive layout
- Browser console errors
- Failed network requests

### Playwright Rules

- Use stable selectors.
- Prefer role, label, test ID, or other reliable selectors.
- Avoid unnecessary hard-coded waits.
- Wait for real UI or network conditions.
- Use isolated test data.
- Do not depend on production records.
- Capture trace, screenshot, or relevant diagnostics on failure where configured.
- Cover happy paths, negative paths, and authorization paths.

## QA-Only Workflow

When asked only to test or audit:

1. Do not modify application code.
2. Read relevant requirements and existing tests.
3. Identify complete user flows.
4. Identify supported roles.
5. Identify UI pages and components.
6. Identify API endpoints.
7. Identify database effects.
8. Identify SignalR, Hangfire, and Redis impact.
9. Define happy, negative, boundary, and authorization scenarios.
10. Execute tests.
11. Separate product defects from environment failures.
12. Produce structured findings.
13. Do not fix defects unless explicitly instructed.

## Bug Investigation Workflow

When asked to investigate:

1. Reproduce the issue.
2. Record exact steps.
3. Record expected behavior.
4. Record actual behavior.
5. Inspect console and network activity for frontend issues.
6. Inspect API logs and stack traces for backend issues.
7. Trace the flow through Angular, API, Application, Infrastructure, and database layers.
8. Identify root cause.
9. Identify affected modules and regression risk.
10. Propose the smallest safe fix.
11. Do not modify code unless fixing is explicitly included in the task.

## Bug Fixing Workflow

When asked to fix a bug:

1. Read the bug report.
2. Reproduce the issue.
3. Confirm the root cause.
4. Add or update a regression test when practical.
5. Apply the smallest safe fix.
6. Build the affected project.
7. Run focused tests.
8. Run related regression tests.
9. Review the diff.
10. Report verification evidence.

## Code Review Workflow

When asked to review code, check:

- Correctness
- Security
- Authorization
- Validation
- Data integrity
- Architecture violations
- Performance
- Concurrency
- Error handling
- Logging
- Test coverage
- Maintainability
- Duplicate logic
- Backward compatibility
- Frontend usability
- Accessibility
- Responsive behavior

Report findings by severity:

- Critical
- High
- Medium
- Low
- Informational

Each finding should include:

- File and location
- Problem
- Risk
- Evidence
- Recommended fix

Do not modify code during a review unless explicitly instructed.

## Git Rules

- Check `git status` before editing.
- Preserve unrelated local changes.
- Do not commit unless explicitly instructed.
- Do not push unless explicitly instructed.
- Do not force push.
- Do not rewrite history.
- Do not delete branches without explicit approval.
- Review the relevant diff before reporting completion.

## Branch Promotion and Deployment Safety

Rules:

1. The required promotion path is:
   feature branch → develop → staging-environment → main

2. Never suggest, create, approve, merge, or execute a pull request into staging-environment or main unless the user explicitly asks for that exact promotion in the current conversation.

3. Never treat "next", "proceed", "continue", "merge it", or similar general approval as authorization to promote into staging-environment or main.

4. Promotion into staging-environment requires the user to write an explicit statement equivalent to:
   "Approve creating the develop to staging-environment PR."

5. Merging into staging-environment requires a separate explicit statement equivalent to:
   "Approve merging PR #<number> into staging-environment."

6. Promotion into main requires the user to write an explicit statement equivalent to:
   "Approve creating the staging-environment to main PR."

7. Merging into main requires a separate explicit statement equivalent to:
   "Approve merging PR #<number> into main."

8. Deployment is a separate operation from merging. Never deploy automatically after a merge.

9. A staging deployment requires a separate explicit statement:
   "Approve deploying staging."

10. A production/live deployment requires a separate explicit statement:
    "Approve deploying production."

11. Before suggesting any staging or production promotion, run the appropriate release review:
    - /release-check before develop → staging-environment
    - /qa-full before staging-environment → main

12. Do not recommend promotion while:
    - tests are pending;
    - required tests failed;
    - the working tree is dirty;
    - commits are not pushed;
    - unresolved review findings exist;
    - staging URLs or production settings are unconfirmed;
    - deployment rollback steps are unverified.

13. A known failing test may only be accepted through a written exception approved by the user. Never assume an earlier exception applies to a new PR.

14. Always stop after:
    - creating a promotion PR;
    - receiving final CI results;
    - merging a promotion PR;
    - completing a deployment.

15. Never combine PR creation, merge, and deployment into one stage.

16. Do not modify staging-environment or main branch pointers directly. Use pull requests only.

17. If the user requests a merge without naming the target branch, inspect the PR target first. If the target is staging-environment or main, stop and request explicit promotion approval.

## Deployment Rules

- Do not deploy unless explicitly instructed.
- Do not modify production settings without approval.
- Verify environment variables.
- Verify connection strings are not placeholders.
- Verify migrations.
- Verify build output.
- Verify health checks.
- Verify authentication and authorization.
- Verify Redis fallback.
- Verify Hangfire.
- Verify SignalR.
- Verify logging and monitoring.
- Verify rollback steps.
- Generate release notes when instructed.

## Existing Repository Organization Findings

The following are known findings, not approved changes:

- Approximately 50 Markdown documents exist at the repository root.
- Several quick-start, testing, audit, and deployment documents may overlap.
- A database backup file exists at the repository root.
- No `docker-compose.yml` was found.
- Root `package.json` is currently a thin mutation-testing orchestration file.
- PowerShell deployment and setup scripts exist at the repository root.
- Some audit reports may be historical or superseded.

Do not move, delete, consolidate, rename, archive, or reorganize these files unless explicitly instructed.

Do not remove the database backup from source control unless explicitly instructed and its retention requirements have been confirmed.

## Dependency and Environment Findings

- MySQL is an external runtime dependency.
- Redis is an optional external runtime dependency.
- No repository-managed Docker Compose environment was found.
- Production configuration requires real connection strings.
- Production startup may reject placeholder connection strings.
- No local Husky or lint-staged commit gate was identified.
- CI is currently the main automated quality gate.

Treat these as documented findings, not automatically approved implementation tasks.

## Documentation Rules

- Search existing documentation before creating another document.
- Prefer updating the most relevant existing document.
- Avoid creating additional overlapping quick-start or audit files.
- Clearly mark dates, assumptions, unresolved issues, and verification status.
- Do not treat an old audit report as current truth without comparing it to the code.

## Context and Token Efficiency

- Keep each session focused on one objective.
- Use targeted search before opening files.
- Read only files relevant to the current task.
- Avoid repeating full file contents in conversation.
- Summarize large command output.
- Report failures and warnings first.
- Store detailed reports in repository files only when instructed.
- Do not repeatedly read unchanged files.
- Start with focused tests before full regression.
- Use `/clear` when moving to an unrelated task.
- Use `/compact` when continuing a long session and context becomes large.

## Actions Requiring Explicit Approval

Explicit approval is required before:

- Destructive database operations
- Production migrations
- Deployment
- Production configuration changes
- Deleting or moving files
- Removing repository documentation
- Removing the database backup
- Broad repository reorganization
- Rewriting Git history
- Force pushing
- Disabling security controls
- Removing or weakening tests
- Broad automated refactoring
- Authentication architecture changes
- Authorization architecture changes
- Running scripts against external systems
- Sending real emails or messages
- Running live lead scraping
- Starting paid external services

## Communication Rules

- Be concise and evidence-based.
- Separate confirmed facts from assumptions.
- Report exact blockers.
- Report exact commands executed.
- Report files changed.
- Report tests executed.
- Report tests not executed.
- Report unresolved risks.
- Do not claim verification that was not performed.
- Ask for approval before destructive, production, database-wide, deployment, security-sensitive, or external-system actions.
