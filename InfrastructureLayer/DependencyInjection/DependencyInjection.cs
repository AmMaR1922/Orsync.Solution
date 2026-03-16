using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using InfrastructureLayer.Data.Context;
using InfrastructureLayer.Data.Repositories;
using InfrastructureLayer.Identity;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ITokenService = InfrastructureLayer.Services.ITokenService;

namespace InfrastructureLayer.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("InfrastructureLayer")
                      .EnableRetryOnFailure(
                          maxRetryCount: 5,
                          maxRetryDelay: TimeSpan.FromSeconds(10),
                          errorNumbersToAdd: null)
            ));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped<IAnalysisRepository, AnalysisRepository>();
        services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();

        // Services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddHttpClient<IMLApiService, MLApiService>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var timeoutSeconds = int.Parse(config["MLApi:TimeoutSeconds"] ?? "600");
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}