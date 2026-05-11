# SimpleBooking

Single-resource booking system (.NET 10) with clean-core architecture. One resource, whole-hour slots, 08:00–15:00.

## Quick Start

```bash
# API
dotnet run --project SimpleBooking.Api      # http://localhost:5106

# CLI client (needs API running)
dotnet run --project SimpleBooking.Cli

# Tests
dotnet test
```

## Architecture

Clean core: domain logic in `SimpleBooking.Core` with zero I/O. Infrastructure, API, and console apps depend on Core.

| Project | Type | Stores |
|---------|------|--------|
| `SimpleBooking.Core` | Class library | Domain models, `BookingService`, repository interfaces |
| `SimpleBooking.Infrastructure` | Class library | EF Core SQLite, `SqlBookingRepository`, `JsonBookingRepository` |
| `SimpleBooking.Api` | ASP.NET Core | REST API with SQLite DB |
| `SimpleBooking.Cli` | Console app | HTTP client — calls API, no project refs |
| `SimpleBooking` | Console app | Legacy app with JSON repos |
| `*.Tests` | NUnit 4.3.2 | Unit + integration tests, hand-rolled fakes |

## API

| Method | Path | Body |
|--------|------|------|
| GET | `/api/bookings` | — |
| POST | `/api/bookings` | `{"date":"2026-05-12","hour":10,"description":"Teammøte"}` |
| GET | `/api/schedule/2026-05-12` | — |

## Domain Rules

- Opening hours: 08–15 (8 slots)
- Cannot book today or past dates
- One resource — no overlapping bookings
- `Booking.Id` is `Guid`
- `HourStatus.IsAvailable` — `true` means free

## Testing

- **No mocking library** (Moq/NSubstitute). Hand-roll fakes in test files
- **Unit tests**: `CreateService(...)` factory, `FakeClock` for time
- **Integration tests**: `WebApplicationFactory<Program>` with temp SQLite DB
- Use `[TestCase]` for boundaries

## Notes

- Solution file is `.slnx` (XML), not `.sln`
- API auto-creates DB via `EnsureCreated()` on startup
- `Program` in API is `public partial` for `WebApplicationFactory`
