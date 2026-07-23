# API Test Skill

## Purpose

Run focused backend API testing for the Logo Design Portal.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not commit or push.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Test only the requested endpoint or flow.
- Do not run the full backend test suite unless explicitly instructed.
- Separate application defects from environment, authentication, test-data, and database issues.
- Do not fix defects unless explicitly instructed.

## Preconditions

Before API testing:

1. Check git status.
2. Confirm the correct backend project:
   - `Backend/src/LogoDesignPortal.API`
3. Confirm backend availability:
   - `https://localhost:5001`
   - `http://localhost:5000`
4. Confirm MySQL availability where the tested endpoint requires it.
5. Confirm Redis requirements where relevant.
6. Confirm the requested test account and role.
7. Confirm whether the endpoint changes data.
8. Use isolated test data.
9. Do not use production data.
10. Do not start or stop services without explicit approval.

If a dependency is unavailable, report the blocker instead of classifying the result as an application defect.

## API Test Process

1. Identify the requested endpoint or business flow.
2. Locate:
   - Controller
   - Route
   - HTTP method
   - Request DTO
   - Validator
   - Application service or handler
   - Infrastructure dependency
   - Entity and database effects
   - Authorization policy or role requirement
   - Existing unit and integration tests
3. Define the smallest relevant test set.
4. Test as applicable:
   - Successful request
   - Validation failure
   - Missing required field
   - Invalid identifier
   - Missing resource
   - Duplicate record
   - Unauthenticated request
   - Forbidden request
   - Role-based access
   - Pagination
   - Filtering
   - Sorting
   - Error handling
   - Data side effects
5. Inspect:
   - HTTP status code
   - Response body
   - Response schema
   - Headers
   - Logs
   - Database effects
   - Security behavior
6. Do not modify code during testing.
7. Produce a structured report.

## Authentication Rules

- Do not bypass authentication.
- Use valid test credentials only.
- Do not expose access tokens in reports.
- Redact tokens, passwords, secrets, and connection strings.
- Verify unauthenticated behavior separately from forbidden behavior.
- Verify affected roles independently.
- Backend authorization is the source of truth.

## Credential Handling

- Never print passwords, access tokens, refresh tokens, API keys, connection strings, or full authorization headers in reports.
- Do not include seeded default passwords in output.
- Refer to credentials generically, such as:
  - valid seeded test account
  - invalid password
  - authorized test user
- Redact sensitive response fields before displaying response bodies.
- When validating token responses, report only:
  - field present or absent
  - token type
  - expiry metadata
  - schema validity
- Do not echo token values.
- Do not store credentials in temporary scripts unless unavoidable.
- If a temporary script requires credentials, keep it outside tracked paths and delete it after the run.
- Report whether temporary credential-bearing files were created and whether they were removed.

## Data Safety Rules

- Use isolated test data.
- Do not modify production records.
- Do not delete shared data.
- Do not run destructive cleanup.
- Do not execute schema changes.
- Do not apply migrations.
- For write endpoints, record exactly what data was created or changed.
- Clean up only through approved safe test mechanisms.
- If cleanup cannot be performed safely, report the remaining test data.

## Command Execution Safety

- Never use output pipelines that hide or replace the original command exit code.
- Do not use verification commands with `| tail`, `| head`, or similar pipelines.
- Preserve and report the actual command or test-process exit code.
- When output is large, redirect it to a temporary log file, preserve the original exit code, then inspect relevant lines.
- Report:
  - Actual exit code
  - Parsed test summary
  - Passed tests
  - Failed tests
  - Skipped tests
- If exit code and parsed results disagree, classify the result as failed or inconclusive.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Request Execution

Prefer existing project tests first.

When direct API requests are needed:

- Use a standard tool already available in the environment.
- Prefer `curl.exe` on Windows when `curl` shell aliases may behave differently.
- Use explicit HTTP method, headers, content type, and request body.
- Do not print bearer tokens.
- Do not save secrets in tracked files.
- Do not create permanent API scripts merely for one observation.
- Temporary request scripts must remain outside tracked repository paths.
- Report temporary script paths and whether they remain after testing.

## Failure Classification

Classify failures as one of:

- Application defect
- Test defect
- Environment issue
- Database issue
- Test-data issue
- Authentication issue
- Authorization issue
- Network issue
- Configuration issue
- Inconclusive

Do not classify a failure as an application defect without evidence.

## Report Format

### API Test Scope

- Endpoint
- Method
- Module
- Roles
- Environment
- Preconditions

### Scenarios Executed

For each scenario:

- Scenario
- Request
- Expected result
- Actual result
- HTTP status
- Status
- Evidence

### Technical Evidence

- Controller and route
- DTO and validator
- Authorization rule
- Response body summary
- Logs
- Database effects
- Relevant files and locations

### Test Execution

- Commands executed
- Actual exit codes
- Tests passed
- Tests failed
- Tests skipped
- Tests not executed

### Findings

For each finding:

- Severity
- Classification
- Area
- Evidence
- User impact
- Data impact
- Security impact
- Recommended next action

### Final Status

Use one:

- API flow passed
- API flow failed
- Partially verified
- Blocked
- Inconclusive

Do not report "API flow passed" unless all required scenarios completed successfully.
