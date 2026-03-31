# Research: Harvest HttpClientDecoratorPattern Ideas

## Decision 1: Sunset the legacy repository only after harvest completion and public redirection

**Decision**: Use a three-step sunset strategy: harvest approved ideas into the primary repository, publish redirection and migration communication in the legacy repository, then archive the legacy repository only after every adopt-now candidate is fully implemented and released.

**Rationale**: This preserves history, minimizes user confusion, and matches the clarified requirement that sunset cannot happen until all adopt-now work is complete.

**Alternatives considered**:
- Immediate deletion: rejected because it loses reference history and increases user confusion.
- Keep the legacy repository active indefinitely: rejected because it preserves split ownership and maintenance overhead.
- Archive before implementation completion: rejected because it conflicts with the clarified sunset gate.

## Decision 2: Keep harvested work in existing packages by default

**Decision**: Route harvested ideas into the existing core package, crawler package, and documentation/examples by default.

**Rationale**: The current repository already separates core HTTP and crawler concerns. Defaulting to existing package boundaries prevents unnecessary package sprawl and preserves the one-line developer experience.

**Alternatives considered**:
- Create a new package for any logically distinct idea: rejected because logical distinction alone is too weak and would fragment the product surface.
- Constrain work to documentation only: rejected because the feature explicitly includes adopting worthwhile code and requirements into the shipped packages.

## Decision 3: Allow a new package only with compelling separation

**Decision**: A new package is allowed only when a harvested capability serves a distinct consumer need and can be versioned and released with clear standalone value.

**Rationale**: This creates a high bar consistent with the existing core/crawler split and the clarified spec answers. It prevents package growth based on preference alone.

**Alternatives considered**:
- Distinct functionality alone: rejected because technical distinction without consumer distinction does not justify a new package.
- Smaller dependency footprint alone: rejected because footprint reduction without standalone release value is insufficient.
- Maintainer preference: rejected because it is not a stable planning or release criterion.

## Decision 4: Make adoption evidence release-blocking

**Decision**: Every adopt-now candidate requires dual evidence before release: automated verification and user-facing documentation.

**Rationale**: This aligns with the constitution's quality requirements and the clarified spec. It creates a consistent release gate for runtime code, docs/examples, and migration guidance.

**Alternatives considered**:
- Tests only: rejected because users still need discoverable guidance and examples.
- Documentation only: rejected because it provides no objective proof of correctness.
- Case-by-case evidence depth with no default: rejected because it weakens release discipline.

## Decision 5: Permit breaking-impact adoptions in this cycle with migration guidance

**Decision**: Breaking-impact candidates may be adopted in the current harvest cycle when they provide sufficient value and include explicit migration guidance in release communications.

**Rationale**: This keeps the harvest useful instead of artificially excluding valuable improvements while still protecting consumers through explicit communication.

**Alternatives considered**:
- Defer all breaking changes to a future major-only track: rejected because it would delay legitimate high-value adoptions discovered during the harvest.
- Reject all breaking candidates: rejected because it prevents intentional product evolution.

## Decision 6: Preserve traceability from discovery to release

**Decision**: Maintain a traceability chain from harvest candidate to decision record, implementation work item, verification evidence, documentation evidence, and release or sunset outcome.

**Rationale**: Traceability supports planning, auditability, release readiness, and archive confidence. It also keeps future maintainers from re-litigating past decisions without context.

**Alternatives considered**:
- Track only in git history: rejected because repository archaeology is too slow and incomplete for planning.
- Track only in issues: rejected because issue threads do not provide a stable schema for release gating.
