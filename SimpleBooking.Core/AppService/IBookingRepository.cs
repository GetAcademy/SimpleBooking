using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.AppService
{
    public interface IBookingRepository
    {
        List<Booking> GetAll();
        void Add(Booking booking);
    }
}
