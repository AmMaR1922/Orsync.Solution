using DomainLayer.Entities;
using DomainLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Configurations
{
    public class MarketAnalysisConfiguration : IEntityTypeConfiguration<MarketAnalysis>
    {
        public void Configure(EntityTypeBuilder<MarketAnalysis> builder)
        {
            builder.ToTable("MarketAnalyses");

            builder.HasKey(x => x.Id);

            builder.OwnsOne(x => x.TherapeuticArea, therapeuticArea =>
            {
                therapeuticArea.Property(t => t.Name)
                    .HasColumnName("TherapeuticArea")
                    .HasMaxLength(200)
                    .IsRequired();
            });

            builder.Property(x => x.Product)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Indication)
                .HasMaxLength(300)
                .IsRequired();

            builder.OwnsOne(x => x.Geography, geography =>
            {
                geography.Property(g => g.Region)
                    .HasColumnName("Geography")
                    .HasMaxLength(100)
                    .IsRequired();
            });

            builder.Property(x => x.ExecutiveSummary)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (AnalysisStatus)Enum.Parse(typeof(AnalysisStatus), v))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            // Relationships
            builder.HasOne(x => x.MarketForecast)
                .WithOne()
                .HasForeignKey<MarketForecast>(f => f.MarketAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SWOTAnalysis)
                .WithOne()
                .HasForeignKey<SWOTAnalysis>(s => s.MarketAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAt);
        }
    }

}
