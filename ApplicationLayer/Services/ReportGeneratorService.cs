using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{

    public class ReportGeneratorService : IReportGenerator
    {
        public string GenerateExecutiveSummary(
            MarketAnalysis analysis,
            MarketForecast forecast,
            SWOTAnalysis swotAnalysis)
        {
            var summary = $@"
EXECUTIVE SUMMARY
Market Analysis Report for {analysis.Product}

Therapeutic Area: {analysis.TherapeuticArea}
Indication: {analysis.Indication}
Geography: {analysis.Geography}
Analysis Date: {analysis.CreatedAt:yyyy-MM-dd}

MARKET OVERVIEW:
The {analysis.TherapeuticArea} market for {analysis.Indication} in {analysis.Geography} demonstrates significant potential. Our analysis projects a market size of ${forecast.MarketSizeInBillions:F2} billion with a compound annual growth rate (CAGR) of {forecast.CAGR:F1}% over the next {forecast.ForecastYears} years. This forecast carries a {forecast.Confidence} confidence level based on current market dynamics and historical trends.

KEY FINDINGS:

Strengths:
{string.Join("\n", swotAnalysis.Strengths.Select((s, i) => $"{i + 1}. {s}"))}

Weaknesses:
{string.Join("\n", swotAnalysis.Weaknesses.Select((w, i) => $"{i + 1}. {w}"))}

Opportunities:
{string.Join("\n", swotAnalysis.Opportunities.Select((o, i) => $"{i + 1}. {o}"))}

Threats:
{string.Join("\n", swotAnalysis.Threats.Select((t, i) => $"{i + 1}. {t}"))}

RECOMMENDATION:
Based on the comprehensive SWOT analysis and market forecast, {analysis.Product} shows promise in the {analysis.Geography} market. Strategic positioning should leverage identified strengths while addressing key weaknesses. Market entry or expansion strategies should capitalize on growth opportunities while mitigating potential threats through robust risk management and competitive intelligence.

The projected {forecast.CAGR:F1}% CAGR indicates a favorable market trajectory, supporting investment decisions in this therapeutic area. However, stakeholders should remain vigilant to competitive dynamics and regulatory changes that may impact market performance.
";

            return summary.Trim();
        }
    }

}
