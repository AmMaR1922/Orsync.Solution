using ApplicationLayer.Interfaces.Infrastructure;
using DomainLayer.Entities;
using InfrastructureLayer.Configurations;
using InfrastructureLayer.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Data.Context
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MarketAnalysis> MarketAnalyses => Set<MarketAnalysis>();
        public DbSet<MarketForecast> MarketForecasts => Set<MarketForecast>();
        public DbSet<SWOTAnalysis> SWOTAnalyses => Set<SWOTAnalysis>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply configurations
            builder.ApplyConfiguration(new MarketAnalysisConfiguration());
            builder.ApplyConfiguration(new MarketForecastConfiguration());
            builder.ApplyConfiguration(new SWOTAnalysisConfiguration());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }

}
