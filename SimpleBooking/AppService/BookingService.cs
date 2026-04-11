using SimpleBooking.DomainService;
using SimpleBooking.Model;

namespace SimpleBooking.AppService
{
    class BookingService
    {
        private readonly Schedule _schedule;

        public BookingService()
        {
            var bookings = JsonBookingRepository.GetAll();
            var today = DateOnly.FromDateTime(DateTime.Today);
            _schedule = new Schedule(bookings, today);
        }

        public List<HourStatus> GetDayStatus(DateOnly date)
        {
            return _schedule.GetDayStatus(date);
        }

        public void BookHour(DateOnly date)
        {
            Console.Clear();

            Console.WriteLine($"Dato: {date:dd.MM.yyyy}");
            Console.WriteLine();

            var hourStatuses = _schedule.GetDayStatus(date);

            foreach (var status in hourStatuses)
            {
                if (status.IsAvailable)
                {
                    Console.WriteLine($"{status.Hour:00}:00  Ledig");
                }
                else
                {
                    Console.WriteLine($"{status.Hour:00}:00  Opptatt  ({status.Description})");
                }
            }

            if (hourStatuses.All(x => !x.IsAvailable))
            {
                ShowMessage("Det er ingen ledige timer å booke.");
                return;
            }

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

            var booking = new Booking(date, hour, description.Trim());
            var added = _schedule.TryAddBooking(booking);

            if (!added)
            {
                ShowMessage("Booking feilet.");
                return;
            }

            JsonBookingRepository.Add(booking);
            JsonOutboxRepository.Append(OutboxMessage.CreateBookingConfirmation(booking));
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
