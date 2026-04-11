using System.Text.Json;

namespace SimpleBooking.Model
{
    class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Payload { get; set; } = "";

        public static OutboxMessage CreateBookingConfirmation(Booking booking)
        {
            var payload = JsonSerializer.Serialize(new
            {
                booking.Id,
                booking.Date,
                booking.Hour,
                booking.Description
            });

            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "BookingConfirmationRequested",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };
        }
    }
}
