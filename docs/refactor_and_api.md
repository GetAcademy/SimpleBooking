# Refactor + API + SQLite — gjennomgang

## Mål

- Rense kjernelogikken for side effects (Console, DateTime, fil-I/O)
- Gjøre det mulig å kjøre samme bookinglogikk fra både konsoll og API
- Erstatte JSON-filer med SQLite-database
- Beholde konsoll-appen som et alternativ

---

## Steg 1: Interfaces i Core

**Hvorfor:** BookingService og BookingApp var tett koblet til konkrete JSON-klasser og `DateTime.Today`. Ved å definere interfaces i Core kan vi bytte persistens uten å endre forretningslogikken.

**Filer opprettet i `SimpleBooking.Core/AppService/`:**

| Fil | Hensikt |
|---|---|
| `IBookingRepository.cs` | `GetAll()` og `Add(Booking)` |
| `IOutboxRepository.cs` | `Append(BookingConfirmationRequested)` |
| `IClock.cs` | `Today` (abstraherer `DateTime.Today`) |

**Endringer i `SimpleBooking.Core/Model/Booking.cs`:**
- La til `private set;` på properties (påkrevd for EF Core)
- La til privat parameterløs constructor (påkrevd for EF Core)

---

## Steg 2: BookingService bruker interfaces

**Før:** `BookingService(IEnumerable<Booking> bookings, DateOnly today)`  
**Etter:** `BookingService(IBookingRepository, IOutboxRepository, IClock)`

BookingService leser nå alltid fra repository via `BuildSchedule()` på hvert kall. Persistering skjer internt i `BookHour()`, slik at konsumenten (BookingApp / API) ikke trenger å forholde seg til repositories direkte.

```csharp
// Før: BookingApp måtte kalle repos selv
var result = _bookingService.BookHour(date, hour, desc);
_bookingRepository.Add(result.Booking!);
_outboxRepository.Append(result.BookingConfirmationRequested!);

// Etter: BookingService håndterer alt
var result = _bookingService.BookHour(date, hour, desc);
```

---

## Steg 3: BookingApp og Program.cs renses

**BookingApp** tar nå kun `BookingService` og `IClock` — trenger ikke lenger repository-avhengigheter.

**Program.cs** (console) er composition rooten:

```csharp
var bookingRepository = new JsonBookingRepository();
var outboxRepository = new JsonOutboxRepository();
var clock = new SystemClock();
var bookingService = new BookingService(bookingRepository, outboxRepository, clock);
var app = new BookingApp(bookingService, clock);
app.Run();
```

---

## Steg 4: SystemClock flyttes til Infrastructure

`SystemClock` (`IClock` → `DateTime.Today`) flyttet fra `SimpleBooking/Infrastructure/` til `SimpleBooking.Infrastructure/` slik at både console-app og API kan bruke den.

---

## Steg 5: Forretningslogikk-tester utvidet

I stedet for en mocking-ramme (Moq/NSubstitute) ble det laget enkle test-doubles i testfilen:

| Klasse | Implementerer | Beskrivelse |
|---|---|---|
| `InMemoryBookingRepository` | `IBookingRepository` | Holder bookings i en `List<Booking>` |
| `InMemoryOutboxRepository` | `IOutboxRepository` | Samler opp `BookingConfirmationRequested`-meldinger |
| `FakeClock` | `IClock` | Returnerer forhåndsinnstilt `Today`-verdi |

BookingService-konstruktøren endret fra `(IEnumerable<Booking>, DateOnly)` til `(IBookingRepository, IOutboxRepository, IClock)`, så testene ble oppdatert til å bruke test-doubles i stedet for raw data.

**Nye testscenarioer lagt til (25 totalt, opp fra 9):**

