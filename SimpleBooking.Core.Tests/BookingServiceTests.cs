using NUnit.Framework;
using SimpleBooking.Core.AppService;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.Tests
{
    public class BookingServiceTests
    {
        private static readonly DateOnly Today = new(2026, 1, 1);

        private static BookingService CreateService(params Booking[] existingBookings)
        {
            var repo = new InMemoryBookingRepository(existingBookings);
            var outbox = new InMemoryOutboxRepository();
            var clock = new FakeClock { Today = Today };
            return new BookingService(repo, outbox, clock);
        }

        [Test]
        public void GetDayStatus_shows_opening_hours_and_existing_booking()
        {
            var date = Today.AddDays(1);
            var existingBooking = new Booking(Guid.NewGuid(), date, 10, "Planlegging");
            var service = CreateService(existingBooking);

            var statuses = service.GetDayStatus(date);

            Assert.That(statuses.Select(x => x.Hour), Is.EqualTo(Enumerable.Range(8, 8)));

            var bookedHour = statuses.Single(x => x.Hour == 10);
            Assert.That(bookedHour.IsAvailable, Is.False);
            Assert.That(bookedHour.Description, Is.EqualTo("Planlegging"));

            var availableHour = statuses.Single(x => x.Hour == 11);
            Assert.That(availableHour.IsAvailable, Is.True);
            Assert.That(availableHour.Description, Is.Null);
        }

        [Test]
        public void GetDayStatus_shows_all_available_when_no_bookings()
        {
            var date = Today.AddDays(1);
            var service = CreateService();

            var statuses = service.GetDayStatus(date);

            Assert.That(statuses, Has.Count.EqualTo(8));
            Assert.That(statuses.All(s => s.IsAvailable), Is.True);
        }

        [Test]
        public void GetDayStatus_shows_all_booked_when_all_hours_occupied()
        {
            var date = Today.AddDays(1);
            var allBookings = Enumerable.Range(8, 8)
                .Select(h => new Booking(Guid.NewGuid(), date, h, $"Møte {h}"))
                .ToArray();
            var service = CreateService(allBookings);

            var statuses = service.GetDayStatus(date);

            Assert.That(statuses.All(s => s.IsAvailable), Is.False);
        }

        [Test]
        public void GetDayStatus_shows_mixed_availability_on_same_date()
        {
            var date = Today.AddDays(1);
            var existingBooking = new Booking(Guid.NewGuid(), date, 9, "Frokostmøte");
            var service = CreateService(existingBooking);

            var statuses = service.GetDayStatus(date);

            Assert.That(statuses.Single(s => s.Hour == 9).IsAvailable, Is.False);
            Assert.That(statuses.Single(s => s.Hour == 8).IsAvailable, Is.True);
            Assert.That(statuses.Single(s => s.Hour == 10).IsAvailable, Is.True);
        }

        [Test]
        public void BookHour_for_tomorrow_within_opening_hours_succeeds()
        {
            var date = Today.AddDays(1);
            var service = CreateService();

            var result = service.BookHour(date, 9, "  Teammøte  ");

            Assert.That(result.Success, Is.True);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.None));
            Assert.That(result.Booking, Is.Not.Null);
            Assert.That(result.BookingConfirmationRequested, Is.Not.Null);

            Assert.That(result.Booking!.Date, Is.EqualTo(date));
            Assert.That(result.Booking.Hour, Is.EqualTo(9));
            Assert.That(result.Booking.Description, Is.EqualTo("Teammøte"));

            Assert.That(result.BookingConfirmationRequested!.Id, Is.EqualTo(result.Booking.Id));
            Assert.That(result.BookingConfirmationRequested.Date, Is.EqualTo(date));
            Assert.That(result.BookingConfirmationRequested.Hour, Is.EqualTo(9));
            Assert.That(result.BookingConfirmationRequested.Description, Is.EqualTo("Teammøte"));
        }
        [Test]
        public void BookHour_same_day_fails()
        {
            var service = CreateService();

            var result = service.BookHour(Today, 9, "Møte");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.NotBookable));
        }
        [Test]
        public void BookHour_persists_booking_to_repository()
        {
            var date = Today.AddDays(1);
            var (repo, outbox, clock) = CreateRawDoubles();
            clock.Today = Today;
            var service = new BookingService(repo, outbox, clock);

            var result = service.BookHour(date, 9, "Møte");

            Assert.That(result.Success, Is.True);
            var saved = repo.GetAll();
            Assert.That(saved, Has.Count.EqualTo(1));
            Assert.That(saved[0].Hour, Is.EqualTo(9));
        }

        [Test]
        public void BookHour_appends_outbox_message()
        {
            var date = Today.AddDays(1);
            var (repo, outbox, clock) = CreateRawDoubles();
            clock.Today = Today;
            var service = new BookingService(repo, outbox, clock);

            service.BookHour(date, 9, "Møte");

            Assert.That(outbox.Messages, Has.Count.EqualTo(1));
            Assert.That(outbox.Messages[0].Hour, Is.EqualTo(9));
        }

        [Test]
        public void BookHour_successful_booking_shows_as_booked_in_subsequent_query()
        {
            var date = Today.AddDays(1);
            var service = CreateService();

            var result = service.BookHour(date, 10, "Møte");
            Assert.That(result.Success, Is.True);

            var statuses = service.GetDayStatus(date);
            Assert.That(statuses.Single(s => s.Hour == 10).IsAvailable, Is.False);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void BookHour_for_today_or_past_is_rejected(int daysFromToday)
        {
            var service = CreateService();

            var result = service.BookHour(Today.AddDays(daysFromToday), 9, "Møte");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.NotBookable));
            Assert.That(result.Booking, Is.Null);
            Assert.That(result.BookingConfirmationRequested, Is.Null);
        }

        [TestCase(7)]
        [TestCase(16)]
        public void BookHour_outside_opening_hours_is_rejected(int hour)
        {
            var service = CreateService();

            var result = service.BookHour(Today.AddDays(1), hour, "Møte");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.NotBookable));
        }

        [Test]
        public void BookHour_at_opening_hour_8_succeeds()
        {
            var service = CreateService();
            var result = service.BookHour(Today.AddDays(1), 8, "Tidlig møte");
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void BookHour_at_closing_hour_15_succeeds()
        {
            var service = CreateService();
            var result = service.BookHour(Today.AddDays(1), 15, "Sene møte");
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void BookHour_for_far_future_date_succeeds()
        {
            var service = CreateService();
            var result = service.BookHour(Today.AddDays(365), 12, "Årsmøte");
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void BookHour_for_already_booked_hour_is_rejected()
        {
            var date = Today.AddDays(1);
            var existingBooking = new Booking(Guid.NewGuid(), date, 9, "Første møte");
            var service = CreateService(existingBooking);

            var result = service.BookHour(date, 9, "Andre møte");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.HourAlreadyBooked));
        }

        [Test]
        public void BookHour_for_same_hour_different_date_succeeds()
        {
            var date1 = Today.AddDays(1);
            var date2 = Today.AddDays(2);
            var existingBooking = new Booking(Guid.NewGuid(), date1, 9, "Møte dag 1");
            var service = CreateService(existingBooking);

            var result = service.BookHour(date2, 9, "Møte dag 2");

            Assert.That(result.Success, Is.True);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void BookHour_with_empty_description_is_rejected(string? description)
        {
            var service = CreateService();

            var result = service.BookHour(Today.AddDays(1), 9, description!);

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.MissingDescription));
        }

        [Test]
        public void BookHour_rejects_future_date_when_clock_is_ahead()
        {
            var (repo, outbox, clock) = CreateRawDoubles();
            clock.Today = Today;
            var service = new BookingService(repo, outbox, clock);

            var result = service.BookHour(Today, 10, "I dag");
            Assert.That(result.Success, Is.False);
        }

        private static (
            InMemoryBookingRepository,
            InMemoryOutboxRepository,
            FakeClock
        )
        CreateRawDoubles()
        {
            return (new InMemoryBookingRepository(), new InMemoryOutboxRepository(), new FakeClock());
        }
    }

    public class OverlapsWithTests
    {
        private static readonly DateOnly SomeDate = new(2026, 6, 15);

        [Test]
        public void OverlapsWith_returns_true_for_same_date_and_hour()
        {
            var a = new Booking(Guid.NewGuid(), SomeDate, 10, "A");
            var b = new Booking(Guid.NewGuid(), SomeDate, 10, "B");
            Assert.That(a.OverlapsWith(b), Is.True);
        }

        [Test]
        public void OverlapsWith_returns_false_for_same_date_different_hour()
        {
            var a = new Booking(Guid.NewGuid(), SomeDate, 10, "A");
            var b = new Booking(Guid.NewGuid(), SomeDate, 11, "B");
            Assert.That(a.OverlapsWith(b), Is.False);
        }

        [Test]
        public void OverlapsWith_returns_false_for_different_date_same_hour()
        {
            var a = new Booking(Guid.NewGuid(), SomeDate, 10, "A");
            var b = new Booking(Guid.NewGuid(), SomeDate.AddDays(1), 10, "B");
            Assert.That(a.OverlapsWith(b), Is.False);
        }

        [Test]
        public void OverlapsWith_is_symmetric()
        {
            var a = new Booking(Guid.NewGuid(), SomeDate, 10, "A");
            var b = new Booking(Guid.NewGuid(), SomeDate, 10, "B");
            Assert.That(a.OverlapsWith(b), Is.EqualTo(b.OverlapsWith(a)));
        }
    }

    internal class InMemoryBookingRepository : IBookingRepository
    {
        private readonly List<Booking> _bookings = new();

        public InMemoryBookingRepository(params Booking[] bookings)
        {
            _bookings.AddRange(bookings);
        }

        public List<Booking> GetAll() => _bookings.ToList();

        public void Add(Booking booking) => _bookings.Add(booking);
    }

    internal class InMemoryOutboxRepository : IOutboxRepository
    {
        public List<BookingConfirmationRequested> Messages { get; } = new();

        public void Append(BookingConfirmationRequested confirmation) => Messages.Add(confirmation);
    }

    internal class FakeClock : IClock
    {
        public DateOnly Today { get; set; }
    }
}
