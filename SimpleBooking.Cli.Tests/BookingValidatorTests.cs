using NUnit.Framework;
using SimpleBooking.Cli;

namespace SimpleBooking.Cli.Tests;

public class BookingValidatorTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);

    [Test]
    public void ValidateDate_tomorrow_returns_null()
    {
        var tomorrow = Today.AddDays(1);
        var error = BookingValidator.ValidateDate(tomorrow, Today);
        Assert.That(error, Is.Null);
    }

    [Test]
    public void ValidateDate_today_returns_error()
    {
        var error = BookingValidator.ValidateDate(Today, Today);
        Assert.That(error, Is.Not.Null);
        Assert.That(error, Does.Contain("i morgen"));
    }

    [Test]
    public void ValidateDate_past_returns_error()
    {
        var yesterday = Today.AddDays(-1);
        var error = BookingValidator.ValidateDate(yesterday, Today);
        Assert.That(error, Is.Not.Null);
    }

    [TestCase(8)]
    [TestCase(10)]
    [TestCase(15)]
    public void ValidateHour_within_range_returns_null(int hour)
    {
        var error = BookingValidator.ValidateHour(hour);
        Assert.That(error, Is.Null);
    }

    [TestCase(7)]
    [TestCase(16)]
    [TestCase(23)]
    public void ValidateHour_outside_range_returns_error(int hour)
    {
        var error = BookingValidator.ValidateHour(hour);
        Assert.That(error, Is.Not.Null);
        Assert.That(error, Does.Contain("8"));
        Assert.That(error, Does.Contain("15"));
    }

    [Test]
    public void ValidateDescription_with_text_returns_null()
    {
        var error = BookingValidator.ValidateDescription("Teammøte");
        Assert.That(error, Is.Null);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void ValidateDescription_empty_returns_error(string? description)
    {
        var error = BookingValidator.ValidateDescription(description!);
        Assert.That(error, Is.Not.Null);
    }
}
