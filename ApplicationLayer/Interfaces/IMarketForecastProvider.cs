using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IMarketForecastProvider
    {
        Task<MarketForecast> GenerateForecastAsync(
            string therapeuticArea,
            string product,
            string indication,
            string geography,
            CancellationToken cancellationToken = default);
    }

}
