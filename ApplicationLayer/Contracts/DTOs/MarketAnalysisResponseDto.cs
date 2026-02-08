using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contracts.DTOs
{
    public class MarketAnalysisResponseDto
    {
        public Guid Id { get; set; }
        public string TherapeuticArea { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Indication { get; set; } = string.Empty;
        public string Geography { get; set; } = string.Empty;
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public MarketForecastDto? MarketForecast { get; set; }
        public SWOTAnalysisDto? SWOTAnalysis { get; set; }
    }

}
