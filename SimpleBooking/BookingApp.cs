using SimpleBooking.AppService;

namespace SimpleBooking
{
    class BookingApp
    {
        private readonly BookingService _bookingService;
        private DateOnly _currentDate;

        public BookingApp()
        {
            _bookingService = new BookingService();
            _currentDate = DateOnly.FromDateTime(DateTime.Today);
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

                switch (key)
                {
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        _currentDate = _currentDate.AddDays(1);
                        break;

                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        _currentDate = _currentDate.AddDays(-1);
                        break;

                    case ConsoleKey.B:
                        _bookingService.BookHour(_currentDate);
                        break;

                    case ConsoleKey.Q:
                        isRunning = false;
                        break;
                }
            }
        }

        private void ShowCurrentDay()
        {
            Console.WriteLine($"Dato: {_currentDate:dd.MM.yyyy}");
            Console.WriteLine();

            var availableHours = _bookingService.GetAvailableHours(_currentDate);

            Console.WriteLine("Ledige timer:");
            if (availableHours.Count == 0)
            {
                Console.WriteLine("Ingen ledige timer.");
                return;
            }

            foreach (var hour in availableHours)
            {
                Console.WriteLine($"- {hour:00}:00");
            }
        }
    }
}
