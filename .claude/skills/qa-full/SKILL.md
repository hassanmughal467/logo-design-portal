# Full QA Skill

## Purpose

Coordinate complete, repeatable QA testing for a selected Logo Design Portal module or business flow.

This Skill orchestrates focused project inspection, API testing, UI testing, regression testing, defect reporting, and release-risk classification.

It does not automatically fix defects.

## Existing Specialized Skills

Use the rules and workflows from these project Skills where relevant:

- `.claude/skills/project-audit/SKILL.md`
- `.claude/skills/bug-investigator/SKILL.md`
- `.claude/skills/regression-test/SKILL.md`
- `.claude/skills/ui-test/SKILL.md`
- `.claude/skills/api-test/SKILL.md`
- `.claude/skills/code-review/SKILL.md`
- `.claude/skills/release-check/SKILL.md`

Do not use `.claude/skills/bug-fixer/SKILL.md` during a QA-only run unless the user starts a separate approved fixing task.

## Rules

- Follow all instructions in the root `CLAUDE.md`.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not commit or push.
- Do not deploy.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Test only the requested module, flow, or release scope.
- Do not scan the entire repository when targeted inspection is sufficient.
- Do not run every test suite automatically.
- Start with the smallest relevant test set.
- Expand testing only when shared behavior or release risk justifies it.
- Separate application defects from environment, test, data, authentication, authorization, network, and tooling issues.
- Do not fix findings during this Skill.
- Do not report success without verification evidence.

## Required Input

Before testing, identify:

- Module or user flow
- Target environment
- User roles
- Expected behavior
- Backend endpoints
- Frontend routes
- Whether the flow creates, changes, or deletes data
- Required services
- Required test accounts
- Whether broad regression testing is approved

If the requested scope is unclear, report the missing scope instead of running broad tests.

## Service Safety

- Do not start or stop backend, frontend, MySQL, Redis, or other services without explicit approval.
- The correct backend project is:
  - `Backend/src/LogoDesignPortal.API`
- Never start:
  - `Backend/LogoDesignPortal.API`
- Preferred backend startup command:
  - `dotnet run --launch-profile https`
- Expected backend URLs:
  - `https://localhost:5001`
  - `http://localhost:5000`
- Preferred frontend startup command:
  - `npm start --prefix Frontend`
- Expected frontend URL:
  - `http://localhost:4200`
- Stop only services started during the current QA run and only when approval was given.
- Never stop services that were already running before testing.

## QA Workflow

### Phase 1 — Baseline

1. Check `git status`.
2. Record current branch and commit.
3. Record unexplained working-tree changes.
4. Confirm the requested test scope.
5. Confirm required roles and credentials.
6. Confirm required services.
7. Confirm whether test data changes are allowed.

### Phase 2 — Targeted Inspection

1. Locate relevant Angular routes, components, services, guards, models, and tests.
2. Locate relevant API controllers, DTOs, validators, services, entities, repositories, middleware, and tests.
3. Identify authentication and authorization requirements.
4. Identify database effects.
5. Identify Redis, SignalR, Hangfire, cache, notification, or background-job effects.
6. Identify existing unit, integration, API, Playwright, and regression tests.
7. Do not modify files.

### Phase 3 — Test Plan

Define only applicable scenarios:

- Happy path
- Negative path
- Validation path
- Boundary path
- Authentication path
- Authorization path
- Missing-resource path
- Duplicate-data path
- Loading state
- Empty state
- Error state
- Navigation and refresh behavior
- Browser console behavior
- Failed network requests
- Database side effects
- SignalR events
- Hangfire jobs
- Redis fallback
- Logging and sensitive-data handling

Mark non-applicable scenarios clearly.

### Phase 4 — Focused Automated Testing

Run the smallest relevant tests first.

As applicable:

- Backend unit tests
- Backend integration tests
- Frontend unit tests
- Focused API requests
- Focused Playwright tests
- Focused regression tests

Do not run full suites unless:

- Explicitly requested
- Shared infrastructure changed
- Authentication or authorization changed
- Common services changed
- Release verification requires it

### Phase 5 — Runtime Verification

As applicable, verify:

- Backend availability
- Frontend availability
- Login and session behavior
- API status codes
- Response structure
- Validation messages
- Role-based behavior
- Browser console errors
- Failed network requests
- Database side effects
- SignalR behavior
- Hangfire behavior
- Redis fallback
- Health checks
- Logging
- Sensitive-data exposure

