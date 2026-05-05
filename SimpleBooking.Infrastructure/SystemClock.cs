using SimpleBooking.Core.AppService;

namespace SimpleBooking.Infrastructure
{
    public class SystemClock : IClock
    {
        public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    }
}
