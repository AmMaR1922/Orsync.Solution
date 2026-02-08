using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{

    public class SWOTAnalysis : BaseEntity
    {
        public List<string> Strengths { get; private set; }
        public List<string> Weaknesses { get; private set; }
        public List<string> Opportunities { get; private set; }
        public List<string> Threats { get; private set; }

        // Navigation property
        public Guid MarketAnalysisId { get; private set; }

        private SWOTAnalysis()
        {
            Strengths = new List<string>();
            Weaknesses = new List<string>();
            Opportunities = new List<string>();
            Threats = new List<string>();
        }

        public SWOTAnalysis(
            List<string> strengths,
            List<string> weaknesses,
            List<string> opportunities,
            List<string> threats) : this()
        {
            Strengths = strengths ?? new List<string>();
            Weaknesses = weaknesses ?? new List<string>();
            Opportunities = opportunities ?? new List<string>();
            Threats = threats ?? new List<string>();
        }

        public void SetMarketAnalysisId(Guid marketAnalysisId)
        {
            MarketAnalysisId = marketAnalysisId;
        }

        public void AddStrength(string strength)
        {
            if (!string.IsNullOrWhiteSpace(strength))
            {
                Strengths.Add(strength);
                SetUpdatedAt();
            }
        }

        public void AddWeakness(string weakness)
        {
            if (!string.IsNullOrWhiteSpace(weakness))
            {
                Weaknesses.Add(weakness);
                SetUpdatedAt();
            }
        }

        public void AddOpportunity(string opportunity)
        {
            if (!string.IsNullOrWhiteSpace(opportunity))
            {
                Opportunities.Add(opportunity);
                SetUpdatedAt();
            }
        }

        public void AddThreat(string threat)
        {
            if (!string.IsNullOrWhiteSpace(threat))
            {
                Threats.Add(threat);
                SetUpdatedAt();
            }
        }
    }

}
