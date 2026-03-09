#region old
//using System.Text.Json.Serialization;

//namespace ApplicationLayer.Contracts.DTOs;

///// <summary>
///// ✨ الـ Response الرئيسي
///// </summary>
//public class GenerateMarketAnalysisResponse
//{
//    [JsonPropertyName("id")]
//    public string Id { get; set; } = string.Empty;

//    [JsonPropertyName("timestamp")]
//    public DateTime Timestamp { get; set; }

//    [JsonPropertyName("input")]
//    public InputDataDto Input { get; set; } = new();

//    [JsonPropertyName("confidence_score")]
//    public double ConfidenceScore { get; set; }

//    [JsonPropertyName("generated_by")]
//    public List<string> GeneratedBy { get; set; } = new();

//    [JsonPropertyName("uploaded_files")]
//    public List<UploadedFileUrlDto> UploadedFiles { get; set; } = new();

//    [JsonPropertyName("executive_summary")]
//    public string ExecutiveSummary { get; set; } = string.Empty;

//    [JsonPropertyName("market_overview")]
//    public MarketOverviewDto MarketOverview { get; set; } = new();

//    [JsonPropertyName("financial_forecast")]
//    public FinancialForecastDto FinancialForecast { get; set; } = new();

//    [JsonPropertyName("competitors")]
//    public List<CompetitorDto> Competitors { get; set; } = new();

//    [JsonPropertyName("scientific_evidence")]
//    public List<ScientificEvidenceDto> ScientificEvidence { get; set; } = new();

//    [JsonPropertyName("ai_insights")]
//    public AIInsightsDto AIInsights { get; set; } = new();

//    [JsonPropertyName("risks")]
//    public List<string> Risks { get; set; } = new();

//    [JsonPropertyName("triangulation")]
//    public TriangulationDto Triangulation { get; set; } = new();
//}

//// ========================================
//// ✅ INPUT - استقبل strings مش enums
//// ========================================

//public class InputDataDto
//{
//    [JsonPropertyName("topic")]
//    public string Topic { get; set; } = string.Empty;

//    [JsonPropertyName("product")]
//    public string Product { get; set; } = string.Empty;

//    [JsonPropertyName("indication")]
//    public string Indication { get; set; } = string.Empty;

//    // ✅ Changed to string - ML API returns string
//    [JsonPropertyName("geography")]
//    public string Geography { get; set; } = string.Empty;

//    // ✅ Changed to string - ML API returns string
//    [JsonPropertyName("depth")]
//    public string Depth { get; set; } = string.Empty;
//}

//// ========================================
//// FILES
//// ========================================

//public class UploadedFileUrlDto
//{
//    [JsonPropertyName("file_id")]
//    public string FileId { get; set; } = string.Empty;

//    [JsonPropertyName("file_name")]
//    public string FileName { get; set; } = string.Empty;

//    [JsonPropertyName("file_url")]
//    public string FileUrl { get; set; } = string.Empty;

//    [JsonPropertyName("file_size")]
//    public long FileSize { get; set; }

//    [JsonPropertyName("file_extension")]
//    public string FileExtension { get; set; } = string.Empty;
//}

//// ========================================
//// MARKET
//// ========================================

//public class MarketOverviewDto
//{
//    [JsonPropertyName("market_size")]
//    public string MarketSize { get; set; } = string.Empty;

//    [JsonPropertyName("cagr")]
//    public string Cagr { get; set; } = string.Empty;

//    [JsonPropertyName("forecast_year")]
//    public string ForecastYear { get; set; } = string.Empty;

//    [JsonPropertyName("key_trends")]
//    public List<string> KeyTrends { get; set; } = new();
//}

//// ========================================
//// FINANCIAL
//// ========================================

//public class FinancialForecastDto
//{
//    [JsonPropertyName("revenue_projections")]
//    public List<RevenueProjectionDto> RevenueProjections { get; set; } = new();

//    [JsonPropertyName("cagr")]
//    public string Cagr { get; set; } = string.Empty;

//    [JsonPropertyName("key_drivers")]
//    public List<string> KeyDrivers { get; set; } = new();
//}

