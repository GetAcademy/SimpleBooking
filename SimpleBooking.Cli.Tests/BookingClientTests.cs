using System.Net;
using System.Text.Json;
using NUnit.Framework;
using SimpleBooking.Cli;

namespace SimpleBooking.Cli.Tests;

public class BookingClientTests
{
    private static HttpClient CreateFakeClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var fakeHandler = new FakeHttpMessageHandler(handler);
        return new HttpClient(fakeHandler) { BaseAddress = new Uri("http://localhost:5106/api") };
    }

    [Test]
    public async Task GetBookingsAsync_returns_deserialized_list()
    {
        var bookings = new List<Booking>
        {
            new(Guid.NewGuid(), new DateOnly(2026, 5, 12), 10, "Møte")
        };

        var httpClient = CreateFakeClient(req =>
        {
            Assert.That(req.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(req.RequestUri?.ToString(), Does.EndWith("/bookings"));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(bookings))
            };
        });

        var client = new BookingClient(httpClient, "http://localhost:5106/api");
        var result = await client.GetBookingsAsync();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Hour, Is.EqualTo(10));
    }

    [Test]
    public async Task GetScheduleAsync_returns_hour_slots()
    {
        var slots = new List<HourSlot>
        {
            new(8, true, null),
            new(9, false, "Booket")
        };

        var httpClient = CreateFakeClient(req =>
        {
            Assert.That(req.RequestUri?.ToString(), Does.EndWith("/schedule/2026-05-12"));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(slots))
            };
        });

        var client = new BookingClient(httpClient, "http://localhost:5106/api");
        var result = await client.GetScheduleAsync(new DateOnly(2026, 5, 12));

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].IsAvailable, Is.True);
        Assert.That(result[1].IsAvailable, Is.False);
    }

    [Test]
    public async Task CreateBookingAsync_success_returns_booking()
    {
        var booking = new Booking(Guid.NewGuid(), new DateOnly(2026, 5, 12), 10, "Møte");

        var httpClient = CreateFakeClient(req =>
        {
            Assert.That(req.Method, Is.EqualTo(HttpMethod.Post));
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(booking))
            };
        });

        var client = new BookingClient(httpClient, "http://localhost:5106/api");
        var result = await client.CreateBookingAsync(new DateOnly(2026, 5, 12), 10, "Møte");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Booking, Is.Not.Null);
        Assert.That(result.Booking!.Hour, Is.EqualTo(10));
    }

    [Test]
    public async Task CreateBookingAsync_conflict_returns_failure()
    {
        var error = new ErrorResponse("HourAlreadyBooked", "Timen er allerede booket.");

        var httpClient = CreateFakeClient(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent(JsonSerializer.Serialize(error))
            };
        });

        var client = new BookingClient(httpClient, "http://localhost:5106/api");
        var result = await client.CreateBookingAsync(new DateOnly(2026, 5, 12), 10, "Møte");

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Timen er allerede booket."));
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
