# Bug Fixer Skill

## Purpose

Fix confirmed defects in the Logo Design Portal using the smallest safe change and complete verification.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Fix only confirmed defects.
- Read the bug report or investigation evidence before editing.
- Reproduce the defect before fixing when practical.
- Do not modify unrelated files.
- Preserve unrelated local changes.
- Do not commit or push.
- Do not run destructive commands.
- Do not change production configuration.
- Do not weaken authentication, authorization, validation, logging, or tests.
- Do not remove, disable, skip, or weaken tests to make the suite pass.
- Do not perform broad refactoring during a focused bug fix.
- Ask for approval before destructive, database-wide, security-sensitive, or deployment-related actions.

## Fixing Process

1. Check git status.
2. Read the confirmed bug report.
3. Reproduce the issue when practical.
4. Identify the root cause.
5. Identify the smallest safe fix area.
6. Locate related frontend, backend, database, authorization, cache, SignalR, Hangfire, and test code.
7. Present a concise implementation plan.
8. Add or update a regression test when practical.
9. Confirm the regression test exposes the original defect where practical.
10. Apply the smallest safe code change.
11. Review the relevant diff.
12. Build the affected project.
13. Run the smallest relevant test set first.
14. Run related regression tests when shared behavior is affected.
15. Check browser console and failed network requests for frontend changes.
16. Report all verification evidence.

## Command Execution Safety

- Never use output pipelines that can hide or replace the original command exit code.
- Do not use patterns such as `test-command | tail`, `test-command | head`, or similar pipelines for verification commands.
- Preserve and report the exit code of the actual build or test process.
- When output is large, redirect it to a temporary log file, preserve the original exit code, then inspect relevant lines afterward.
- Report both the actual process exit code and parsed build or test summary.
- If the exit code and parsed result disagree, classify the run as failed or inconclusive.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Change Safety

- Make the smallest safe change.
- Preserve public behavior unless the task explicitly requires a behavior change.
- Reuse existing services, interfaces, DTOs, validators, helpers, components, and patterns.
- Do not introduce duplicate logic.
- Do not introduce new architecture unless necessary and approved.
- Do not expose secrets or sensitive data.
- Do not silently suppress exceptions.
- Validate user-controlled input.
- Preserve role and ownership boundaries.
- Preserve data integrity.
- Review migrations before any schema change.
- Do not apply production migrations.

## Verification Requirements

For backend changes, verify as relevant:

- Build
- Focused unit tests
- Focused integration tests
- API success case
- Validation failure
- Unauthorized case
- Forbidden case
- Missing resource
- Invalid identifier
- Error handling

For frontend changes, verify as relevant:

- Angular build or type check
- Focused unit tests
- Loading state
- Success state
- Empty state
- Error state
- Form validation
- Browser console
- Failed network requests
- Role-based visibility
- Navigation and refresh behavior

For end-to-end flow changes, verify as relevant:

- Happy path
- Negative path
- Authorization path
- Stable selectors
- No unnecessary hard-coded waits
- Isolated test data

## Report Format

### Fix Summary

- Bug title
- Root cause
- Fix applied
- Behavior changed

### Files Changed

For each file:

- File path
- Reason for change
- Summary of change

### Verification

- Commands executed
- Actual exit codes
- Tests passed
- Tests failed
- Tests skipped
- Tests not executed
- Manual verification
- Browser console status
- Network status

### Risk Review

- Regression risk
- Security impact
- Data impact
- Performance impact
- Remaining risks
- Assumptions

### Final Status

Use one:

- Verified fixed
- Fixed but partially verified
- Blocked
- Not fixed

Do not report "Verified fixed" unless the relevant build and tests passed.
