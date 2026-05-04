using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.AppService
{
    public class BookingService
    {
        private readonly Schedule _schedule;

        public BookingService(IEnumerable<Booking> bookings, DateOnly today)
        {
            _schedule = new Schedule(bookings, today);
        }

        public List<HourStatus> GetDayStatus(DateOnly date)
        {
            return _schedule.GetDayStatus(date);
        }

        public BookHourResult BookHour(DateOnly date, int hour, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BookHourResult.Failed(BookingFailureReason.MissingDescription);
            }

            var booking = new Booking(date, hour, description.Trim());
            var failureReason = _schedule.GetFailureReason(booking);

            if (failureReason != BookingFailureReason.None)
            {
                return BookHourResult.Failed(failureReason);
            }

            _schedule.AddBooking(booking);

            var confirmation = new BookingConfirmationRequested(booking);
            return BookHourResult.Ok(booking, confirmation);
        }
    }
}
