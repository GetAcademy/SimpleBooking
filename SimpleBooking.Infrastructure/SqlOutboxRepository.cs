using System.Text.Json;
using SimpleBooking.Core.AppService;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Infrastructure
{
    public class SqlOutboxRepository : IOutboxRepository
    {
        private readonly BookingDbContext _db;

        public SqlOutboxRepository(BookingDbContext db)
        {
            _db = db;
        }

        public void Append(BookingConfirmationRequested confirmation)
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "BookingConfirmationRequested",
                CreatedAt = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(confirmation)
            };

            _db.OutboxMessages.Add(message);
            _db.SaveChanges();
        }
    }
}
