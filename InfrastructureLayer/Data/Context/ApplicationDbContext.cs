//using DomainLayer.Entities;
//using InfrastructureLayer.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

//namespace InfrastructureLayer.Data.Context;

//public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
//{
//    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//        : base(options)
//    {
//    }

//    public DbSet<Analysis> Analyses { get; set; }
//    public DbSet<UploadedFile> UploadedFiles { get; set; }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        modelBuilder.Entity<Analysis>(entity =>
//        {
//            entity.ToTable("Analyses");
//            entity.HasKey(e => e.Id);

//            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
//            entity.Property(e => e.TherapeuticArea).IsRequired().HasMaxLength(200);
//            entity.Property(e => e.Product).IsRequired().HasMaxLength(200);
//            entity.Property(e => e.Indication).IsRequired().HasMaxLength(200);
//            entity.Property(e => e.TargetGeographyJson)
//          .HasColumnName("TargetGeography")
//          .HasColumnType("nvarchar(max)");

//            entity.Property(e => e.ResearchDepthJson)
//                  .HasColumnName("ResearchDepth")
//                  .HasColumnType("nvarchar(max)");
//            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
//            entity.Property(e => e.ResponseJson).HasColumnType("nvarchar(max)");
//            entity.Property(e => e.FileIdsJson).HasColumnType("nvarchar(max)");

//            entity.HasIndex(e => e.UserId);
//            entity.HasIndex(e => e.CreatedAt);
//        });

//        modelBuilder.Entity<UploadedFile>(entity =>
//        {
//            entity.ToTable("UploadedFiles");
//            entity.HasKey(e => e.Id);

//            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
//            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
//            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
//            entity.Property(e => e.FileSize).IsRequired();
//            entity.Property(e => e.FileExtension).IsRequired().HasMaxLength(10);
//            entity.Property(e => e.BatchId).IsRequired();

//            entity.HasIndex(e => e.BatchId);
//        });
//    }
//}


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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ========================================
        // Analysis Configuration
        // ========================================

        builder.Entity<Analysis>(entity =>
        {
            entity.ToTable("Analyses");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.TherapeuticArea)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Product)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Indication)
                .IsRequired()
                .HasMaxLength(200);

            // ✅ Store as JSON strings
            entity.Property(e => e.GeographyJson)
                .IsRequired()
                .HasColumnName("Geography");

            entity.Property(e => e.ResearchDepthJson)
                .IsRequired()
                .HasColumnName("ResearchDepth");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ResponseJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.FileIdsJson)
                .HasColumnName("FileIds")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ========================================
        // UploadedFile Configuration
        // ========================================

        builder.Entity<UploadedFile>(entity =>
        {
            entity.ToTable("UploadedFiles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FileExtension)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.BatchId);
        });
    }
}