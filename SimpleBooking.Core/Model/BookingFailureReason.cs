namespace SimpleBooking.Core.Model
{
    public enum BookingFailureReason
    {
        None,
        MissingDescription,
        NotBookable,
        HourAlreadyBooked
    }
}
