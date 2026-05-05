using Microsoft.EntityFrameworkCore;
using SimpleBooking.Core.Model;

namespace SimpleBooking.Infrastructure
{
    public class BookingDbContext : DbContext
    {
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Date, e.Hour }).IsUnique();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
