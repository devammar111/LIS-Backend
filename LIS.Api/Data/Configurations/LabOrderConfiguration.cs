using LIS.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LIS.Api.Data.Configurations;

public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        // Persist enums as readable strings rather than ints.
        builder.Property(o => o.TestType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Explicit DateOnly <-> DateTime conversion so BOTH SQL Server and the SQLite
        // test provider translate ordering/filtering on CollectionDate server-side identically.
        builder.Property(o => o.CollectionDate)
            .HasConversion(
                new ValueConverter<DateOnly, DateTime>(
                    d => d.ToDateTime(TimeOnly.MinValue),
                    dt => DateOnly.FromDateTime(dt)));

        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasIndex(o => o.CollectionDate);
        builder.HasIndex(o => o.Priority);
    }
}
