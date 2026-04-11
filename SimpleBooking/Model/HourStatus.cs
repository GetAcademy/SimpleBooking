namespace SimpleBooking.Model
{
    class HourStatus
    {
        public int Hour { get; }
        public bool IsAvailable { get; }
        public string? Description { get; }

        public HourStatus(int hour, bool isAvailable, string? description)
        {
            Hour = hour;
            IsAvailable = isAvailable;
            Description = description;
        }
    }
}
