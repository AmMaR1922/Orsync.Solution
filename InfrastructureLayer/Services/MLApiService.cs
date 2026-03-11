using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace InfrastructureLayer.Services;

public class MLApiService : IMLApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MLApiService> _logger;
    private readonly string _mlApiBaseUrl;
    private readonly string? _mlApiKey;

    public MLApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MLApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _mlApiBaseUrl = configuration["MLApi:BaseUrl"]
            ?? throw new InvalidOperationException("MLApi:BaseUrl not configured");

        _mlApiKey = configuration["MLApi:ApiKey"];

        if (int.TryParse(configuration["MLApi:TimeoutSeconds"], out int timeout))
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
    }

    public async Task<GenerateMarketAnalysisResponse> GenerateAnalysisAsync(
        MLApiRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUrl = $"{_mlApiBaseUrl}/api/v1/report";

            _logger.LogInformation("========================================");
            _logger.LogInformation("Calling ML API: {Url}", requestUrl);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            if (!string.IsNullOrWhiteSpace(_mlApiKey))
                httpRequest.Headers.Add("X-API-Key", _mlApiKey);

            var payload = new
            {
                therapeutic_area = request.TherapeuticArea,
                specific_product = request.SpecificProduct,
                indication = request.Indication,
                target_geography = request.TargetGeography ?? new List<string>(),
                research_depth = request.ResearchDepth ?? new List<string>(),
                files = request.Files?.Select(f => new
                {
                    file_id = f.FileId,
                    file_name = f.FileName,
                    file_url = f.FileUrl,
                    file_size = f.FileSize,
                    file_extension = f.FileExtension
                }) ?? Enumerable.Empty<object>()
            };

            var payloadJson = JsonConvert.SerializeObject(payload);
            httpRequest.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("ML API Status: {Status}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ML API ERROR RESPONSE:");
                _logger.LogError(responseContent);

                throw new Exception($"ML API Error ({response.StatusCode}): {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(responseContent);

            if (result == null)
                throw new Exception("Invalid response from ML API");

            _logger.LogInformation("✓ ML Analysis generated successfully. ID: {Id}", result.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML API call failed");
            throw;
        }
    }

    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_mlApiBaseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

