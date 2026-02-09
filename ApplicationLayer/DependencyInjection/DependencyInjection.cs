using ApplicationLayer.Services;
using ApplicationLayer.UseCases;
using ApplicationLayer.UseCases.GenerateMarketAnalysis;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace ApplicationLayer.DependencyInjection
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // AutoMapper
            //services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

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
