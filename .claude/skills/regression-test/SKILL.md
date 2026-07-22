# Regression Test Skill

## Purpose

Run focused regression testing for confirmed application changes in the Logo Design Portal.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not commit or push.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Test only the requested scope.
- Start with the smallest relevant test set.
- Expand testing only when shared behavior is affected.
- Separate product failures from environment failures.
- Do not report success without verification evidence.

## Regression Process

1. Check git status.
2. Identify the files changed.
3. Identify the affected module and user flow.
4. Identify related frontend, backend, database, authorization, cache, SignalR, Hangfire, and shared-service dependencies.
5. Identify existing unit, integration, and end-to-end tests.
6. Define the minimum focused regression set.
7. Run focused tests first.
8. Expand to related regression tests only when justified.
9. Record exact failures.
10. Compare failures with the changed behavior.
11. Separate new regressions from pre-existing failures.
12. Do not fix defects unless explicitly instructed.

## Command Execution Safety

- Never use output pipelines that hide or replace the original command exit code.
- Do not use verification commands with `| tail`, `| head`, or similar pipelines.
- Preserve and report the exit code of the actual build or test process.
- When output is large, redirect it to a temporary log file, preserve the original exit code, then inspect relevant lines.
- Report both:
  - Actual process exit code
  - Parsed test result summary
- If exit code and parsed results disagree, classify the result as failed or inconclusive until the discrepancy is explained.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Frontend Focused Test Rule

When running a single Angular spec file and project-wide coverage thresholds would cause an unrelated non-zero exit code, use:

`--code-coverage=false`

Do not modify `karma.conf.js` or reduce project coverage thresholds.

## Regression Coverage

Check as relevant:

### Frontend

- Component behavior
- Service behavior
- Form validation
- Loading state
- Success state
- Empty state
- Error state
- Navigation
- Browser refresh
- Role-based visibility
- Browser console
- Failed network requests

### Backend

- Build
- Unit tests
- Integration tests
- Successful API response
- Validation failure
- Unauthorized request
- Forbidden request
- Missing resource
- Invalid identifier
- Error handling
- Data integrity

### Shared Systems

- JWT authentication
- Role authorization
- SignalR events
- Hangfire jobs
- Redis fallback
- Database queries
- Cache invalidation
- Logging
- Health checks

## Report Format

### Regression Scope

- Change tested
- Files changed
- Modules affected
- Roles affected

### Tests Executed

For each command:

- Command
- Actual exit code
- Parsed result
- Passed
- Failed
- Skipped
- Coverage status

### Findings

For each failure:

- Severity
- Test
- Expected result
- Actual result
- Evidence
- New regression or pre-existing issue
- Recommended next action

### Final Status

Use one:

- Regression passed
- Regression failed
- Partially verified
- Blocked

Do not report "Regression passed" unless all required focused regression tests complete successfully.
