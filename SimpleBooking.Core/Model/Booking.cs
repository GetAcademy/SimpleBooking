namespace SimpleBooking.Core.Model
{
    public class Booking
    {
        public Guid Id { get; }
        public DateOnly Date { get; }
        public int Hour { get; }
        public string Description { get; }

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
