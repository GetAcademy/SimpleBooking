using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using SimpleBooking.Model;

namespace SimpleBooking.DomainService
{
    internal class JsonOutboxRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public JsonOutboxRepository(string filePath)
        {
            _filePath = filePath;
        }

        public List<OutboxMessage> GetAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<OutboxMessage>();
            }

            var json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<OutboxMessage>();
            }

            return JsonSerializer.Deserialize<List<OutboxMessage>>(json) ?? new List<OutboxMessage>();
        }

        public void Append(OutboxMessage message)
        {
            var messages = GetAll();
            messages.Add(message);
            SaveAll(messages);
        }

        private void SaveAll(List<OutboxMessage> messages)
        {
            var json = JsonSerializer.Serialize(messages, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
