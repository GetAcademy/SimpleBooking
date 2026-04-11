namespace SimpleBooking.Model
{
    class Schedule
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

                if (existingBooking is null)
                {
                    result.Add(new HourStatus(hour, true, null));
                }
                else
                {
                    result.Add(new HourStatus(hour, false, existingBooking.Description));
                }
            }

            return result;
        }

        public bool TryAddBooking(Booking booking)
        {
            if (!IsBookableDate(booking))
            {
                return false;
            }

            if (!IsWithinOpeningHours(booking))
            {
                return false;
            }

            if (HasConflict(booking))
            {
                return false;
            }

            _bookings.Add(booking);
            _bookings.Sort((a, b) =>
            {
                var dateComparison = a.Date.CompareTo(b.Date);
                return dateComparison != 0 ? dateComparison : a.Hour.CompareTo(b.Hour);
            });

            return true;
        }

        private bool HasConflict(Booking booking)
        {
            return _bookings.Any(existing => existing.OverlapsWith(booking));
        }

        private bool IsWithinOpeningHours(Booking booking)
        {
            return booking.Hour >= OpeningHour && booking.Hour < ClosingHour;
        }

        private bool IsBookableDate(Booking booking)
        {
            return booking.Date > _today;
        }
    }
}
