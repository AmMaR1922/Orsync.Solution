 

using ApplicationLayer.Interfaces;
using ApplicationLayer.Interfaces.Infrastructure;
using ApplicationLayer.Services;
using InfrastructureLayer.Data.Context;
using InfrastructureLayer.Identity;
using InfrastructureLayer.Repositories;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfrastructureLayer.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
    
            // Database
           
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());
             
            // Identity (Working in .NET 8)
  
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
 
            // Repositories
      
            services.AddScoped<IMarketAnalysisRepository, MarketAnalysisRepository>();
 
            // Services
         
            services.AddScoped<IMarketForecastProvider, MarketForecastProvider>();
            services.AddScoped<IReportGenerator, ReportGeneratorService>();
            // ✨ إضافة جديدة: Token Service
            services.AddScoped<ITokenService, TokenService>();
            return services;
        }
    }
}

