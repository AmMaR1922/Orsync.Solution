using System.Text.Json.Serialization;

namespace ApplicationLayer.Contracts.DTOs;

public class GenerateMarketAnalysisResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("input")]
    public InputDataDto Input { get; set; } = new();

    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("generated_by")]
    public List<string> GeneratedBy { get; set; } = new();

    [JsonPropertyName("executive_summary")]
    public string ExecutiveSummary { get; set; } = string.Empty;

    [JsonPropertyName("market_overview")]
    public MarketOverviewDto MarketOverview { get; set; } = new();

    [JsonPropertyName("financial_forecast")]
    public FinancialForecastDto FinancialForecast { get; set; } = new();

    [JsonPropertyName("competitors")]
    public List<CompetitorDto> Competitors { get; set; } = new();

    [JsonPropertyName("scientific_evidence")]
    public List<ScientificEvidenceDto> ScientificEvidence { get; set; } = new();

    [JsonPropertyName("ai_insights")]
    public AIInsightsDto AIInsights { get; set; } = new();

    [JsonPropertyName("risks")]
    public List<string> Risks { get; set; } = new();

    [JsonPropertyName("triangulation")]
    public TriangulationDto Triangulation { get; set; } = new();
}

public class InputDataDto
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("product")]
    public string Product { get; set; } = string.Empty;

    [JsonPropertyName("indication")]
    public string Indication { get; set; } = string.Empty;

    [JsonPropertyName("geography")]
    public string Geography { get; set; } = string.Empty;

    [JsonPropertyName("depth")]
    public string Depth { get; set; } = string.Empty;
}

public class MarketOverviewDto
{
    [JsonPropertyName("market_size")]
    public string MarketSize { get; set; } = string.Empty;

    [JsonPropertyName("cagr")]
    public string Cagr { get; set; } = string.Empty;

    [JsonPropertyName("forecast_year")]
    public string ForecastYear { get; set; } = string.Empty;

    [JsonPropertyName("key_trends")]
    public List<string> KeyTrends { get; set; } = new();
}

public class FinancialForecastDto
{
    [JsonPropertyName("revenue_projections")]
    public List<RevenueProjectionDto> RevenueProjections { get; set; } = new();

    [JsonPropertyName("cagr")]
    public string Cagr { get; set; } = string.Empty;

    [JsonPropertyName("key_drivers")]
    public List<string> KeyDrivers { get; set; } = new();
}

public class RevenueProjectionDto
{
    [JsonPropertyName("year")]
    public string Year { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}

public class CompetitorDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("product")]
    public string Product { get; set; } = string.Empty;

    [JsonPropertyName("mechanism")]
    public string Mechanism { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class ScientificEvidenceDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
}

public class AIInsightsDto
{
    [JsonPropertyName("swot")]
    public SWOTDto Swot { get; set; } = new();

    [JsonPropertyName("regulatory")]
    public RegulatoryDto Regulatory { get; set; } = new();

    [JsonPropertyName("reimbursement")]
    public ReimbursementDto Reimbursement { get; set; } = new();

    [JsonPropertyName("strategic_recommendations")]
    public List<string> StrategicRecommendations { get; set; } = new();
}

public class SWOTDto
{
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("opportunities")]
    public List<string> Opportunities { get; set; } = new();

    [JsonPropertyName("threats")]
    public List<string> Threats { get; set; } = new();
}

public class RegulatoryDto
{
    [JsonPropertyName("approval_pathways")]
    public List<string> ApprovalPathways { get; set; } = new();

    [JsonPropertyName("key_regulations")]
    public List<string> KeyRegulations { get; set; } = new();

    [JsonPropertyName("upcoming_changes")]
    public List<string> UpcomingChanges { get; set; } = new();
}

public class ReimbursementDto
{
    [JsonPropertyName("payer_landscape")]
    public List<string> PayerLandscape { get; set; } = new();

    [JsonPropertyName("pricing_models")]
    public List<string> PricingModels { get; set; } = new();

    [JsonPropertyName("access_barriers")]
    public List<string> AccessBarriers { get; set; } = new();
}

public class TriangulationDto
{
    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("points")]
    public List<TriangulationPointDto> Points { get; set; } = new();

    [JsonPropertyName("methodology")]
    public string Methodology { get; set; } = string.Empty;
}

public class TriangulationPointDto
{
    [JsonPropertyName("claim")]
    public string Claim { get; set; } = string.Empty;

    [JsonPropertyName("sources")]
    public List<string> Sources { get; set; } = new();

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}