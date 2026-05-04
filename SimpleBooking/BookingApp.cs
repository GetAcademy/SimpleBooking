using SimpleBooking.Core.AppService;
using SimpleBooking.Core.Model;
using SimpleBooking.Infrastructure;

namespace SimpleBooking
{
    class BookingApp
    {
        private readonly BookingService _bookingService;
        private readonly JsonBookingRepository _bookingRepository;
        private readonly JsonOutboxRepository _outboxRepository;
        private readonly DateOnly _today;
        private DateOnly _currentDate;

        public BookingApp(
            BookingService bookingService,
            JsonBookingRepository bookingRepository,
            JsonOutboxRepository outboxRepository,
            DateOnly today)
        {
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
            _outboxRepository = outboxRepository;
            _today = today;
            _currentDate = _today;
        }

        public void Run()
        {
            var isRunning = true;

            while (isRunning)
            {
                Console.Clear();
                ShowCurrentDay();

                Console.WriteLine();
                Console.WriteLine("[+] Neste dag");
                Console.WriteLine("[-] Forrige dag");
                Console.WriteLine("[B] Book time");
                Console.WriteLine("[Q] Avslutt");
                Console.Write("Valg: ");

                var key = Console.ReadKey(intercept: true).Key;

                if (key == ConsoleKey.Add) _currentDate = _currentDate.AddDays(1);
                else if (key == ConsoleKey.Subtract && _currentDate > _today) _currentDate = _currentDate.AddDays(-1);
                else if (key == ConsoleKey.B) BookHour();
                else if (key == ConsoleKey.Q) isRunning = false;
            }
        }

        private void ShowCurrentDay()
        {
            Console.WriteLine($"Dato: {_currentDate:dd.MM.yyyy}");
            Console.WriteLine();

            var hourStatuses = _bookingService.GetDayStatus(_currentDate);

            foreach (var status in hourStatuses)
            {
                var statusText = status.IsAvailable
                    ? "Ledig"
                    : $"Opptatt  ({status.Description})";

                Console.WriteLine($"{status.Hour:00}:00  {statusText}");
            }
        }

        private void BookHour()
        {
            var hourStatuses = _bookingService.GetDayStatus(_currentDate);

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
            var description = Console.ReadLine() ?? "";

            var result = _bookingService.BookHour(_currentDate, hour, description);

            if (!result.Success)
            {
                ShowMessage(GetFailureMessage(result.FailureReason));
                return;
            }

            _bookingRepository.Add(result.Booking!);
            _outboxRepository.Append(result.BookingConfirmationRequested!);
        }

        private static string GetFailureMessage(BookingFailureReason failureReason)
        {
            return failureReason switch
            {
                BookingFailureReason.MissingDescription => "Beskrivelse må fylles ut.",
                BookingFailureReason.HourAlreadyBooked => "Booking feilet.",
                BookingFailureReason.NotBookable => "Booking feilet.",
                _ => "Booking feilet."
            };
        }

        private static void ShowMessage(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Trykk en tast for å fortsette...");
            Console.ReadKey(intercept: true);
        }
    }
}
