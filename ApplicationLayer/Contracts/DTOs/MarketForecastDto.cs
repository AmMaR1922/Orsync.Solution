using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contracts.DTOs
{

    public class MarketForecastDto
    {
        public decimal MarketSizeInBillions { get; set; }
        public decimal CAGR { get; set; }
        public int ForecastYears { get; set; }
        public string Confidence { get; set; } = string.Empty;
    }

}
