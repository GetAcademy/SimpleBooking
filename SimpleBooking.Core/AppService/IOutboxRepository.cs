using SimpleBooking.Core.Model;

namespace SimpleBooking.Core.AppService
{
    public interface IOutboxRepository
    {
        void Append(BookingConfirmationRequested confirmation);
    }
}
