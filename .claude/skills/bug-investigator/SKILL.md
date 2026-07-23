# Bug Investigator Skill

## Purpose

Investigate and diagnose defects in the Logo Design Portal without changing application code.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not commit or push.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Use targeted inspection only.
- Reproduce the issue before diagnosing it when practical.
- Separate confirmed findings from assumptions.
- Do not claim a root cause without evidence.
- Do not fix the defect unless explicitly instructed in a separate task.

## Investigation Process

1. Check git status.
2. Record the reported issue.
3. Record exact reproduction steps.
4. Record expected behavior.
5. Record actual behavior.
6. Identify affected user roles.
7. Identify affected frontend routes, components, services, guards, and models.
8. Identify affected backend controllers, DTOs, validators, services, entities, repositories, and middleware.
9. Inspect browser console and network activity for frontend issues.
10. Inspect API logs, exceptions, and response bodies for backend issues.
11. Trace the complete flow through Angular, API, Application, Infrastructure, and database layers.
12. Check authentication and authorization behavior.
13. Check related SignalR, Hangfire, Redis, and cache behavior where relevant.
14. Check existing tests and whether they cover the defect.
15. Identify the smallest likely fix area.
16. Identify regression risk.
17. Do not make code changes.

## Command Execution Safety

- Never use output pipelines that can hide or replace the original command exit code.
- Do not use patterns such as `test-command | tail`, `test-command | head`, or similar pipelines for verification commands.
- Preserve and report the exit code of the actual build or test process.
- When command output is large, redirect it to a temporary log file, preserve the original exit code, and inspect only relevant lines afterward.
- Report both the actual process exit code and the parsed result summary.
- If the exit code and parsed result disagree, classify the run as failed or inconclusive.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Report Format

### Bug Summary

- Title
- Severity
- Affected module
- Affected roles
- Environment

### Reproduction

- Preconditions
- Steps
- Expected result
- Actual result
- Reproduction status

### Evidence

- Frontend evidence
- Backend evidence
- Database evidence
- Logs
- Console errors
- Network responses
- Relevant files and locations

### Root Cause

- Confirmed root cause
- Supporting evidence
- Assumptions
- Alternative causes ruled out

### Impact

- User impact
- Data impact
- Security impact
- Regression risk

### Recommended Fix

- Smallest safe fix area
- Files likely affected
- Tests required
- Risks to verify

### Execution Summary

- Files inspected
- Commands executed
- Tests executed
- Tests not executed
- Actual exit codes
- Unresolved questions
