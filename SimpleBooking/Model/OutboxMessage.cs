using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBooking.Model
{
    internal class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Payload { get; set; } = "";
    }
}
