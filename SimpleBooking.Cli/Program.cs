using SimpleBooking.Cli;

const string BaseUrl = "http://localhost:5106/api";

using var client = new HttpClient();
var bookingClient = new BookingClient(client, BaseUrl);
var today = DateOnly.FromDateTime(DateTime.Today);

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== SimpleBooking CLI ===");
    Console.WriteLine("1. List alle bookings");
    Console.WriteLine("2. Se timeplan for en dag");
    Console.WriteLine("3. Opprett booking");
    Console.WriteLine("q. Avslutt");
    Console.Write("Valg: ");

    var choice = Console.ReadLine()?.Trim();

    try
    {
        switch (choice)
        {
            case "1":
                await ListBookings();
                break;
            case "2":
                await ViewSchedule();
                break;
            case "3":
                await CreateBooking();
                break;
            case "q":
                return;
            default:
                Console.WriteLine("Ugyldig valg.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Feil: {ex.Message}");
    }
}

async Task ListBookings()
{
    var bookings = await bookingClient.GetBookingsAsync();
    if (bookings.Count == 0)
    {
        Console.WriteLine("Ingen bookings funnet.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"{"ID",-36} {"Dato",-12} {"Time",-5} Beskrivelse");
    Console.WriteLine(new string('-', 70));
    foreach (var b in bookings)
    {
        Console.WriteLine($"{b.Id,-36} {b.Date,-12} {b.Hour,-5} {b.Description}");
    }
}

async Task ViewSchedule()
{
    var date = ReadDate("Dato (yyyy-MM-dd): ");
    if (!date.HasValue) return;

    await ShowSchedule(date.Value);
}

async Task ShowSchedule(DateOnly date)
{
    var slots = await bookingClient.GetScheduleAsync(date);
    if (slots.Count == 0)
    {
        Console.WriteLine("Ingen timeinformasjon funnet.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Timeplan for {date:yyyy-MM-dd}:");
    Console.WriteLine($"{"Time",-6} {"Status",-12} Beskrivelse");
    Console.WriteLine(new string('-', 40));
    foreach (var s in slots)
    {
        var status = s.IsAvailable ? "Ledig" : "Opptatt";
        Console.WriteLine($"{s.Hour,-6} {status,-12} {s.Description ?? ""}");
    }
}

async Task CreateBooking()
{
    var date = ReadDate("Dato (yyyy-MM-dd): ");
    if (!date.HasValue) return;

    var dateError = BookingValidator.ValidateDate(date.Value, today);
    if (dateError != null)
    {
        Console.WriteLine($"Feil: {dateError}");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Timeplan for valgt dato:");
    await ShowSchedule(date.Value);
    Console.WriteLine();

    var hour = ReadHour("Time (8-15): ");
    if (!hour.HasValue) return;

    var hourError = BookingValidator.ValidateHour(hour.Value);
    if (hourError != null)
    {
        Console.WriteLine($"Feil: {hourError}");
        return;
    }

    Console.Write("Beskrivelse: ");
    var description = Console.ReadLine()?.Trim() ?? "";

    var descError = BookingValidator.ValidateDescription(description);
    if (descError != null)
    {
        Console.WriteLine($"Feil: {descError}");
        return;
    }

    var result = await bookingClient.CreateBookingAsync(date.Value, hour.Value, description);

    if (result.Success)
    {
        Console.WriteLine($"Booking opprettet! ID: {result.Booking!.Id}");
    }
    else
    {
        Console.WriteLine($"Feil: {result.ErrorMessage}");
    }
}

DateOnly? ReadDate(string prompt)
{
    Console.Write(prompt);
    var input = Console.ReadLine()?.Trim();
    if (!DateOnly.TryParse(input, out var date))
    {
        Console.WriteLine("Ugyldig dato.");
        return null;
    }
    return date;
}

int? ReadHour(string prompt)
{
    Console.Write(prompt);
    var input = Console.ReadLine()?.Trim();
    if (!int.TryParse(input, out var hour))
    {
        Console.WriteLine("Ugyldig time.");
        return null;
    }
    return hour;
}
