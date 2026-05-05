using SimpleBooking.Core.AppService;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Infrastructure
{
    public class SqlBookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _db;

        public SqlBookingRepository(BookingDbContext db)
        {
            _db = db;
        }

        public List<Booking> GetAll()
        {
            return _db.Bookings.ToList();
        }

        public void Add(Booking booking)
        {
            _db.Bookings.Add(booking);
            _db.SaveChanges();
        }
    }
}