| Kategori | Antall | Hva testes |
|---|---|---|
| `GetDayStatus` | 4 | Alle ledige, alle opptatt, mixed, med eksisterende booking |
| `BookHour` — success | 7 | Trimming, persistering, outbox, hour 8, hour 15, fremtidig, vises som booket |
| `BookHour` — rejection | 8 | Fortid, i dag, hour 7, hour 16, duplikat, tom/whitespace/null description, clock fremover |
| `OverlapsWith` | 4 | Samme, ulik time, ulik dato, symmetri |

---

## Steg 6: SimpleBooking.Infrastructure (EF Core + SQLite)

### Prosjekt

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.7" />
```

### BookingDbContext

- `DbSet<Booking>` → tabellen `Bookings`
- `DbSet<OutboxMessage>` → tabellen `OutboxMessages`
- Unik-indeks på `(Date, Hour)` i `Bookings`

### SqlBookingRepository

Implementerer `IBookingRepository`:

```csharp
public List<Booking> GetAll() => _db.Bookings.ToList();
public void Add(Booking booking) { _db.Bookings.Add(booking); _db.SaveChanges(); }
```

### SqlOutboxRepository

Implementerer `IOutboxRepository`:

```csharp
public void Append(BookingConfirmationRequested confirmation)
{
    _db.OutboxMessages.Add(new OutboxMessage { ... });
    _db.SaveChanges();
}
```

### `OutboxMessage`-entitet

```csharp
public class OutboxMessage {
    public Guid Id { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Payload { get; set; }   // JSON-serialisert BookingConfirmationRequested
}
```

---

## Steg 7: SimpleBooking.Api (ASP.NET Core Web API)

### Endepunkter

| Metode | Rute | Beskrivelse | Respons |
|---|---|---|---|
| `GET` | `/api/schedule/{date}` | Hent status for alle timer (08–15) | `200` + `List<HourStatus>` |
| `POST` | `/api/bookings` | Opprett booking | `201 Created` / `400` / `409` / `422` |

### Eksempler

**GET /api/schedule/2026-05-06**
```json
[
  {"hour":8,"isAvailable":true,"description":null},
  {"hour":9,"isAvailable":true,"description":null},
  {"hour":10,"isAvailable":false,"description":"Teammøte"},
  ...
]
```

**POST /api/bookings**
```json
// Request
{"date":"2026-05-06","hour":14,"description":"Standup"}

// Response 201
{"id":"guid","date":"2026-05-06","hour":14,"description":"Standup"}
```

**Feilhåndtering:**
| Status | `error`-felt | Når |
|---|---|---|
| `400` | `MissingDescription` | Tom/whitespace-beskrivelse |
| `409` | `HourAlreadyBooked` | Timen er allerede booket |
| `422` | `NotBookable` | Utenfor åpningstid / fortid |

### DI-konfigurasjon i API

```csharp
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite("Data Source=SimpleBooking.db"));
builder.Services.AddScoped<IBookingRepository, SqlBookingRepository>();
builder.Services.AddScoped<IOutboxRepository, SqlOutboxRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<BookingService>();
```

Databasen opprettes automatisk ved oppstart via `db.Database.EnsureCreated()`.

---

## Steg 8: API-endepunkt for henting av bookinger

`GET /api/bookings` — returnerer alle bookinger. Lagt til i `BookingsController.cs` for å kunne liste opp bookinger via API-et i tillegg til å opprette nye.

---

## Steg 9: API-integrasjonstester

### Prosjekt: `SimpleBooking.Api.Tests`

Bruker `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) til å spinne opp API-et i minnet med en midlertidig SQLite-database (temp-fil som slettes ved dispose).

### Testoppsett

`BookingApiFactory` arver `WebApplicationFactory<Program>` og overstyrer DbContext til å bruke en unik temp-fil:

```csharp
services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite($"Data Source={_dbPath}"));
```

Hver testklasse får sin egen factory-instans (`OneTimeSetUp`/`OneTimeTearDown`), så databasen er isolert per testkjørsel.

### API-tester (13 stykker)

