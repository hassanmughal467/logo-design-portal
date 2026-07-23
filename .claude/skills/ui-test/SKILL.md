# UI Test Skill

## Purpose

Run focused Playwright-based browser testing for the Logo Design Portal.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not commit or push.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Test only the requested flow.
- Do not run the full Playwright suite unless explicitly instructed.
- Use existing Playwright configuration and test patterns.
- Separate application defects from environment failures.
- Do not fix defects unless explicitly instructed.

## Preconditions

Before browser testing:

1. Check git status.
2. Confirm the backend project is:
   - Backend/src/LogoDesignPortal.API
3. Confirm backend availability:
   - https://localhost:5001
4. Confirm frontend availability:
   - http://localhost:4200
5. Confirm required database services are available.
6. Confirm the requested test account and role.
7. Do not start or stop production services.
8. Do not use real customer data.

If the backend or frontend is unavailable, report the exact blocker instead of classifying the flow as an application defect.

## Local Environment Startup Rules

- Do not start or stop local backend or frontend services without explicit approval.
- Never start services from `Backend/LogoDesignPortal.API`; that is a stale duplicate project.
- The correct backend project is:
  - `Backend/src/LogoDesignPortal.API`
- Preferred backend startup command:
  - `dotnet run --launch-profile https`
- Expected backend URLs:
  - `https://localhost:5001`
  - `http://localhost:5000`
- Do not use `--no-launch-profile` unless the required environment variables are explicitly supplied.
- If `--no-launch-profile` is used for a local Development run, ensure:
  - `ASPNETCORE_ENVIRONMENT=Development`
- Preferred frontend startup command:
  - `npm start --prefix Frontend`
- Expected frontend URL:
  - `http://localhost:4200`
- After testing, stop only services that were started by the current test run and only when approval was given.
- Never stop services that were already running before the test began.

## Test Process

1. Identify the requested user flow.
2. Identify supported roles.
3. Locate existing Playwright tests for the same module.
4. Locate relevant Angular routes, components, services, guards, and API endpoints.
5. Define:
   - Happy path
   - Negative path
   - Validation path
   - Authorization path
   - Empty state
   - Error state
6. Run the smallest relevant Playwright test or test group.
7. Inspect:
   - Browser console errors
   - Failed network requests
   - HTTP status codes
   - Visible validation messages
   - Loading behavior
   - Navigation behavior
   - Role-based visibility
8. Capture screenshot, trace, or diagnostics on failure where configured.
9. Do not modify code during testing.
10. Produce a structured report.

## Selector Rules

- Prefer Playwright role selectors.
- Prefer accessible names and labels.
- Prefer stable test IDs when available.
- Avoid brittle CSS selectors.
- Avoid selectors based on generated PrimeNG classes when a stable alternative exists.
- Do not use unnecessary hard-coded waits.
- Wait for actual UI state, network state, or visible content.
- Do not use force-click unless the reason is documented.
- Do not hide flaky behavior with retries alone.

## Test Data Rules

- Use isolated test data.
- Do not depend on production records.
- Do not use real customer information.
- Use unique identifiers where creation is required.
- Clean up created test data only through approved safe test mechanisms.
- Do not run destructive cleanup against shared or production databases.
- Do not reuse state between unrelated tests.
- Preserve test independence.

## Command Execution Safety

- Never use output pipelines that hide or replace the original command exit code.
- Do not use verification commands with `| tail`, `| head`, or similar pipelines.
- Preserve and report the actual Playwright process exit code.
- When output is large, redirect it to a temporary log file, preserve the original exit code, then inspect relevant lines.
- Report:
  - Actual exit code
  - Parsed test summary
  - Passed tests
  - Failed tests
  - Skipped tests
  - Flaky or retried tests
- If exit code and parsed results disagree, classify the result as failed or inconclusive.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Windows Playwright Invocation

- First try the existing repository npm script or standard Playwright command.
- If the Windows Playwright shim is unavailable or broken, use:
  - `node node_modules/@playwright/test/cli.js`
- When running from the repository root, use the correct Playwright config and test paths.
- Treat a missing Windows `.cmd` or `.ps1` shim as a tooling/environment issue, not an application defect.
- Do not reinstall dependencies or replace `node_modules` without explicit approval.
- Do not create permanent test files merely to perform an observation.
- Temporary observation scripts must remain outside tracked repository paths.
- Report temporary script paths and whether they remain after testing.

## Browser Failure Classification

Classify failures as one of:

- Application defect
- Test defect
- Environment issue
- Test-data issue
- Authentication issue
- Authorization issue
- Network issue
- Browser or certificate issue
- Inconclusive

Do not label a failure as an application defect without evidence.

## Report Format

### UI Test Scope

- Flow tested
- Module
- Roles
- Environment
- Browser
- Preconditions

### Scenarios Executed

For each scenario:

- Scenario
- Steps
- Expected result
- Actual result
- Status
- Evidence

### Technical Evidence

- Console errors
- Failed network requests
- HTTP status codes
- Screenshots
- Traces
- Relevant files and locations

### Test Execution

- Commands executed
- Actual exit codes
- Tests passed
- Tests failed
- Tests skipped
- Retries
- Tests not executed

### Findings

For each finding:

- Severity
- Classification
- Area
- Evidence
- User impact
- Recommended next action

### Final Status

Use one:

- UI flow passed
- UI flow failed
- Partially verified
- Blocked
- Inconclusive

Do not report "UI flow passed" unless every required scenario completed successfully.
