using Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.ModelConfigurations
{
    public class ReservationConfig : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.Property(x => x.BuyerName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.BuyerEmail)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.ReservationCode)
                .HasMaxLength(9);

            builder.Property(x => x.CancelledAt);

            builder.Property(x => x.LostTickets)
                .IsRequired();

            builder.HasIndex(x => x.ReservationCode)
                .IsUnique();

            builder.HasOne<Domain.Events.Event>() 
                .WithMany()                   
                .HasForeignKey(x => x.EventId)  
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}