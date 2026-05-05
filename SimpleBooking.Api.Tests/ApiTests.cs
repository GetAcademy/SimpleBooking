using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace SimpleBooking.Api.Tests
{
    [TestFixture]
    public class ApiTests
    {
        private BookingApiFactory _factory = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new BookingApiFactory();
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetSchedule_returns_200_with_8_hours()
        {
            var response = await _client.GetAsync("/api/schedule/2026-06-01");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var slots = await response.Content.ReadFromJsonAsync<List<HourSlot>>();
            Assert.That(slots, Is.Not.Null);
            Assert.That(slots, Has.Count.EqualTo(8));
            Assert.That(slots!.All(s => s.IsAvailable), Is.True);
        }

        [Test]
        public async Task CreateBooking_returns_201()
        {
            var request = new { Date = "2026-06-15", Hour = 10, Description = "Testmøte" };

            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var body = await response.Content.ReadFromJsonAsync<BookingResponse>();
            Assert.That(body, Is.Not.Null);
            Assert.That(body!.Date, Is.EqualTo("2026-06-15"));
            Assert.That(body.Hour, Is.EqualTo(10));
            Assert.That(body.Description, Is.EqualTo("Testmøte"));
            Assert.That(body.Id, Is.Not.Empty);
        }

        [Test]
        public async Task CreateBooking_then_GetSchedule_shows_hour_as_booked()
        {
            var request = new { Date = "2026-07-01", Hour = 14, Description = "Ettermiddagsmøte" };
            var createResponse = await _client.PostAsJsonAsync("/api/bookings", request);
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var scheduleResponse = await _client.GetAsync("/api/schedule/2026-07-01");
            var slots = await scheduleResponse.Content.ReadFromJsonAsync<List<HourSlot>>();

            Assert.That(slots!.Single(s => s.Hour == 14).IsAvailable, Is.False);
            Assert.That(slots.Single(s => s.Hour == 14).Description, Is.EqualTo("Ettermiddagsmøte"));
        }

        [Test]
        public async Task CreateBooking_empty_description_returns_400()
        {
            var request = new { Date = "2026-06-15", Hour = 10, Description = "" };

            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.That(body!.Error, Is.EqualTo("MissingDescription"));
        }

        [Test]
        public async Task CreateBooking_whitespace_description_returns_400()
        {
            var request = new { Date = "2026-06-15", Hour = 10, Description = "   " };

            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task CreateBooking_past_date_returns_422()
        {
            var request = new { Date = "2020-01-01", Hour = 10, Description = "Fortid" };

            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        }

        [Test]
        public async Task CreateBooking_outside_hours_returns_422()
        {
            var request = new { Date = "2026-06-15", Hour = 7, Description = "For tidlig" };

            var response = await _client.PostAsJsonAsync("/api/bookings", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        }

        [Test]
        public async Task CreateBooking_duplicate_hour_returns_409()
        {
            var request = new { Date = "2026-08-01", Hour = 12, Description = "Første" };
            var first = await _client.PostAsJsonAsync("/api/bookings", request);
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var second = await _client.PostAsJsonAsync("/api/bookings", request);

            Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            var body = await second.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.That(body!.Error, Is.EqualTo("HourAlreadyBooked"));
        }

        [Test]
        public async Task GetAllBookings_returns_200_with_list()
        {
            var response = await _client.GetAsync("/api/bookings");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var bookings = await response.Content.ReadFromJsonAsync<List<BookingResponse>>();
            Assert.That(bookings, Is.Not.Null);
        }

        [Test]
        public async Task GetAllBookings_includes_newly_created_booking()
        {
            var request = new { Date = "2026-09-01", Hour = 11, Description = "Sync-test" };
            var createResponse = await _client.PostAsJsonAsync("/api/bookings", request);
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var getResponse = await _client.GetAsync("/api/bookings");
            var bookings = await getResponse.Content.ReadFromJsonAsync<List<BookingResponse>>();

            Assert.That(bookings!.Any(b => b.Hour == 11 && b.Date == "2026-09-01"), Is.True);
        }

        [Test]
        public async Task GetSchedule_returns_200_for_boundary_hours()
        {
            var date = "2026-06-01";
            var response = await _client.GetAsync($"/api/schedule/{date}");
            var slots = await response.Content.ReadFromJsonAsync<List<HourSlot>>();

            Assert.That(slots!.First().Hour, Is.EqualTo(8));
            Assert.That(slots.Last().Hour, Is.EqualTo(15));
        }

        [Test]
        public async Task CreateBooking_at_hour_8_succeeds()
        {
            var request = new { Date = "2026-10-01", Hour = 8, Description = "Åpningstid" };
            var response = await _client.PostAsJsonAsync("/api/bookings", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async Task CreateBooking_at_hour_15_succeeds()
        {
            var request = new { Date = "2026-10-01", Hour = 15, Description = "Stengning" };
            var response = await _client.PostAsJsonAsync("/api/bookings", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }
    }

    public record HourSlot(int Hour, bool IsAvailable, string? Description);
    public record BookingResponse(string Id, string Date, int Hour, string Description);
    public record ErrorResponse(string Error, string? Message);
}
