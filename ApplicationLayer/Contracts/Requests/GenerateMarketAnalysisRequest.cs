using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contracts.Requests
{

    public class GenerateMarketAnalysisRequest
    {
        public string TherapeuticArea { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Indication { get; set; } = string.Empty;
        public string Geography { get; set; } = string.Empty;
    }

}
