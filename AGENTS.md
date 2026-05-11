# SimpleBooking - Agent Guide

## Architecture

.NET 10 solution with a clean-core design. Core contains domain logic with zero I/O. Infrastructure, API, and console apps all depend on Core.

| Project | Type | Notes |
|---------|------|-------|
| `SimpleBooking.Core` | Class library | Domain models (`Booking`, `Schedule`, `HourStatus`), `BookingService`, repository interfaces (`IBookingRepository`, `IOutboxRepository`, `IClock`) |
| `SimpleBooking.Infrastructure` | Class library | EF Core `BookingDbContext`, `SqlBookingRepository`, `JsonBookingRepository`, `SystemClock` |
| `SimpleBooking.Api` | ASP.NET Core Web | Controllers: `BookingsController` (`/api/bookings`), `ScheduleController` (`/api/schedule/{date}`). Uses SQLite in dev |
| `SimpleBooking.Cli` | Console app | HTTP client that calls the API. No project references to other projects |
| `SimpleBooking` | Console app | Legacy original app. Directly instantiates JSON repos |
| `SimpleBooking.Core.Tests` | NUnit unit tests | Hand-rolled fakes (`InMemoryBookingRepository`, `FakeClock`). No mocking library |
| `SimpleBooking.Api.Tests` | NUnit integration tests | Uses `WebApplicationFactory<Program>` with temp-file SQLite DB |

## Developer Commands

```bash
# Run API (http://localhost:5106)
dotnet run --project SimpleBooking.Api

# Run CLI client (requires API running)
dotnet run --project SimpleBooking.Cli

# Run legacy console app
dotnet run --project SimpleBooking

# Run all tests
dotnet test

# Run specific test project
dotnet test SimpleBooking.Core.Tests/
dotnet test SimpleBooking.Api.Tests/
```

## API Endpoints

| Method | Path | Body |
|--------|------|------|
| GET | `/api/bookings` | - |
| POST | `/api/bookings` | `{"date":"2026-05-12","hour":10,"description":"Teammøte"}` |
| GET | `/api/schedule/2026-05-12` | - |

## Testing Conventions

- **Framework**: NUnit 4.3.2 with constraint model: `Assert.That(actual, Is.EqualTo(expected))`
- **No mocking library** (Moq, NSubstitute). Write hand-rolled fakes in test files
- **Unit tests**: Factory method `CreateService(...)` for SUT setup. Use `FakeClock` to freeze time
- **Integration tests**: `BookingApiFactory` extends `WebApplicationFactory<Program>`, swaps DbContext for SQLite temp file, cleans up on dispose
- **Parameterized tests**: Use `[TestCase]` for boundary conditions

## Domain Rules (Hardcoded)

- Opening hours: 08:00–15:00 (8 slots)
- Cannot book today or past dates
- One resource only — no overlapping bookings allowed
- `Booking.Id` is a `Guid`, not `int`
- `HourStatus.IsAvailable` (not `IsBooked`) — `true` means slot is free

## Quirks & Gotchas

- Solution file is **`.slnx`** (XML format), not `.sln`
- API auto-creates SQLite DB via `EnsureCreated()` on startup
- `Booking` has a private parameterless constructor for EF Core
- `JsonBookingRepository` persists to `bookings.json` in working directory
- `JsonOutboxRepository` persists to `outbox.json` in working directory
- The `Program` class in API is marked `public partial` so `WebApplicationFactory` can reference it