### Phase 6 — Failure Investigation

For every failure:

1. Reproduce it.
2. Record exact steps.
3. Record expected behavior.
4. Record actual behavior.
5. Preserve evidence.
6. Identify the failure classification.
7. Identify affected files and modules.
8. Identify likely root cause only when evidence supports it.
9. Identify regression risk.
10. Do not fix it.

### Phase 7 — Final QA Report

Produce a complete structured report.

## Command Execution Safety

- Never use output pipelines that hide or replace the original command exit code.
- Do not use verification commands with `| tail`, `| head`, or similar pipelines.
- Preserve the actual process exit code.
- When output is large, redirect it to a temporary log file, preserve the original exit code, then inspect relevant lines.
- Report:
  - Command
  - Actual exit code
  - Parsed result
  - Passed tests
  - Failed tests
  - Skipped tests
  - Retried or flaky tests
  - Coverage status
- If exit code and parsed results disagree, classify the result as failed or inconclusive until the discrepancy is explained.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Angular Focused Test Rule

When running a single Angular spec file and global coverage thresholds would cause an unrelated non-zero exit code, use:

`--code-coverage=false`

Do not modify `karma.conf.js` or reduce project-wide coverage thresholds.

## Windows Playwright Rule

First use the repository npm script or standard Playwright command.

If the Windows Playwright shim is unavailable, use:

`node node_modules/@playwright/test/cli.js`

Treat a missing `.cmd` or `.ps1` shim as a tooling issue, not an application defect.

Do not reinstall dependencies without approval.

## Credential Handling

- Never print passwords.
- Never print access tokens.
- Never print refresh tokens.
- Never print full authorization headers.
- Never print connection strings.
- Refer to credentials generically.
- Redact sensitive response fields.
- Report token fields only as present, absent, valid, invalid, or expired.
- Temporary credential-bearing files must remain outside tracked repository paths and must be removed after use.
- Report whether temporary credential files were created and removed.

## Test Data Safety

- Use isolated test data.
- Do not use production data.
- Do not delete shared data.
- Do not run destructive cleanup.
- Do not apply migrations.
- Do not execute schema changes.
- Record data created or changed during testing.
- Clean up only through approved safe test mechanisms.
- If safe cleanup is unavailable, report remaining test data.

## Failure Classification

Use one classification for every failure:

- Application defect
- Test defect
- Environment issue
- Database issue
- Test-data issue
- Authentication issue
- Authorization issue
- Network issue
- Configuration issue
- Tooling issue
- Browser or certificate issue
- Pre-existing issue
- Inconclusive

Do not classify something as an application defect without evidence.

## Severity Levels

- Critical
- High
- Medium
- Low
- Informational

## QA Report Format

### QA Identification

- Module or flow
- Environment
- Branch
- Commit
- Working-tree status
- Roles
- Services used

### Scope

- Included
- Excluded
- Assumptions
- Preconditions
- Tests not approved

### Files Inspected

List only relevant inspected files.

### Test Plan

For each scenario:

- Scenario
- Expected result
- Test type
- Execution status

### Test Execution

For every command:

- Command
- Actual exit code
- Parsed result
- Passed
- Failed
- Skipped
- Retries
- Coverage status

### Scenario Results

For every scenario:

- Steps
- Expected result
- Actual result
- Status
- Evidence

### Findings

For every finding:

- ID
- Severity
- Classification
- Module
- Roles affected
- File and location
- Finding
- Evidence
- User impact
- Data impact
- Security impact
- Regression risk
- Recommended next action
- Verification status

### Environment and Tooling Issues

List separately from application defects.

### Data Changes

- Records created
- Records changed
- Records deleted
- Cleanup status
- Remaining test data

### Final QA Status

Use one:

- QA passed
- QA passed with warnings
- QA failed
- Partially verified
- Blocked
- Inconclusive

Do not report `QA passed` when:

- A required scenario failed
- A required test was not executed
- A Critical, High, or unresolved Medium defect exists
- The environment was not verified
- Exit codes and parsed results conflict without explanation

## Recommended Follow-Up

When defects are found, recommend this separate workflow:

1. `/bug-investigator`
2. Review investigation report
3. `/bug-fixer`
4. `/regression-test`
5. `/ui-test` or `/api-test` as applicable
6. `/release-check` before deployment

Do not automatically begin the fixing workflow.
