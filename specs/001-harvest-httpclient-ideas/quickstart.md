# Quickstart: Harvest HttpClientDecoratorPattern Ideas

## Purpose

This quickstart describes how maintainers execute the harvest plan from idea discovery through legacy repository archival.

## 1. Build the harvest backlog

1. Audit the legacy repository and record every worthwhile feature, requirement, pattern, example, and process idea as a `HarvestCandidate`.
2. Compare each candidate against the current repository and classify its novelty.
3. Create a `DecisionRecord` for each candidate with one of four outcomes: adopt now, defer, docs-only, or reject.

## 2. Decide the target surface

1. Default candidates into the existing core package, crawler package, or documentation/examples.
2. Consider a new package only if the candidate serves a distinct consumer need and has clear standalone release value.
3. Mark any breaking-impact adoption and prepare migration guidance early.

## 3. Implement adopt-now candidates

1. Create an `AdoptionWorkItem` for every adopt-now candidate.
2. Implement changes in the appropriate project areas.
3. Add automated verification for each adopted item.
4. Update user-facing documentation for each adopted item.
5. Add migration guidance for any breaking-impact change.

## 4. Validate release readiness

1. Confirm all adopt-now work items have automated verification evidence.
2. Confirm all adopt-now work items have user-facing documentation evidence.
3. Confirm any breaking-impact adoption includes explicit migration communication.
4. Confirm no sunset step begins before every adopt-now item is fully implemented.

## 5. Prepare legacy repository sunset

1. Publish a clear status notice in the legacy repository pointing users to the primary repository.
2. Publish the final harvest summary showing what was adopted, deferred, documented, or rejected.
3. Create or update the `SunsetReadinessRecord`.
4. Archive the legacy repository only when every readiness field is satisfied.

## Expected Outputs

- A complete harvest backlog with traceable decisions.
- Released implementations for all adopt-now candidates.
- Documentation and migration guidance aligned with adopted work.
- A legacy repository archive that occurs after, not before, implementation completion.
