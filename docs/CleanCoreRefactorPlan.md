# Clean Core Refactor Plan

This document describes a practical, phased plan to move SimpleBooking from its current console-driven structure to a clean-core architecture where business logic is reusable from multiple entry points.

## Recommended Order

1. Clean Core first: isolate business logic and push side effects to adapters/infrastructure.
2. SOLID second: harden the design after boundaries are visible in code.

Note: Some dependency inversion is intentionally introduced during the clean-core pass because it is the mechanism that separates core/application from infrastructure.

## Target Outcome

- Business logic can run without Console, file I/O, or system clock side effects.
- The same use cases can be called from Console, API, and web UI.
- Infrastructure details (JSON today, database later) can be swapped without changing core logic.
- Tests can run quickly with in-memory or mocked dependencies.

## Current Friction

- Booking flow mixes business logic and Console interaction.
- Persistence is static and file-bound.
- System time is read directly in domain-adjacent flow.
- Composition is done with concrete types instead of abstractions.

## Refactor Principles

- Keep domain rules pure.
- Depend on abstractions at boundaries.
- Place side effects at the edges.
- Use a composition root to wire concrete implementations.
- Preserve behavior at every phase.

## Proposed Layering

1. Core (Domain)
- Entities/value objects and booking rules.
- No references to Console, File, JSON, DateTime.Now/UtcNow.

2. Application (Use Cases)
- Orchestrates booking flow.
- Depends on ports/interfaces only.

3. Infrastructure
- JSON repositories, clock provider, outbox writer.
- Implements interfaces from Application/Core.

4. Adapters/UI
- Console adapter now.
- Future API and web adapters call same use cases.

## Core Interfaces To Introduce In The Clean-Core Pass

- IBookingService (or use-case specific interfaces)
- IBookingRepository
- IOutboxRepository
- IClock
- Optional later: IUserInteraction for console abstraction

## Phased Plan

### Phase 0: Baseline

- Run the app and document expected behavior for:
- Day navigation
- Booking success path
- Invalid input handling
- Outbox append on success
- Add minimal tests around booking rules to lock behavior.

### Phase 1: Clean Core Boundary At Entry

- Make BookingApp depend on IBookingService instead of BookingService concrete type.
- Keep behavior unchanged.
- Use Program as composition root.

Deliverable:
- BookingApp constructor receives IBookingService.

### Phase 2: Clean Core Boundary At Persistence

- Introduce IBookingRepository and IOutboxRepository.
- Convert static JSON repositories to instance classes implementing interfaces.
- Inject these into BookingService.

Deliverable:
- BookingService has no direct dependency on static repository classes.

### Phase 3: Clean Core Boundary At Time

- Introduce IClock for current date/time.
- Use IClock where today/utc-now are currently read.
- Keep schedule and outbox behavior equivalent.

Deliverable:
- No direct DateTime.Today/UtcNow in application flow.

### Phase 4: Clean Core Service Purification

- Keep Console interaction in BookingApp (or a console adapter), not inside BookingService.
- BookingService should accept validated inputs and return results/errors.

Deliverable:
- BookingService can be called from API/web without Console coupling.

### Phase 5: Optional API Adapter Pilot

- Add a thin API endpoint for one use case (list day status or create booking).
- Reuse existing application service and interfaces.

Deliverable:
- Proof that core logic is reusable without duplication.

## SOLID Hardening Pass (After Clean Core)

### S1: SRP Tightening

- Keep BookingService focused on orchestration only.
- Keep validation/rule logic in domain types or dedicated policy classes.

### S2: OCP And Strategy Readiness

- Introduce extension points only where there is real variation expected.
- Example: booking policy strategies for special opening hours.

### S3: DIP Completion

- Review remaining concrete dependencies in application/core.
- Ensure all infrastructure concerns are behind interfaces.

### S4: ISP/LSP Cleanup

- Split broad interfaces if consumers use only subsets.
- Verify substitutions preserve expected behavior.

## Suggested Result Contract For Use Cases

Use explicit results instead of writing messages directly:

- Success with payload
- Validation error with reason
- Conflict/not-available result

This keeps UI decisions (text, status codes) outside core/application.

## Testing Strategy

1. Unit tests for core rules (Schedule/booking constraints).
2. Unit tests for application service with mocked repositories and clock.
3. Integration tests for JSON repositories using temp files.
4. Smoke test for Console adapter behavior.

## Done Criteria

- Booking logic callable without Console.
- Persistence implementation swappable behind interfaces.
- Time source injectable.
- Existing behavior preserved.
- Core/application tests run without disk or console side effects.

## Risks And Mitigation

- Risk: Behavior drift during constructor and dependency changes.
- Mitigation: Small phases and behavior-preserving tests.

- Risk: Over-abstraction too early.
- Mitigation: Introduce only interfaces used by current seams.

- Risk: Refactor stalls before value is visible.
- Mitigation: Deliver value every phase (especially phases 1-2).
