# SimpleBooking

SimpleBooking is a small .NET console application for viewing available booking hours, creating bookings, and writing booking confirmation requests to an outbox file.

The diagram below shows the current structure of the codebase as implemented today. It focuses on the main classes, their responsibilities, and how data and side effects move through the application.

## UML Class Diagram

```mermaid
classDiagram
    class Program {
        +Main()
    }

    class BookingApp {
        -BookingService _bookingService
        -DateOnly _today
        -DateOnly _currentDate
        +Run()
        -ShowCurrentDay()
    }

    class BookingService {
        -Schedule _schedule
        +GetDayStatus(date)
        +BookHour(date)
        -ShowMessage(message)
    }

    class JsonBookingRepository {
        <<static>>
        +GetAll()
        +Add(booking)
    }

    class JsonOutboxRepository {
        <<static>>
        +GetAll()
        +Append(message)
    }

    class Schedule {
        +OpeningHour
        +ClosingHour
        +GetDayStatus(date)
        +TryAddBooking(booking)
    }

    class Booking {
        +Guid Id
        +DateOnly Date
        +int Hour
        +string Description
        +OverlapsWith(other)
    }

    class HourStatus {
        +int Hour
        +bool IsAvailable
        +string Description
    }

    class OutboxMessage {
        +Guid Id
        +string Type
        +DateTime CreatedAt
        +string Payload
        +CreateBookingConfirmation(booking)
    }

    Program --> BookingApp : creates
    BookingApp --> BookingService : uses
    BookingService *-- Schedule : owns
    BookingService ..> JsonBookingRepository : loads and saves
    BookingService ..> JsonOutboxRepository : appends events
    Schedule o-- Booking : contains
    Schedule ..> HourStatus : returns
    JsonBookingRepository ..> Booking : persists
    JsonOutboxRepository ..> OutboxMessage : persists
    OutboxMessage ..> Booking : built from
```

## Architecture Notes

The application starts in `Program`, which creates `BookingApp`. `BookingApp` is the console-driven UI loop and delegates booking operations to `BookingService`.

`BookingService` coordinates the booking flow. It loads bookings from `JsonBookingRepository`, applies booking rules through `Schedule`, persists successful bookings back to the booking repository, and appends an `OutboxMessage` through `JsonOutboxRepository`.

The domain model lives in `Model`:

- `Schedule` contains booking rules and availability calculations.
- `Booking` represents a single reserved hour.
- `HourStatus` represents the availability of one hour slot.
- `OutboxMessage` represents an integration event written to the outbox file.

## Side Effects In The Current Design

The current implementation mixes business logic and side effects in a few places:

- Console I/O happens in `BookingApp` and `BookingService`.
- File I/O happens in `JsonBookingRepository` and `JsonOutboxRepository`.
- System time is read directly when creating the schedule and when creating outbox messages.

That makes this structure a good candidate for a later clean-core refactor, but this README documents the current design as it exists now.
## Terje specs 
Applikasjonen er et veldig enkelt bookingsystem.

Tenk dere for eksempel:

et møterom

eller et bord på en restaurant

…som kan bookes på følgende måte:

Det finnes kun én ressurs

Åpningstid er 08–16 (timer 08–15 kan bookes)

Man booker hele timer

Man kan ikke booke i fortiden

Man kan kun booke fra og med dagen etter i dag

Hver booking består av:

dato

time

en enkel tekst (beskrivelse)

Applikasjonen lar deg:

bla mellom dager

se status for alle timer (ledig/opptatt)

legge inn en booking

Ved vellykket booking:

lagres den til en JSON-fil

det skrives også en melding til en “outbox” (simulering av f.eks. e-post)

Hva dere skal gjøre
Før møtet ønsker jeg at dere:

Kjører applikasjonen og blir kjent med funksjonaliteten

Leser gjennom koden

Prøver å forstå hvordan den er bygget opp

Og viktigst:

👉 Begynn å reflektere over:

Hva er det som ikke er så bra med denne koden?
Hva er det som skurrer litt?

Viktig presisering
Dette handler ikke primært om at koden ikke er perfekt objektorientert.

Koden er bevisst laget som:

ryddig

forståelig

ganske “normal”

Men det er noe mer grunnleggende som ikke er helt riktig.

Tenk spesielt på dette
Se for dere at vi på sikt skal:

bygge et API (backend)

lagre data i en database

lage en frontend (web)

👉 Spørsmålet er:

Hvordan måtte denne koden vært for at vi kunne gjenbruke logikken i et API?

Og dermed:

Hva er det i denne koden som gjør det vanskelig å gjenbruke den?