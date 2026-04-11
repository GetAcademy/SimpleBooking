using SimpleBooking.AppService;

namespace SimpleBooking
{
    class BookingApp
    {
        private readonly BookingService _bookingService;
        private readonly DateOnly _today;
        private DateOnly _currentDate;

        public BookingApp()
        {
            _bookingService = new BookingService();
            _today = DateOnly.FromDateTime(DateTime.Today);
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
                else if (key == ConsoleKey.B) _bookingService.BookHour(_currentDate);
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
    }
}
