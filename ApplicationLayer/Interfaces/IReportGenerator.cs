using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IReportGenerator
    {
        string GenerateExecutiveSummary(
            MarketAnalysis analysis,
            MarketForecast forecast,
            SWOTAnalysis swotAnalysis);
    }

}
