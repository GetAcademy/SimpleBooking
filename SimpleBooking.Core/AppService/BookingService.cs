using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.AppService
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IClock _clock;

        public BookingService(
            IBookingRepository bookingRepository,
            IOutboxRepository outboxRepository,
            IClock clock)
        {
            _bookingRepository = bookingRepository;
            _outboxRepository = outboxRepository;
            _clock = clock;
        }

        public List<HourStatus> GetDayStatus(DateOnly date)
        {
            var schedule = BuildSchedule();
            return schedule.GetDayStatus(date);
        }

        public BookHourResult BookHour(DateOnly date, int hour, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return BookHourResult.Failed(BookingFailureReason.MissingDescription);
            }

            var booking = new Booking(date, hour, description.Trim());
            var schedule = BuildSchedule();
            var failureReason = schedule.GetFailureReason(booking);

            if (failureReason != BookingFailureReason.None)
            {
                return BookHourResult.Failed(failureReason);
            }

            _bookingRepository.Add(booking);

            var confirmation = new BookingConfirmationRequested(booking);
            _outboxRepository.Append(confirmation);

            return BookHourResult.Ok(booking, confirmation);
        }

        private Schedule BuildSchedule()
        {
            return new Schedule(_bookingRepository.GetAll(), _clock.Today);
        }
    }
}
