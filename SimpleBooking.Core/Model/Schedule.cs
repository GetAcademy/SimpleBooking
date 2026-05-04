namespace SimpleBooking.Core.Model
{
    public class Schedule
    {
        public const int OpeningHour = 8;
        public const int ClosingHour = 16; // 8-15 er bookbare timer

        private readonly List<Booking> _bookings;
        private readonly DateOnly _today;

        public Schedule(IEnumerable<Booking> bookings, DateOnly today)
        {
            _bookings = bookings
                .OrderBy(b => b.Date)
                .ThenBy(b => b.Hour)
                .ToList();

            _today = today;
        }

        public List<HourStatus> GetDayStatus(DateOnly date)
        {
            var result = new List<HourStatus>();

            for (var hour = OpeningHour; hour < ClosingHour; hour++)
            {
                var existingBooking = _bookings
                    .FirstOrDefault(b => b.Date == date && b.Hour == hour);

                result.Add(existingBooking is null
                    ? new HourStatus(hour, true, null)
                    : new HourStatus(hour, false, existingBooking.Description));
            }

            return result;
        }

        public BookingFailureReason GetFailureReason(Booking booking)
        {
            if (!IsBookableDate(booking)) return BookingFailureReason.NotBookable;
            if (!IsWithinOpeningHours(booking)) return BookingFailureReason.NotBookable;
            if (HasConflict(booking)) return BookingFailureReason.HourAlreadyBooked;

            return BookingFailureReason.None;
        }

        public void AddBooking(Booking booking)
        {
            _bookings.Add(booking);
            SortBookings();
        }

        private void SortBookings()
        {
            _bookings.Sort((a, b) =>
            {
                var dateComparison = a.Date.CompareTo(b.Date);
                return dateComparison != 0 ? dateComparison : a.Hour.CompareTo(b.Hour);
            });
        }

        private bool HasConflict(Booking booking) =>
            _bookings.Any(existing => existing.OverlapsWith(booking));

        private bool IsWithinOpeningHours(Booking booking) =>
            booking.Hour >= OpeningHour && booking.Hour < ClosingHour;

        private bool IsBookableDate(Booking booking) =>
            booking.Date > _today;
    }
}
