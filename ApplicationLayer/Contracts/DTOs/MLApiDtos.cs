using Newtonsoft.Json;

namespace ApplicationLayer.Contracts.DTOs;

public class MLApiRequestDto
{
    [JsonProperty("therapeutic_area")]
    public string TherapeuticArea { get; set; } = string.Empty;

    [JsonProperty("specific_product")]
    public string? SpecificProduct { get; set; }

    [JsonProperty("indication")]
    public string? Indication { get; set; }

    [JsonProperty("target_geography")]
    public string TargetGeography { get; set; } = string.Empty;

    [JsonProperty("research_depth")]
    public string ResearchDepth { get; set; } = "standard";

    [JsonProperty("files")]
    public List<MLApiFileDto> Files { get; set; } = new();
}

public class MLApiFileDto
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
