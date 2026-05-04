using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.AppService
{
    public class BookHourResult
    {
        public bool Success { get; }
        public Booking? Booking { get; }
        public BookingConfirmationRequested? BookingConfirmationRequested { get; }
        public BookingFailureReason FailureReason { get; }

        private BookHourResult(
            bool success,
            Booking? booking,
            BookingConfirmationRequested? bookingConfirmationRequested,
            BookingFailureReason failureReason)
        {
            Success = success;
            Booking = booking;
            BookingConfirmationRequested = bookingConfirmationRequested;
            FailureReason = failureReason;
        }

        public static BookHourResult Ok(
            Booking booking,
            BookingConfirmationRequested bookingConfirmationRequested)
        {
            return new BookHourResult(
                true,
                booking,
                bookingConfirmationRequested,
                BookingFailureReason.None);
        }

        public static BookHourResult Failed(BookingFailureReason failureReason)
        {
            return new BookHourResult(false, null, null, failureReason);
        }
    }
}
