using Application.Abstractions.Data;
using Domain.Events;
using Domain.Reservations;
using Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database
{
    public sealed class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options
    ) : DbContext(options), IApplicationDbContext
    {
        public DbSet<Event> Events { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Venue> Venues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Default);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}