using System.Text.Json;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Infrastructure
{
    class JsonBookingRepository
    {
        private const string FilePath = "bookings.json";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public List<Booking> GetAll()
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

            var bookingDtos = JsonSerializer.Deserialize<List<BookingDto>>(json) ?? new List<BookingDto>();
            return bookingDtos.Select(ToBooking).ToList();
        }

        public void Add(Booking booking)
        {
            var bookings = GetAll();
            bookings.Add(booking);

            var bookingDtos = bookings.Select(ToDto).ToList();
            var json = JsonSerializer.Serialize(bookingDtos, JsonOptions);
            File.WriteAllText(FilePath, json);
        }

        private static Booking ToBooking(BookingDto dto)
        {
            return new Booking(dto.Id, dto.Date, dto.Hour, dto.Description);
        }

        private static BookingDto ToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                Date = booking.Date,
                Hour = booking.Hour,
                Description = booking.Description
            };
        }

        private class BookingDto
        {
            public Guid Id { get; set; }
            public DateOnly Date { get; set; }
            public int Hour { get; set; }
            public string Description { get; set; } = "";
        }
    }
}
