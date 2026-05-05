using SimpleBooking;
using SimpleBooking.Core.AppService;
using SimpleBooking.Infrastructure;

var bookingRepository = new JsonBookingRepository();
var outboxRepository = new JsonOutboxRepository();
var clock = new SystemClock();
var bookingService = new BookingService(bookingRepository, outboxRepository, clock);

var app = new BookingApp(bookingService, clock);
app.Run();
