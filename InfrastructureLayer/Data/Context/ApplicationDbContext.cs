using DomainLayer.Entities;
using InfrastructureLayer.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Data.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Analysis> Analyses { get; set; }
    public DbSet<UploadedFile> UploadedFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Analysis>(entity =>
        {
            entity.ToTable("Analyses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.TherapeuticArea).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Product).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Indication).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Geography).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ResearchDepth).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ResponseJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FileIdsJson).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<UploadedFile>(entity =>
        {
            entity.ToTable("UploadedFiles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.FileExtension).IsRequired().HasMaxLength(10);
            entity.Property(e => e.BatchId).IsRequired();

            entity.HasIndex(e => e.BatchId);
        });
    }
}