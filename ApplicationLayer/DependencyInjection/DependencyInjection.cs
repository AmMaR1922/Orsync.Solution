using ApplicationLayer.Services;
using ApplicationLayer.UseCases;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ApplicationLayer.UseCases.GenerateMarketAnalysis;



namespace ApplicationLayer.DependencyInjection
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // AutoMapper
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);

            // FluentValidation
            services.AddValidatorsFromAssemblyContaining<GenerateMarketAnalysisValidator>();

            // Services
            services.AddScoped<SWOTAnalyzer>();
            services.AddScoped<ReportGeneratorService>();

            // Use Cases
            services.AddScoped<GenerateMarketAnalysisUseCase>();

            return services;
        }
    }

}
