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

    public class SWOTAnalysisConfiguration : IEntityTypeConfiguration<SWOTAnalysis>
    {
        public void Configure(EntityTypeBuilder<SWOTAnalysis> builder)
        {
            builder.ToTable("SWOTAnalyses");

            builder.HasKey(x => x.Id);

            // Store lists as JSON
            builder.Property(x => x.Strengths)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Weaknesses)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Opportunities)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Threats)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            // Foreign key to MarketAnalysis
            builder.HasOne<MarketAnalysis>()
                .WithOne(a => a.SWOTAnalysis)
                .HasForeignKey<SWOTAnalysis>(s => s.MarketAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
