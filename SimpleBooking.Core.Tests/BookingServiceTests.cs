using NUnit.Framework;
using SimpleBooking.Core.AppService;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.Tests
{
    public class BookingServiceTests
    {
        private static readonly DateOnly Today = new(2026, 1, 1);

        [Test]
        public void GetDayStatus_shows_opening_hours_and_existing_booking()
        {
            var date = Today.AddDays(1);
            var existingBooking = new Booking(Guid.NewGuid(), date, 10, "Planlegging");
            var service = new BookingService(new[] { existingBooking }, Today);

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
        public void BookHour_for_tomorrow_within_opening_hours_succeeds()
        {
            var date = Today.AddDays(1);
            var service = new BookingService(Array.Empty<Booking>(), Today);

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

        [TestCase(-1)]
        [TestCase(0)]
        public void BookHour_for_today_or_past_is_rejected(int daysFromToday)
        {
            var service = new BookingService(Array.Empty<Booking>(), Today);

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
            var service = new BookingService(Array.Empty<Booking>(), Today);

            var result = service.BookHour(Today.AddDays(1), hour, "Møte");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.NotBookable));
        }

        [Test]
        public void BookHour_for_already_booked_hour_is_rejected()
        {
            var date = Today.AddDays(1);
            var existingBooking = new Booking(Guid.NewGuid(), date, 9, "Første møte");
            var service = new BookingService(new[] { existingBooking }, Today);

            var result = service.BookHour(date, 9, "Andre møte");

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.HourAlreadyBooked));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void BookHour_with_empty_description_is_rejected(string description)
        {
            var service = new BookingService(Array.Empty<Booking>(), Today);

            var result = service.BookHour(Today.AddDays(1), 9, description);

            Assert.That(result.Success, Is.False);
            Assert.That(result.FailureReason, Is.EqualTo(BookingFailureReason.MissingDescription));
        }
    }
}
