using SimpleBooking.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using SimpleBooking.DomainService;

namespace SimpleBooking.Service
{
    internal class BookingService
    {
        private readonly JsonBookingRepository _bookingRepository;
        private readonly JsonOutboxRepository _outboxRepository;

        private const int OpeningHour = 8;
        private const int ClosingHour = 16; // 8-15 er bookbare timer

        public BookingService(
            JsonBookingRepository bookingRepository,
            JsonOutboxRepository outboxRepository)
        {
            _bookingRepository = bookingRepository;
            _outboxRepository = outboxRepository;
        }

        public void ShowDay(DateOnly date)
        {
            Console.WriteLine($"Dato: {date:dd.MM.yyyy}");
            Console.WriteLine();

            var availableHours = GetAvailableHours(date);

            Console.WriteLine("Ledige timer:");
            if (availableHours.Count == 0)
            {
                Console.WriteLine("Ingen ledige timer.");
            }
            else
            {
                foreach (var hour in availableHours)
                {
                    Console.WriteLine($"- {hour:00}:00");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Opptatte timer:");

            var bookings = _bookingRepository.GetAll()
                .Where(b => b.Date == date)
                .OrderBy(b => b.Hour)
                .ToList();

            if (bookings.Count == 0)
            {
                Console.WriteLine("Ingen bookinger.");
            }
            else
            {
                foreach (var booking in bookings)
                {
                    Console.WriteLine($"- {booking.Hour:00}:00  {booking.Description}");
                }
            }
        }

        public void BookHour(DateOnly date)
        {
            Console.Clear();
            ShowDay(date);
            Console.WriteLine();

            Console.Write("Hvilken time vil du booke? ");
            var hourText = Console.ReadLine();

            if (!int.TryParse(hourText, out var hour))
            {
                ShowMessage("Ugyldig time.");
                return;
            }

            Console.Write("Beskrivelse: ");
            var description = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(description))
            {
                ShowMessage("Beskrivelse må fylles ut.");
                return;
            }

            if (!IsWithinOpeningHours(hour))
            {
                ShowMessage("Timen er utenfor åpningstid.");
                return;
            }

            if (!IsAvailable(date, hour))
            {
                ShowMessage("Timen er opptatt.");
                return;
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Date = date,
                Hour = hour,
                Description = description.Trim()
            };

            _bookingRepository.Add(booking);

            var payload = JsonSerializer.Serialize(new
            {
                booking.Id,
                booking.Date,
                booking.Hour,
                booking.Description
            });

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "BookingConfirmationRequested",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            _outboxRepository.Append(outboxMessage);

            ShowMessage("Booking opprettet.");
        }

        private List<int> GetAvailableHours(DateOnly date)
        {
            var bookedHours = _bookingRepository.GetAll()
                .Where(b => b.Date == date)
                .Select(b => b.Hour)
                .ToHashSet();

            var availableHours = new List<int>();

            for (int hour = OpeningHour; hour < ClosingHour; hour++)
            {
                if (!bookedHours.Contains(hour))
                {
                    availableHours.Add(hour);
                }
            }

            return availableHours;
        }

        private bool IsAvailable(DateOnly date, int hour)
        {
            return !_bookingRepository.GetAll()
                .Any(b => b.Date == date && b.Hour == hour);
        }

        private bool IsWithinOpeningHours(int hour)
        {
            return hour >= OpeningHour && hour < ClosingHour;
        }

        private void ShowMessage(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Trykk en tast for å fortsette...");
            Console.ReadKey(intercept: true);
        }
    }
}
