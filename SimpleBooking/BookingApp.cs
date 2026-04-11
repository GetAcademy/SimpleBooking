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

                switch (key)
                {
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        _currentDate = _currentDate.AddDays(1);
                        break;

                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        if (_currentDate > _today)
                        {
                            _currentDate = _currentDate.AddDays(-1);
                        }
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

            var hourStatuses = _bookingService.GetDayStatus(_currentDate);

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
        }
    }
}
