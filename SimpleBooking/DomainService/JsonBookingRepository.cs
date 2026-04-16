using SimpleBooking.Model;
using System.Text.Json;

namespace SimpleBooking.DomainService
{
    static class JsonBookingRepository
    {
        private const string FilePath = "bookings.json";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static List<Booking> GetAll()
        {
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, "[]");
                return new List<Booking>();
            }

            var json = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                File.WriteAllText(FilePath, "[]");
                return new List<Booking>();
            }

            return JsonSerializer.Deserialize<List<Booking>>(json) ?? new List<Booking>();
        }

        public static void Add(Booking booking)
        {
            var bookings = GetAll();
            bookings.Add(booking);

            var json = JsonSerializer.Serialize(bookings, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
    }
}
