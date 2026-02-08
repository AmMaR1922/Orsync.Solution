using DomainLayer.Common;
using DomainLayer.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{

    public class MarketForecast : BaseEntity
    {
        public decimal MarketSizeInBillions { get; private set; }
        public decimal CAGR { get; private set; } // Compound Annual Growth Rate
        public int ForecastYears { get; private set; }
        public ConfidenceLevel Confidence { get; private set; }

        // Navigation property
        public Guid MarketAnalysisId { get; private set; }

        private MarketForecast() { } // For EF Core

        public MarketForecast(
            decimal marketSizeInBillions,
            decimal cagr,
            int forecastYears,
            ConfidenceLevel confidence)
        {
            if (marketSizeInBillions <= 0)
                throw new ArgumentException("Market size must be positive", nameof(marketSizeInBillions));

            if (cagr < -100 || cagr > 1000)
                throw new ArgumentException("CAGR must be between -100% and 1000%", nameof(cagr));

            if (forecastYears <= 0 || forecastYears > 20)
                throw new ArgumentException("Forecast years must be between 1 and 20", nameof(forecastYears));

            MarketSizeInBillions = marketSizeInBillions;
            CAGR = cagr;
            ForecastYears = forecastYears;
            Confidence = confidence ?? ConfidenceLevel.Medium;
        }

        public void SetMarketAnalysisId(Guid marketAnalysisId)
        {
            MarketAnalysisId = marketAnalysisId;
        }

        public void UpdateForecast(decimal marketSize, decimal cagr, int years, ConfidenceLevel confidence)
        {
            MarketSizeInBillions = marketSize;
            CAGR = cagr;
            ForecastYears = years;
            Confidence = confidence;
            SetUpdatedAt();
        }
    }

}
