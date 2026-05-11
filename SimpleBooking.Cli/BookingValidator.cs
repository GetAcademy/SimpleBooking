namespace SimpleBooking.Cli;

public static class BookingValidator
{
    public const int OpeningHour = 8;
    public const int ClosingHour = 16;

    public static string? ValidateDate(DateOnly date, DateOnly today)
    {
        if (date <= today)
        {
            return "Dato må være fra og med i morgen.";
        }
        return null;
    }

    public static string? ValidateHour(int hour)
    {
        if (hour < OpeningHour || hour >= ClosingHour)
        {
            return $"Time må være mellom {OpeningHour} og {ClosingHour - 1}.";
        }
        return null;
    }

    public static string? ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Beskrivelse må fylles ut.";
        }
        return null;
    }
}
