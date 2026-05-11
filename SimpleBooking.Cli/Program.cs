using System.Net.Http.Json;

const string BaseUrl = "http://localhost:5106/api";

using var client = new HttpClient();

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
    var response = await client.GetAsync($"{BaseUrl}/bookings");
    response.EnsureSuccessStatusCode();

    var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
    if (bookings == null || bookings.Count == 0)
    {
        Console.WriteLine("Ingen bookings funnet.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"{"ID",-5} {"Dato",-12} {"Time",-5} Beskrivelse");
    Console.WriteLine(new string('-', 50));
    foreach (var b in bookings)
    {
        Console.WriteLine($"{b.Id,-5} {b.Date,-12} {b.Hour,-5} {b.Description}");
    }
}

async Task ViewSchedule()
{
    Console.Write("Dato (yyyy-MM-dd): ");
    var dateInput = Console.ReadLine()?.Trim();
    if (!DateOnly.TryParse(dateInput, out var date))
    {
        Console.WriteLine("Ugyldig dato.");
        return;
    }

    var response = await client.GetAsync($"{BaseUrl}/schedule/{date:yyyy-MM-dd}");
    response.EnsureSuccessStatusCode();

    var slots = await response.Content.ReadFromJsonAsync<List<HourSlot>>();
    if (slots == null || slots.Count == 0)
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
    Console.Write("Dato (yyyy-MM-dd): ");
    var dateInput = Console.ReadLine()?.Trim();
    Console.Write("Time (0-23): ");
    var hourInput = Console.ReadLine()?.Trim();
    Console.Write("Beskrivelse: ");
    var description = Console.ReadLine()?.Trim();

    if (!DateOnly.TryParse(dateInput, out var date))
    {
        Console.WriteLine("Ugyldig dato.");
        return;
    }

    if (!int.TryParse(hourInput, out var hour))
    {
        Console.WriteLine("Ugyldig time.");
        return;
    }

    var request = new { Date = date, Hour = hour, Description = description };
    var response = await client.PostAsJsonAsync($"{BaseUrl}/bookings", request);

    if (response.StatusCode == System.Net.HttpStatusCode.Created)
    {
        var booking = await response.Content.ReadFromJsonAsync<Booking>();
        Console.WriteLine($"Booking opprettet! ID: {booking?.Id}");
    }
    else
    {
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Console.WriteLine($"Feil: {error?.Message ?? response.ReasonPhrase}");
    }
}

record Booking(Guid Id, DateOnly Date, int Hour, string Description);
record HourSlot(int Hour, bool IsAvailable, string? Description);
record ErrorResponse(string Error, string Message);
