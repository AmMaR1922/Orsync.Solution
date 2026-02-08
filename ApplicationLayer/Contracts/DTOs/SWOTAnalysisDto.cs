using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contracts.DTOs
{
    public class SWOTAnalysisDto
    {
        public List<string> Strengths { get; set; } = new();
        public List<string> Weaknesses { get; set; } = new();
        public List<string> Opportunities { get; set; } = new();
        public List<string> Threats { get; set; } = new();
    }

}
