# SOLID Hardening Phase Plan

This document is the second pass after the clean-core refactor. The objective is to strengthen maintainability, testability, and change safety without adding unnecessary abstraction.

## Scope And Timing

- Start this phase only after clean-core boundaries are in place.
- Keep refactors behavior-preserving.
- Favor small pull requests per principle area.

## Goals

- Clarify responsibilities for each class.
- Make dependencies explicit and injectable.
- Keep interfaces focused on consumer needs.
- Ensure substitutions behave consistently.
- Enable new features through extension instead of modification.

## Baseline Assumptions

Before starting:

- Business logic is callable without Console or file I/O coupling.
- Repositories and time concerns are behind abstractions.
- Composition root exists in Program.

If any assumption is not true, complete the clean-core tasks first.

## S1: SRP (Single Responsibility Principle)

### Current pressure points

- BookingService currently risks carrying orchestration, validation messaging, and user-facing concerns.
- BookingApp can accumulate UI flow and application decision logic over time.

### Actions

- Keep BookingService focused on application orchestration only.
- Keep user interaction text/formatting in the console adapter or BookingApp.
- Move reusable validation/rule decisions into domain policies or dedicated validators when logic grows.

### Done criteria

- BookingService contains no Console reads/writes.
- Error/result translation is outside domain logic.
- Rule-heavy methods are covered by focused unit tests.

## S2: OCP (Open/Closed Principle)

### Current pressure points

- Rule variation (special opening hours, blackout dates, resource types) may force edits to core classes.

### Actions

- Introduce extension points only where variation is expected.
- Add a booking policy strategy seam for optional rule sets.
- Keep default behavior as the existing policy to avoid regressions.

### Done criteria

- New rule variants can be added via new policy implementations.
- Existing core classes do not require edits for common policy additions.

## S3: LSP (Liskov Substitution Principle)

### Current pressure points

- Repository/interface implementations can drift in behavior (null handling, ordering, conflict behavior).

### Actions

- Define contract expectations for each interface method.
- Add contract tests shared across repository implementations.
- Ensure all implementations preserve domain invariants and return semantics.

### Done criteria

- Any IBookingRepository implementation passes the same contract test suite.
- Any IOutboxRepository implementation preserves append semantics.

## S4: ISP (Interface Segregation Principle)

### Current pressure points

- Growth can push broad service/repository interfaces that force unused members on consumers.

### Actions

- Keep interfaces minimal and use-case focused.
- Split broad interfaces when consumers depend on only a subset.
- Prefer query/command separation when it reduces coupling.

### Done criteria

- Consumers depend only on members they actually use.
- No adapter classes exist solely to ignore unused interface members.

## S5: DIP (Dependency Inversion Principle) Completion

### Current pressure points

- Residual direct concrete dependencies may remain after clean-core extraction.

### Actions

- Audit constructors in BookingApp, BookingService, and infrastructure classes.
- Replace remaining concrete dependencies in high-level modules with abstractions.
- Keep framework and storage dependencies in outer layers.

### Done criteria

- High-level modules depend on interfaces, not concrete infrastructure types.
- New infrastructure implementations can be introduced without modifying core/application classes.

## Class-Level Mapping For This Repository

- Program: composition root only; no business logic.
- BookingApp: UI flow and presentation concerns only.
- BookingService: orchestrates use cases through ports.
- Schedule/Booking/HourStatus: domain rules and state only.
- JsonBookingRepository/JsonOutboxRepository: infrastructure implementations behind interfaces.
- OutboxMessage: domain event model; serialization concerns stay outside domain when practical.

## Suggested Work Order

1. SRP cleanup in BookingService and BookingApp.
2. DIP audit and constructor cleanup.
3. ISP split where consumers reveal broad interfaces.
4. LSP contract tests for repositories.
5. OCP extension seam for future booking policy variation.

## Test Strategy For SOLID Phase

- Unit tests for use-case orchestration and domain rules.
- Contract tests for each interface implementation.
- Integration tests for JSON-backed repositories.
- One end-to-end smoke test for console flow.

## Exit Criteria

- SOLID concerns are explicit and verified by tests.
- Core/application code remains framework-agnostic.
- Architecture can support console + API/web adapters with shared logic.
