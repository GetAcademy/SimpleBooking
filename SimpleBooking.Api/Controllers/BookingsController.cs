using Microsoft.AspNetCore.Mvc;
using SimpleBooking.Core.AppService;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;

        public BookingsController(BookingService bookingService, IBookingRepository bookingRepository)
        {
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var bookings = _bookingRepository.GetAll();
            return Ok(bookings);
        }

        [HttpPost]
        public IActionResult CreateBooking([FromBody] CreateBookingRequest request)
        {
            var result = _bookingService.BookHour(request.Date, request.Hour, request.Description);

            if (!result.Success)
            {
                return result.FailureReason switch
                {
                    BookingFailureReason.MissingDescription => BadRequest(new { error = "MissingDescription", message = "Beskrivelse må fylles ut." }),
                    BookingFailureReason.NotBookable => UnprocessableEntity(new { error = "NotBookable", message = "Kan ikke booke denne timen." }),
                    BookingFailureReason.HourAlreadyBooked => Conflict(new { error = "HourAlreadyBooked", message = "Timen er allerede booket." }),
                    _ => BadRequest(new { error = "Unknown", message = "Uventet feil." })
                };
            }

            return CreatedAtAction(
                nameof(ScheduleController.GetDayStatus),
                "Schedule",
                new { date = request.Date },
                new
                {
                    id = result.Booking!.Id,
                    date = result.Booking.Date,
                    hour = result.Booking.Hour,
                    description = result.Booking.Description
                });
        }
    }

    public class CreateBookingRequest
    {
        public DateOnly Date { get; set; }
        public int Hour { get; set; }
        public string Description { get; set; } = "";
    }
}
