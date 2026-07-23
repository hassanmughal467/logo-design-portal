# Project Audit Skill

## Purpose

Perform a focused, read-only audit of the Logo Design Portal.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not run destructive commands.
- Do not commit or push.
- Use targeted inspection instead of scanning the entire repository.
- Preserve unrelated local changes.
- Separate confirmed findings from assumptions.
- Do not claim that a problem exists without evidence.

## Audit Process

1. Check git status.
2. Identify the requested audit scope.
3. Read only files relevant to that scope.
4. Search for related services, controllers, DTOs, validators, entities, Angular components, routes, guards, and tests.
5. Check architecture boundaries.
6. Check authentication and authorization.
7. Check validation and error handling.
8. Check database and query risks.
9. Check frontend loading, empty, success, and error states.
10. Check test coverage.
11. Check logging and sensitive-data exposure.
12. Check regression risk.
13. Do not fix findings unless explicitly instructed.

## Command Execution Safety

- Never use output pipelines that can hide or replace the original command exit code.
- Do not use patterns such as `test-command | tail`, `test-command | head`, or similar pipelines for verification commands.
- Preserve and report the exit code of the actual build or test process.
- When command output is large, redirect it to a temporary log file, preserve the original exit code, and inspect only the relevant final lines afterward.
- A test run must be reported as failed when any test fails, even if the surrounding shell command returns exit code 0.
- Report both:
  - Actual process exit code
  - Parsed test result summary
- If the exit code and parsed result disagree, classify the run as failed or inconclusive and investigate the discrepancy.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Report Format

For each finding include:

- Severity: Critical, High, Medium, Low, or Informational
- Area
- File and location
- Finding
- Evidence
- Risk
- Recommended action
- Verification status

At the end include:

- Scope reviewed
- Files inspected
- Commands executed
- Tests executed
- Tests not executed
- Assumptions
- Unresolved questions
- Overall risk summary
