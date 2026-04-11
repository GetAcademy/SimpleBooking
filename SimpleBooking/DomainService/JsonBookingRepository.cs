using SimpleBooking.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SimpleBooking.DomainService
{
    internal class JsonBookingRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public JsonBookingRepository(string filePath)
        {
            _filePath = filePath;
        }

        public List<Booking> GetAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Booking>();
            }

            var json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Booking>();
            }

            return JsonSerializer.Deserialize<List<Booking>>(json) ?? new List<Booking>();
        }

        public void Add(Booking booking)
        {
            var bookings = GetAll();
            bookings.Add(booking);
            SaveAll(bookings);
        }

        private void SaveAll(List<Booking> bookings)
        {
            var json = JsonSerializer.Serialize(bookings, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
