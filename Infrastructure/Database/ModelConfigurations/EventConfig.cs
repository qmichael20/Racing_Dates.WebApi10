using Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.ModelConfigurations
{
    public class EventConfig : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(e => e.VenueId)
                .IsRequired();

            builder.Property(e => e.MaxCapacity)
                .IsRequired();

            builder.Property(e => e.StartDate)
                .IsRequired();

            builder.Property(e => e.EndDate)
                .IsRequired();

            builder.Property(e => e.TicketPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.HasOne(e => e.Venue)        
                .WithMany()                       
                .HasForeignKey(e => e.VenueId)    
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Event_MaxCapacity_Positive",
                    "[MaxCapacity] > 0");

                t.HasCheckConstraint(
                    "CK_Event_TicketPrice_Positive",
                    "[TicketPrice] > 0");

                t.HasCheckConstraint(
                    "CK_Event_Dates",
                    "[EndDate] > [StartDate]");
            });
        }
    }
}