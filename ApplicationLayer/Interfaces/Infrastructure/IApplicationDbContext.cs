using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces.Infrastructure
{

    public interface IApplicationDbContext
    {
        DbSet<MarketAnalysis> MarketAnalyses { get; }
        DbSet<MarketForecast> MarketForecasts { get; }
        DbSet<SWOTAnalysis> SWOTAnalyses { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}
