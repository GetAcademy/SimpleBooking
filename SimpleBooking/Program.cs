using SimpleBooking;
using SimpleBooking.Core.AppService;
using SimpleBooking.Infrastructure;

var bookingRepository = new JsonBookingRepository();
var outboxRepository = new JsonOutboxRepository();
var bookings = bookingRepository.GetAll();
var today = DateOnly.FromDateTime(DateTime.Today);
var bookingService = new BookingService(bookings, today);

var app = new BookingApp(bookingService, bookingRepository, outboxRepository, today);
app.Run();