//public class RevenueProjectionDto
//{
//    [JsonPropertyName("year")]
//    public string Year { get; set; } = string.Empty;

//    [JsonPropertyName("amount")]
//    public string Amount { get; set; } = string.Empty;
//}

//// ========================================
//// COMPETITORS
//// ========================================

//public class CompetitorDto
//{
//    [JsonPropertyName("name")]
//    public string Name { get; set; } = string.Empty;

//    [JsonPropertyName("product")]
//    public string Product { get; set; } = string.Empty;

//    [JsonPropertyName("mechanism")]
//    public string Mechanism { get; set; } = string.Empty;

//    [JsonPropertyName("status")]
//    public string Status { get; set; } = string.Empty;
//}

//// ========================================
//// SCIENTIFIC
//// ========================================

//public class ScientificEvidenceDto
//{
//    [JsonPropertyName("title")]
//    public string Title { get; set; } = string.Empty;

//    [JsonPropertyName("source")]
//    public string Source { get; set; } = string.Empty;

//    [JsonPropertyName("url")]
//    public string Url { get; set; } = string.Empty;

//    [JsonPropertyName("summary")]
//    public string Summary { get; set; } = string.Empty;
//}

//// ========================================
//// AI INSIGHTS
//// ========================================

//public class AIInsightsDto
//{
//    [JsonPropertyName("swot")]
//    public SWOTDto Swot { get; set; } = new();

//    [JsonPropertyName("regulatory")]
//    public RegulatoryDto Regulatory { get; set; } = new();

//    [JsonPropertyName("reimbursement")]
//    public ReimbursementDto Reimbursement { get; set; } = new();

//    [JsonPropertyName("strategic_recommendations")]
//    public List<string> StrategicRecommendations { get; set; } = new();
//}

//public class SWOTDto
//{
//    [JsonPropertyName("strengths")]
//    public List<string> Strengths { get; set; } = new();

//    [JsonPropertyName("weaknesses")]
//    public List<string> Weaknesses { get; set; } = new();

//    [JsonPropertyName("opportunities")]
//    public List<string> Opportunities { get; set; } = new();

//    [JsonPropertyName("threats")]
//    public List<string> Threats { get; set; } = new();
//}

//public class RegulatoryDto
//{
//    [JsonPropertyName("approval_pathways")]
//    public List<string> ApprovalPathways { get; set; } = new();

//    [JsonPropertyName("key_regulations")]
//    public List<string> KeyRegulations { get; set; } = new();

//    [JsonPropertyName("upcoming_changes")]
//    public List<string> UpcomingChanges { get; set; } = new();
//}

//public class ReimbursementDto
//{
//    [JsonPropertyName("payer_landscape")]
//    public List<string> PayerLandscape { get; set; } = new();

//    [JsonPropertyName("pricing_models")]
//    public List<string> PricingModels { get; set; } = new();

//    [JsonPropertyName("access_barriers")]
//    public List<string> AccessBarriers { get; set; } = new();
//}

//// ========================================
//// TRIANGULATION
//// ========================================

//public class TriangulationDto
//{
//    [JsonPropertyName("score")]
//    public double Score { get; set; }

//    [JsonPropertyName("points")]
//    public List<TriangulationPointDto> Points { get; set; } = new();

//    [JsonPropertyName("methodology")]
//    public string Methodology { get; set; } = string.Empty;
//}

//public class TriangulationPointDto
//{
//    [JsonPropertyName("claim")]
//    public string Claim { get; set; } = string.Empty;

//    [JsonPropertyName("sources")]
//    public List<string> Sources { get; set; } = new();

//    [JsonPropertyName("confidence")]
//    public double Confidence { get; set; }

//    [JsonPropertyName("status")]
//    public string Status { get; set; } = string.Empty;
//} 
#endregion


using Newtonsoft.Json;

namespace ApplicationLayer.Contracts.DTOs;

// ========================================
// ✅ MAIN RESPONSE
// ========================================

public class GenerateMarketAnalysisResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("total_time_seconds")]
    public double TotalTimeSeconds { get; set; }

    [JsonProperty("total_time")]
    public string TotalTime { get; set; } = string.Empty;

    [JsonProperty("token_usage")]
    public TokenUsageDto? TokenUsage { get; set; }

    [JsonProperty("input")]
    public InputDataDto Input { get; set; } = new();

    [JsonProperty("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonProperty("generated_by")]
    public List<string> GeneratedBy { get; set; } = new();

    [JsonProperty("executive_summary")]
    public ExecutiveSummaryDto ExecutiveSummary { get; set; } = new();

    [JsonProperty("analyze")]
    public AnalyzeDto Analyze { get; set; } = new();

    [JsonProperty("strategize")]
    public StrategizeDto Strategize { get; set; } = new();

    [JsonProperty("triangulation")]
    public TriangulationDto Triangulation { get; set; } = new();

    // ✅ للـ files اللي انت رفعتها
    [JsonProperty("uploaded_files")]
    public List<UploadedFileUrlDto> UploadedFiles { get; set; } = new();
}

// ========================================
// ✅ TOKEN USAGE
// ========================================

public class TokenUsageDto
{
    [JsonProperty("total_input_tokens")]
    public int TotalInputTokens { get; set; }

    [JsonProperty("total_output_tokens")]
    public int TotalOutputTokens { get; set; }

    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonProperty("per_component")]
    public Dictionary<string, ComponentTokensDto> PerComponent { get; set; } = new();
}

public class ComponentTokensDto
{
    [JsonProperty("input_tokens")]
    public int InputTokens { get; set; }

    [JsonProperty("output_tokens")]
    public int OutputTokens { get; set; }

    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }
}

// ========================================
// ✅ INPUT
// ========================================

