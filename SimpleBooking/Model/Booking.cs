using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBooking.Model
{
    internal class Booking
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public int Hour { get; set; }
        public string Description { get; set; } = "";
    }
}
