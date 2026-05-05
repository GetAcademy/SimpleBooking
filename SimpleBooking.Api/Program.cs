using Microsoft.EntityFrameworkCore;
using SimpleBooking.Core.AppService;
using SimpleBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=SimpleBooking.db"));

builder.Services.AddScoped<IBookingRepository, SqlBookingRepository>();
builder.Services.AddScoped<IOutboxRepository, SqlOutboxRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<BookingService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public partial class Program { }
