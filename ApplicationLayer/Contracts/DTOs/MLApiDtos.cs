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
    public List<string> TargetGeography { get; set; } = new();

    [JsonProperty("research_depth")]
    public List<string> ResearchDepth { get; set; } = new();


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