public class InputDataDto
{
    [JsonProperty("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonProperty("product")]
    public string? Product { get; set; }

    [JsonProperty("indication")]
    public string? Indication { get; set; }

    [JsonProperty("geography")]
    public string Geography { get; set; } = string.Empty;

    [JsonProperty("depth")]
    public string Depth { get; set; } = string.Empty;
}

// ========================================
// ✅ EXECUTIVE SUMMARY
// ========================================

public class ExecutiveSummaryDto
{
    [JsonProperty("market_overview")]
    public MarketOverviewSummaryDto MarketOverview { get; set; } = new();

    [JsonProperty("competition_overview")]
    public CompetitionOverviewDto CompetitionOverview { get; set; } = new();

    [JsonProperty("market_insights")]
    public List<string> MarketInsights { get; set; } = new();

    [JsonProperty("strategic_vision")]
    public StrategicVisionDto StrategicVision { get; set; } = new();
}

public class MarketOverviewSummaryDto
{
    [JsonProperty("defined_market")]
    public string DefinedMarket { get; set; } = string.Empty;

    [JsonProperty("maturity")]
    public string Maturity { get; set; } = string.Empty;

    [JsonProperty("current_total_revenue_and_growth_percentage")]
    public string CurrentTotalRevenueAndGrowthPercentage { get; set; } = string.Empty;

    [JsonProperty("sob_and_sector_share_percentage")]
    public string SobAndSectorSharePercentage { get; set; } = string.Empty;

    [JsonProperty("brand_current_contribution_percentage")]
    public string BrandCurrentContributionPercentage { get; set; } = string.Empty;
}

public class CompetitionOverviewDto
{
    [JsonProperty("main_competitors")]
    public List<string> MainCompetitors { get; set; } = new();

    [JsonProperty("current_contribution_and_growth_percentage")]
    public string CurrentContributionAndGrowthPercentage { get; set; } = string.Empty;

    [JsonProperty("market_leader_and_why")]
    public string MarketLeaderAndWhy { get; set; } = string.Empty;

    [JsonProperty("usp_maintaining_leader_position")]
    public string UspMaintainingLeaderPosition { get; set; } = string.Empty;

    [JsonProperty("new_entrants")]
    public List<string> NewEntrants { get; set; } = new();

    [JsonProperty("important_tactics_to_change_game")]
    public List<string> ImportantTacticsToChangeGame { get; set; } = new();
}

public class StrategicVisionDto
{
    [JsonProperty("short_term_objectives")]
    public string ShortTermObjectives { get; set; } = string.Empty;

    [JsonProperty("medium_term_objectives")]
    public string MediumTermObjectives { get; set; } = string.Empty;
}

// ========================================
// ✅ ANALYZE
// ========================================

public class AnalyzeDto
{
    [JsonProperty("market")]
    public MarketAnalysisDto Market { get; set; } = new();

    [JsonProperty("customers")]
    public CustomersDto Customers { get; set; } = new();

    [JsonProperty("competitors")]
    public List<CompetitorDetailDto> Competitors { get; set; } = new();

    [JsonProperty("brand")]
    public BrandDto Brand { get; set; } = new();

    [JsonProperty("risks")]
    public List<string> Risks { get; set; } = new();

    [JsonProperty("regulatory_and_market_access")]
    public List<string> RegulatoryAndMarketAccess { get; set; } = new();

    [JsonProperty("pestle")]
    public PestleDto Pestle { get; set; } = new();

    [JsonProperty("swot")]
    public SwotDto Swot { get; set; } = new();
}

public class MarketAnalysisDto
{
    [JsonProperty("sales_history")]
    public List<SalesHistoryDto> SalesHistory { get; set; } = new();

    [JsonProperty("growth_annotations")]
    public List<GrowthAnnotationDto> GrowthAnnotations { get; set; } = new();

    [JsonProperty("overall_metrics")]
    public OverallMetricsDto OverallMetrics { get; set; } = new();
}

public class SalesHistoryDto
{
    [JsonProperty("year")]
    public string Year { get; set; } = string.Empty;

    [JsonProperty("total_sales")]
    public double TotalSales { get; set; }

    [JsonProperty("segments")]
    public List<SegmentDto> Segments { get; set; } = new();
}

public class SegmentDto
{
    [JsonProperty("segment_name")]
    public string SegmentName { get; set; } = string.Empty;

    [JsonProperty("absolute_value")]
    public double AbsoluteValue { get; set; }

    [JsonProperty("share_percentage")]
    public double SharePercentage { get; set; }
}

public class GrowthAnnotationDto
{
    [JsonProperty("start_year")]
    public string StartYear { get; set; } = string.Empty;

    [JsonProperty("end_year")]
    public string EndYear { get; set; } = string.Empty;

    [JsonProperty("growth_percentage")]
    public double GrowthPercentage { get; set; }
}

public class OverallMetricsDto
{
    [JsonProperty("yoy_growth_percentage")]
    public double YoyGrowthPercentage { get; set; }

    [JsonProperty("cagr_percentage")]
    public double CagrPercentage { get; set; }

    [JsonProperty("value_market_share_percentage")]
    public double ValueMarketSharePercentage { get; set; }

    [JsonProperty("unit_market_share_percentage")]
    public double UnitMarketSharePercentage { get; set; }

    [JsonProperty("market_share_gain_points")]
    public double MarketShareGainPoints { get; set; }
}

public class CustomersDto
{
    [JsonProperty("segmentation")]
    public string Segmentation { get; set; } = string.Empty;

    [JsonProperty("patient_journey")]
    public PatientJourneyDto PatientJourney { get; set; } = new();
}

public class PatientJourneyDto
{
    [JsonProperty("awareness")]
    public string Awareness { get; set; } = string.Empty;

    [JsonProperty("diagnosis")]
    public string Diagnosis { get; set; } = string.Empty;

    [JsonProperty("treatment_decision")]
    public string TreatmentDecision { get; set; } = string.Empty;

    [JsonProperty("treatment")]
    public string Treatment { get; set; } = string.Empty;

    [JsonProperty("monitoring")]
    public string Monitoring { get; set; } = string.Empty;
}

public class CompetitorDetailDto
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("growth_percentage")]
    public double GrowthPercentage { get; set; }

    [JsonProperty("vms_percentage")]
    public double VmsPercentage { get; set; }

    [JsonProperty("ums_percentage")]
    public double UmsPercentage { get; set; }

    [JsonProperty("msg")]
    public double Msg { get; set; }

