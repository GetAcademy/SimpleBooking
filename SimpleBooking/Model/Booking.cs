namespace SimpleBooking.Model
{
    class Booking
    {
        public Guid Id { get; private set; }
        public DateOnly Date { get; private set; }
        public int Hour { get; private set; }
        public string Description { get; private set; }

        public Booking(DateOnly date, int hour, string description)
        {
            Id = Guid.NewGuid();
            Date = date;
            Hour = hour;
            Description = description;
        }

        // Trengs for JSON-deserialisering
        public Booking()
        {
            Description = "";
        }

        public bool OverlapsWith(Booking other)
        {
            return Date == other.Date && Hour == other.Hour;
        }
    }
}
