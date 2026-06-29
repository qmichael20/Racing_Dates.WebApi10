using System;
using Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.ModelConfigurations
{
    public class VenueConfig : IEntityTypeConfiguration<Venue>
    {
        public void Configure(EntityTypeBuilder<Venue> builder)
        {
            builder.ToTable("Venues");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(v => v.Capacity)
                .IsRequired();

            builder.Property(v => v.Ciudad)
                .IsRequired()
                .HasMaxLength(100);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint(
                    "CK_Venue_Capacity_Positive",
                    "[Capacity] > 0");
            });
        }
    }
}