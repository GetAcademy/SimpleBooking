namespace SimpleBooking.Core.AppService
{
    public interface IClock
    {
        DateOnly Today { get; }
    }
}
