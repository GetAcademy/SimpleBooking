namespace SimpleBooking.Core.Model
{
    public class BookingConfirmationRequested
    {
        public Guid Id { get; }
        public DateOnly Date { get; }
        public int Hour { get; }
        public string Description { get; }

        public BookingConfirmationRequested(Booking booking)
        {
            Id = booking.Id;
            Date = booking.Date;
            Hour = booking.Hour;
            Description = booking.Description;
        }
    }
}
