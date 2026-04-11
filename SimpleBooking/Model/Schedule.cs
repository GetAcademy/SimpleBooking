namespace SimpleBooking.Model
{
    class Schedule
    {
        public const int OpeningHour = 8;
        public const int ClosingHour = 16; // 8-15 er bookbare timer

        private readonly List<Booking> _bookings;

        public Schedule(IEnumerable<Booking> bookings)
        {
            _bookings = bookings
                .OrderBy(b => b.Date)
                .ThenBy(b => b.Hour)
                .ToList();
        }

        public List<int> GetAvailableHours(DateOnly date)
        {
            var availableHours = new List<int>();

            for (var hour = OpeningHour; hour < ClosingHour; hour++)
            {
                var candidate = new Booking(date, hour, "candidate");

                if (!HasConflict(candidate))
                {
                    availableHours.Add(hour);
                }
            }

            return availableHours;
        }

        public bool TryAddBooking(Booking booking)
        {
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
    }
}
