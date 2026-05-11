using System.Net;
using System.Net.Http.Json;

namespace SimpleBooking.Cli;

public class BookingClient
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public BookingClient(HttpClient client, string baseUrl)
    {
        _client = client;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<List<Booking>> GetBookingsAsync()
    {
        var response = await _client.GetAsync($"{_baseUrl}/bookings");
        response.EnsureSuccessStatusCode();
        var bookings = await response.Content.ReadFromJsonAsync<List<Booking>>();
        return bookings ?? new List<Booking>();
    }

    public async Task<List<HourSlot>> GetScheduleAsync(DateOnly date)
    {
        var response = await _client.GetAsync($"{_baseUrl}/schedule/{date:yyyy-MM-dd}");
        response.EnsureSuccessStatusCode();
        var slots = await response.Content.ReadFromJsonAsync<List<HourSlot>>();
        return slots ?? new List<HourSlot>();
    }

    public async Task<BookingResult> CreateBookingAsync(DateOnly date, int hour, string description)
    {
        var request = new { Date = date, Hour = hour, Description = description };
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/bookings", request);

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var booking = await response.Content.ReadFromJsonAsync<Booking>();
            return BookingResult.Ok(booking!);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return BookingResult.Failed(error?.Message ?? response.ReasonPhrase);
    }
}

public record Booking(Guid Id, DateOnly Date, int Hour, string Description);
public record HourSlot(int Hour, bool IsAvailable, string? Description);
public record ErrorResponse(string Error, string Message);

public record BookingResult
{
    public bool Success { get; }
    public Booking? Booking { get; }
    public string? ErrorMessage { get; }

    private BookingResult(bool success, Booking? booking, string? errorMessage)
    {
        Success = success;
        Booking = booking;
        ErrorMessage = errorMessage;
    }

    public static BookingResult Ok(Booking booking) => new(true, booking, null);
    public static BookingResult Failed(string errorMessage) => new(false, null, errorMessage);
}