| Kategori | Antall | Hva testes |
|---|---|---|
| `GET /api/schedule/{date}` | 3 | 200 OK, 8 slots, boundary (8–15) |
| `POST /api/bookings` — success | 4 | 201 Created, hour 8, hour 15, vises som booket i påfølgende GET |
| `POST /api/bookings` — validation | 4 | 400 (tom/whitespace), 422 (fortid/utetid) |
| `POST /api/bookings` — conflict | 1 | 409 ved duplikat time |
| `GET /api/bookings` | 2 | 200 OK, inkluderer nye bookinger |

---

## Steg 10: Løsningsfil oppdatert

```xml
<Solution>
  <Project Path="SimpleBooking/SimpleBooking.csproj" />
  <Project Path="SimpleBooking.Core/SimpleBooking.Core.csproj" />
  <Project Path="SimpleBooking.Core.Tests/SimpleBooking.Core.Tests.csproj" />
  <Project Path="SimpleBooking.Infrastructure/SimpleBooking.Infrastructure.csproj" />
  <Project Path="SimpleBooking.Api/SimpleBooking.Api.csproj" />
</Solution>
```

Console-appen refererer til `SimpleBooking.Core` og `SimpleBooking.Infrastructure`.  
API-et refererer til `SimpleBooking.Core` og `SimpleBooking.Infrastructure`.  
`SimpleBooking.Core.Tests` refererer til `SimpleBooking.Core`.  
`SimpleBooking.Api.Tests` refererer til `SimpleBooking.Api`.

---

## Sluttarkitektur

```
SimpleBooking/               (Console UI — beholdt)
├── BookingApp.cs            ← kun UI, ingen persistens
├── Infrastructure/
│   ├── JsonBookingRepository.cs  ← implementerer IBookingRepository
│   └── JsonOutboxRepository.cs   ← implementerer IOutboxRepository
└── Program.cs               ← composition root (JSON-variant)

SimpleBooking.Core/          (Domene + AppService — uendret logikk)
├── AppService/
│   ├── IBookingRepository.cs    ← port
│   ├── IOutboxRepository.cs     ← port
│   ├── IClock.cs                ← port
│   ├── BookingService.cs        ← use case, bruker ports
│   └── BookHourResult.cs
├── Model/
│   ├── Booking.cs
│   ├── Schedule.cs
│   ├── HourStatus.cs
│   ├── BookingFailureReason.cs
│   └── BookingConfirmationRequested.cs
└── ...

SimpleBooking.Infrastructure/ (SQLite — ny)
├── BookingDbContext.cs
├── OutboxMessage.cs
├── SqlBookingRepository.cs      ← implementerer IBookingRepository
├── SqlOutboxRepository.cs       ← implementerer IOutboxRepository
└── SystemClock.cs               ← implementerer IClock

SimpleBooking.Api/            (Web API — ny)
├── Controllers/
│   ├── ScheduleController.cs
│   └── BookingsController.cs
├── Program.cs                ← composition root (SQLite-variant)
└── appsettings.json          ← connection string

SimpleBooking.Core.Tests/     (Enhetstester — utvidet)
├── BookingServiceTests.cs    ← 25 tester, in-memory doubles

SimpleBooking.Api.Tests/      (Integrasjonstester — ny)
├── BookingApiFactory.cs      ← WebApplicationFactory med SQLite temp-db
├── ApiTests.cs               ← 13 tester mot API-endepunkter
```

---

## Verifikasjon

| Test | Resultat |
|---|---|
| `dotnet build` | 0 warnings, 0 errors |
| `dotnet test` | **38/38 passed** (25 forretningslogikk + 13 API) |
| Konsoll-app | Bygger og kjører (JSON) |
| API `GET /api/schedule/{date}` | Returnerer riktig status |
| API `POST /api/bookings` (ny) | 201 Created |
| API `POST /api/bookings` (duplikat) | 409 Conflict |
| API `POST /api/bookings` (utenfor åpningstid) | 422 Unprocessable |
