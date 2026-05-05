namespace SimpleBooking.Core.Model
{
    public class Booking
    {
        public Guid Id { get; private set; }
        public DateOnly Date { get; private set; }
        public int Hour { get; private set; }
        public string Description { get; private set; } = "";

        private Booking()
        {
        }

        public Booking(DateOnly date, int hour, string description)
            : this(Guid.NewGuid(), date, hour, description)
        {
        }

        public Booking(Guid id, DateOnly date, int hour, string description)
        {
            Id = id;
            Date = date;
            Hour = hour;
            Description = description;
        }

        public bool OverlapsWith(Booking other)
        {
            return Date == other.Date && Hour == other.Hour;
        }
    }
}
