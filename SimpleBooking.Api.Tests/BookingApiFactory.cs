using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleBooking.Infrastructure;

namespace SimpleBooking.Api.Tests
{
    public class BookingApiFactory : WebApplicationFactory<Program>
    {
        public readonly string DbPath = Path.GetTempFileName();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BookingDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<BookingDbContext>(options =>
                    options.UseSqlite($"Data Source={DbPath}"));
            });
        }

        protected override void Dispose(bool disposing)
        {
            try { File.Delete(DbPath); } catch { }
            base.Dispose(disposing);
        }
    }
}