    [JsonProperty("molecule")]
    public string Molecule { get; set; } = string.Empty;

    [JsonProperty("moa")]
    public string Moa { get; set; } = string.Empty;

    [JsonProperty("dosage_form")]
    public string DosageForm { get; set; } = string.Empty;

    [JsonProperty("dose_regimen")]
    public string DoseRegimen { get; set; } = string.Empty;

    [JsonProperty("positioning")]
    public string Positioning { get; set; } = string.Empty;

    [JsonProperty("unique_selling_points")]
    public string UniqueSellingPoints { get; set; } = string.Empty;

    [JsonProperty("pharmaceutical_lines")]
    public string PharmaceuticalLines { get; set; } = string.Empty;

    [JsonProperty("sov_son")]
    public double SovSon { get; set; }
}

public class BrandDto
{
    [JsonProperty("growth_percentage")]
    public double GrowthPercentage { get; set; }

    [JsonProperty("vms_percentage")]
    public double VmsPercentage { get; set; }

    [JsonProperty("ums_percentage")]
    public double UmsPercentage { get; set; }

    [JsonProperty("msg")]
    public double Msg { get; set; }
}

public class PestleDto
{
    [JsonProperty("political")]
    public List<string> Political { get; set; } = new();

    [JsonProperty("economic")]
    public List<string> Economic { get; set; } = new();

    [JsonProperty("social")]
    public List<string> Social { get; set; } = new();

    [JsonProperty("technological")]
    public List<string> Technological { get; set; } = new();

    [JsonProperty("legal")]
    public List<string> Legal { get; set; } = new();

    [JsonProperty("ecological")]
    public List<string> Ecological { get; set; } = new();
}

public class SwotDto
{
    [JsonProperty("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonProperty("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonProperty("opportunities")]
    public List<string> Opportunities { get; set; } = new();

    [JsonProperty("threats")]
    public List<string> Threats { get; set; } = new();
}

// ========================================
// ✅ STRATEGIZE
// ========================================

public class StrategizeDto
{
    [JsonProperty("stp")]
    public StpDto Stp { get; set; } = new();

    [JsonProperty("one_page_strategy")]
    public OnePageStrategyDto OnePageStrategy { get; set; } = new();
}

public class StpDto
{
    [JsonProperty("segmentation")]
    public string Segmentation { get; set; } = string.Empty;

    [JsonProperty("targeting")]
    public string Targeting { get; set; } = string.Empty;

    [JsonProperty("positioning")]
    public string Positioning { get; set; } = string.Empty;
}

public class OnePageStrategyDto
{
    [JsonProperty("target_audiences_rx_share_percentage")]
    public string TargetAudiences { get; set; } = string.Empty;

    [JsonProperty("strategic_imperatives")]
    public List<string> StrategicImperatives { get; set; } = new();

    [JsonProperty("key_actions_or_tactics")]
    public List<string> KeyActions { get; set; } = new();

    [JsonProperty("go_to_market")]
    public List<string> GoToMarket { get; set; } = new();

    [JsonProperty("kpis")]
    public List<string> Kpis { get; set; } = new();
}

// ========================================
// ✅ TRIANGULATION
// ========================================

public class TriangulationDto
{
    [JsonProperty("score")]
    public double Score { get; set; }

    [JsonProperty("points")]
    public List<TriangulationPointDto> Points { get; set; } = new();

    [JsonProperty("methodology")]
    public string Methodology { get; set; } = string.Empty;
}

public class TriangulationPointDto
{
    [JsonProperty("claim")]
    public string Claim { get; set; } = string.Empty;

    [JsonProperty("sources")]
    public List<string> Sources { get; set; } = new();

    [JsonProperty("confidence")]
    public double Confidence { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}

// ========================================
// ✅ UPLOADED FILES
// ========================================

public class UploadedFileUrlDto
{
    [JsonProperty("file_id")]
    public string FileId { get; set; } = string.Empty;

    [JsonProperty("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonProperty("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [JsonProperty("file_size")]
    public long FileSize { get; set; }

    [JsonProperty("file_extension")]
    public string FileExtension { get; set; } = string.Empty;
}