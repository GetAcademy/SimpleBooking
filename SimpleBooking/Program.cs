using SimpleBooking;
using SimpleBooking.DomainService;
using SimpleBooking.Service;

var bookingRepository = new JsonBookingRepository("bookings.json");
var outboxRepository = new JsonOutboxRepository("outbox.json");
var bookingService = new BookingService(bookingRepository, outboxRepository);

var app = new BookingApp(bookingService);
app.Run();