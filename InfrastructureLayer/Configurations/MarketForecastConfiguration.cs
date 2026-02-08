using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Configurations
{
    public class MarketForecastConfiguration : IEntityTypeConfiguration<MarketForecast>
    {
        public void Configure(EntityTypeBuilder<MarketForecast> builder)
        {
            builder.ToTable("MarketForecasts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MarketSizeInBillions)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.CAGR)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.ForecastYears)
                .IsRequired();

            builder.OwnsOne(x => x.Confidence, confidence =>
            {
                confidence.Property(c => c.Level)
                    .HasColumnName("ConfidenceLevel")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            // Foreign key to MarketAnalysis
            builder.HasOne<MarketAnalysis>()
                .WithOne(a => a.MarketForecast)
                .HasForeignKey<MarketForecast>(f => f.MarketAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
