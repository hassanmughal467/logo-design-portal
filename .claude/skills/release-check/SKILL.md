# Release Check Skill

## Purpose

Perform a read-only pre-release verification of the Logo Design Portal and determine whether the selected release is ready for deployment.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not deploy.
- Do not commit or push.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Review only the requested release scope.
- Separate confirmed blockers from warnings and assumptions.
- Do not fix findings unless explicitly instructed in a separate task.
- Do not report release readiness without verification evidence.

## Release Scope

Before starting, identify:

- Release branch
- Target commit or commit range
- Target environment
- Modules changed
- Database changes
- Configuration changes
- Authentication or authorization changes
- Background-job changes
- SignalR changes
- Redis or cache changes
- API changes
- Frontend changes
- Tests expected for the release

If the release target is unclear, report the missing information before running broad verification.

## Pre-Release Process

1. Check `git status`.
2. Confirm the current branch.
3. Confirm the target commit or release range.
4. Review the relevant diff.
5. Identify affected modules and user roles.
6. Identify required environment variables and configuration.
7. Identify database migrations.
8. Identify API contract changes.
9. Identify frontend routing or environment changes.
10. Identify authentication and authorization impact.
11. Identify SignalR, Hangfire, Redis, caching, and health-check impact.
12. Review related unit, integration, API, UI, and regression tests.
13. Run the smallest required verification set first.
14. Expand testing based on change impact.
15. Review build output and warnings.
16. Review unresolved findings from earlier audits where relevant.
17. Produce a structured release-readiness report.
18. Do not deploy.

## Git Safety

- Do not switch branches without explicit approval.
- Do not pull, merge, rebase, reset, cherry-pick, or rewrite history without explicit approval.
- Do not discard local changes.
- Do not commit or push.
- Record the current branch and commit hash.
- Record whether the working tree is clean.
- Treat unexpected local changes as a release risk.

## Build Verification

Verify as applicable:

### Backend

- Restore succeeds
- Build succeeds
- No compilation errors
- Relevant warnings are reviewed
- Focused unit tests pass
- Relevant integration tests pass

### Frontend

- Dependency installation state is understood
- Angular build succeeds
- Type checking succeeds
- Relevant unit tests pass
- Required Playwright flows pass
- Environment replacement is correct

Do not ignore build warnings that indicate security, compatibility, data, or runtime risk.

## Database Verification

- Identify pending migrations.
- Review migration operations.
- Identify destructive operations.
- Identify renamed or removed fields.
- Check compatibility with existing data.
- Check rollback feasibility.
- Do not apply production migrations.
- Do not run destructive SQL.
- Confirm database backup and rollback requirements where relevant.
- Confirm test and production connection strings are not mixed.

A production migration requires explicit approval outside this Skill.

## Configuration Verification

Check as relevant:

- Development configuration
- Production configuration
- Environment variables
- API URLs
- CORS configuration
- JWT settings
- Database connection strings
- Redis settings
- Hangfire settings
- SignalR settings
- Logging configuration
- OpenTelemetry configuration
- Azure Monitor configuration
- Health-check configuration
- File-storage paths
- External-service endpoints

Do not print secrets or full sensitive configuration values.

## Security Verification

Check release changes for:

- Authentication bypass
- Authorization gaps
- Missing ownership checks
- Sensitive-data exposure
- Unsafe logging
- Secret exposure
- Insecure environment values
- Missing validation
- Over-posting
- Injection risks
- Token-storage regressions
- CORS regressions
- Error responses exposing internal details

## Operational Verification

Check as relevant:

- Health endpoints
- Startup behavior
- Database connectivity
- Redis fallback
- Hangfire startup
- SignalR hub startup
- File-storage initialization
- Logging startup
- Background database initialization
- Graceful failure behavior
- Rollback instructions
- Release notes

## Dependency Verification

- Review restore or installation warnings.
- Identify known vulnerable dependencies reported by official package tooling.
- Do not upgrade packages automatically.
- Report package name, current version, severity, and affected project.
- Separate release blockers from follow-up dependency work.
- Do not classify a dependency warning as resolved without verification.

## Command Execution Safety

- Never use output pipelines that hide or replace the original command exit code.
- Do not use verification commands with `| tail`, `| head`, or similar pipelines.
- Preserve and report the actual build or test-process exit code.
- When output is large, redirect it to a temporary log file, preserve the original exit code, then inspect relevant lines.
- Report:
  - Command
  - Actual exit code
  - Parsed result
  - Warnings
  - Failures
- If the exit code and parsed result disagree, classify the result as failed or inconclusive.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Release Blocker Classification

### Blocker

Examples:

- Build failure
- Required test failure
- Critical or High security defect
- Destructive migration without approval
- Production configuration points to localhost
- Missing required production configuration
- Authentication or authorization regression
- Data-loss risk
- Working tree contains unexplained changes

### Warning

Examples:

- Moderate dependency vulnerability
- Non-blocking compiler warning
- Missing optional test coverage
- Known technical debt
- Incomplete documentation
- Performance concern without confirmed production impact

### Informational

Examples:

- Cleanup opportunity
- Optional optimization
- Historical documentation issue
- Non-blocking organizational improvement

## Report Format

### Release Identification

- Branch
- Commit
- Commit range
- Target environment
- Working-tree status

### Change Scope

- Files changed
- Modules affected
- Roles affected
- Database impact
- Configuration impact
- External-system impact

### Verification Executed

For every command:

- Command
- Actual exit code
- Parsed result
- Warnings
- Failures

### Test Results

- Backend unit tests
- Backend integration tests
- Frontend unit tests
- API tests
- Playwright tests
- Regression tests
- Tests not executed

### Release Findings

Group by:

- Blockers
- Warnings
- Informational

For each finding include:

- Category
- Severity
- File or system
- Evidence
- Release impact
- Recommended action
- Verification status

### Deployment Preconditions

- Required environment variables
- Required migrations
- Required services
- Backup requirements
- Rollback requirements
- Monitoring requirements

### Final Status

Use one:

- Release ready
- Release ready with warnings
- Release blocked
- Partially verified
- Inconclusive

Do not report "Release ready" if:

- Required tests were not run
- Any required test failed
- The build failed
- A release blocker remains unresolved
- Production configuration is unverified
- Database migration impact is unverified where migrations exist
