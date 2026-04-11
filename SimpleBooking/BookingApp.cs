using SimpleBooking.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBooking
{
    internal class BookingApp
    {
        private readonly BookingService _bookingService;
        private DateOnly _currentDate;

        public BookingApp(BookingService bookingService)
        {
            _bookingService = bookingService;
            _currentDate = DateOnly.FromDateTime(DateTime.Today);
        }

        public void Run()
        {
            bool isRunning = true;

            while (isRunning)
            {
                Console.Clear();
                _bookingService.ShowDay(_currentDate);

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
    }
}
