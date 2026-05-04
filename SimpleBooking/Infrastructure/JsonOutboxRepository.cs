using System.Text.Json;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Infrastructure
{
    class JsonOutboxRepository
    {
        private const string FilePath = "outbox.json";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public void Append(BookingConfirmationRequested confirmation)
        {
            var messages = GetAll();
            messages.Add(CreateOutboxMessage(confirmation));

            var json = JsonSerializer.Serialize(messages, JsonOptions);
            File.WriteAllText(FilePath, json);
        }

        private static List<OutboxMessageDto> GetAll()
        {
            if (!File.Exists(FilePath))
            {
                return new List<OutboxMessageDto>();
            }

            var json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<OutboxMessageDto>();
            }

            return JsonSerializer.Deserialize<List<OutboxMessageDto>>(json) ?? new List<OutboxMessageDto>();
        }

        private static OutboxMessageDto CreateOutboxMessage(BookingConfirmationRequested confirmation)
        {
            return new OutboxMessageDto
            {
                Id = Guid.NewGuid(),
                Type = "BookingConfirmationRequested",
                CreatedAt = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(confirmation)
            };
        }

        private class OutboxMessageDto
        {
            public Guid Id { get; set; }
            public string Type { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string Payload { get; set; } = "";
        }
    }
}
