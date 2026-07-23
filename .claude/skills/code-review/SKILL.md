# Code Review Skill

## Purpose

Perform focused, read-only code reviews for the Logo Design Portal.

## Rules

- Follow all instructions in the root CLAUDE.md.
- Do not modify application code.
- Do not modify configuration.
- Do not modify database files.
- Do not commit or push.
- Do not run destructive commands.
- Preserve unrelated local changes.
- Review only the requested scope.
- Use targeted inspection.
- Separate confirmed defects from assumptions.
- Do not fix findings unless explicitly instructed in a separate task.
- Do not report style preferences as defects unless they violate established project conventions.

## Review Process

1. Check git status.
2. Identify the review scope:
   - Commit
   - Branch
   - Diff
   - File
   - Module
   - Pull request
3. Read the relevant diff first.
4. Read surrounding implementation only where necessary.
5. Search for related:
   - Interfaces
   - Services
   - DTOs
   - Validators
   - Entities
   - Controllers
   - Angular components
   - Routes
   - Guards
   - Tests
6. Check architecture boundaries.
7. Check correctness.
8. Check authentication and authorization.
9. Check validation.
10. Check data integrity.
11. Check error handling.
12. Check logging and sensitive-data exposure.
13. Check performance and database-query risks.
14. Check concurrency and idempotency where relevant.
15. Check frontend loading, empty, success, and error states.
16. Check test coverage and regression risk.
17. Produce a structured report.
18. Do not modify code.

## Review Areas

### Correctness

- Logic errors
- Incorrect conditions
- Null handling
- Boundary cases
- Incorrect mappings
- Incorrect response status codes
- Broken navigation
- State-management problems
- Async or subscription issues

### Architecture

- Clean Architecture violations
- Business logic in controllers
- Business logic in Angular components
- Infrastructure leakage into Application or Domain
- Duplicate services, DTOs, helpers, or components
- Unnecessary abstractions

### Security

- Authentication bypass
- Authorization gaps
- Insecure direct object references
- Sensitive-data exposure
- Missing validation
- Unsafe logging
- Token handling
- Injection risks
- Over-posting
- Missing ownership checks

### Database

- N+1 queries
- Missing transactions
- Incorrect relationships
- Shadow foreign keys
- Missing indexes
- Unsafe migrations
- Data-loss risks
- Query-filter problems
- Pagination or filtering issues

### Frontend

- Broken loading states
- Missing error handling
- Missing empty states
- Incorrect role visibility
- Route-guard gaps
- Memory leaks
- Unhandled subscriptions
- Duplicate API requests
- Brittle selectors
- Accessibility problems
- Responsive-layout regressions

### Testing

- Missing regression tests
- Vacuous tests
- Tests with no expectations
- Incorrect mocks
- Tests that do not exercise the intended branch
- Tests that pass for the wrong reason
- Disabled or skipped tests
- Coverage gaps in changed behavior

## Command Execution Safety

- Never use output pipelines that hide or replace the original command exit code.
- Do not use verification commands with `| tail`, `| head`, or similar pipelines.
- Preserve and report the actual process exit code.
- When output is large, redirect it to a temporary log file, preserve the exit code, then inspect relevant lines.
- Report both the actual exit code and parsed result summary.
- If exit code and parsed results disagree, classify the result as failed or inconclusive.
- Do not delete temporary diagnostic logs unless explicitly instructed.

## Severity Levels

### Critical

- Immediate security compromise
- Data loss
- Authentication bypass
- Production outage
- Irreversible corruption

### High

- Major authorization issue
- Serious data-integrity defect
- Common user flow broken
- Significant security exposure

### Medium

- Functional defect with limited scope
- Important missing validation
- Meaningful performance issue
- Weak regression protection

### Low

- Minor defect
- Maintainability risk
- Limited edge case
- Test-quality issue

### Informational

- Improvement opportunity
- Non-blocking cleanup
- Documentation gap
- Optional optimization

## Finding Format

For each finding include:

- Severity
- Category
- File and location
- Finding
- Evidence
- Risk
- Affected users or roles
- Recommended action
- Verification status

Do not report a finding without file-level or behavioral evidence.

## Report Format

### Review Scope

- Review target
- Files reviewed
- Modules affected
- Roles affected
- Diff or commit reviewed

### Findings

Group findings by:

- Critical
- High
- Medium
- Low
- Informational

### Test Review

- Existing tests reviewed
- Missing tests
- Weak or vacuous tests
- Tests executed
- Tests not executed
- Actual exit codes

### Risk Summary

- Security risk
- Data risk
- Regression risk
- Performance risk
- Maintainability risk

### Final Recommendation

Use one:

- Approve
- Approve with minor findings
- Changes required
- Block

Do not use "Approve" when Critical, High, or unresolved Medium findings exist.
