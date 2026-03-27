# Testing Standards

## Test Pyramid
- Unit tests are the default for pure logic and service behavior.
- Integration tests validate repository, API contracts, and persistence boundaries.
- E2E tests validate critical business journeys and cross-role workflows.
- Keep E2E focused on high-value paths; push edge permutations down to unit/integration.

## Coverage Rules
- Backend line coverage gate remains at 80%+.
- Frontend unit coverage gate remains enforced in CI.
- New code must not reduce baseline coverage in touched modules.
- Coverage exceptions require explicit reviewer approval in PR.

## Naming Conventions
- Unit tests: `<unit>.<method-or-scenario>.spec`.
- Integration tests: `<feature>.integration.spec` or existing project naming standard.
- E2E tests: `<business-flow>.spec.ts` with descriptive scenario titles.
- Test names must include expected behavior, not implementation detail.

## Unit vs E2E Decision Rules
- Write unit tests when behavior is deterministic and isolated.
- Write integration tests when crossing DB, API, queue, or external boundaries.
- Write E2E tests when validating role transitions, business lifecycle, or UX resilience.
- Avoid duplicating the same assertion at every pyramid layer.

## Flaky Test Policy
- Flaky tests are production defects in the delivery pipeline.
- Any flaky test must be triaged immediately and fixed within 24 hours.
- Do not skip tests in CI (`test.skip`, ignore lists, or allow-failure patterns are prohibited).
- If a failure is non-deterministic, quarantine only with explicit QA owner and 24h SLA.

## E2E Reliability Rules
- Use API factories for deterministic setup and teardown.
- Use `browser.newContext()` for multi-user role isolation.
- Validate both UI and API state for critical lifecycle checkpoints.
- Add failure-mode scenarios (latency, transient 5xx, retry/recovery) for key workflows.

## Mutation Testing Baseline
- Backend mutation command: `dotnet stryker --config-file Backend/stryker-config.json`.
- Frontend mutation command: `npm run mutation --prefix Frontend`.
- Mutation score thresholds are enforced in config and should trend upward sprint-over-sprint.
