using Microsoft.AspNetCore.Mvc;
using SimpleBooking.Core.AppService;

namespace SimpleBooking.Api.Controllers
{
    [ApiController]
    [Route("api/schedule")]
    public class ScheduleController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public ScheduleController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{date}")]
        public IActionResult GetDayStatus(DateOnly date)
        {
            var statuses = _bookingService.GetDayStatus(date);
            return Ok(statuses);
        }
    }
}
