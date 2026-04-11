using System.Text.Json;
using SimpleBooking.Model;

namespace SimpleBooking.DomainService
{
    static class JsonOutboxRepository
    {
        private const string FilePath = "outbox.json";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static List<OutboxMessage> GetAll()
        {
            if (!File.Exists(FilePath))
            {
                return new List<OutboxMessage>();
            }

            var json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<OutboxMessage>();
            }

            return JsonSerializer.Deserialize<List<OutboxMessage>>(json) ?? new List<OutboxMessage>();
        }

        public static void Append(OutboxMessage message)
        {
            var messages = GetAll();
            messages.Add(message);

            var json = JsonSerializer.Serialize(messages, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
    }
}
