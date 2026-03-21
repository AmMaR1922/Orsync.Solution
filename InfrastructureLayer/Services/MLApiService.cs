using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Services;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        if (int.TryParse(configuration["MLApi:TimeoutSeconds"], out var timeout))
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
    }

    public async Task<GenerateMarketAnalysisResponse> GenerateAnalysisAsync(
        MLApiRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUrl = $"{_mlApiBaseUrl.TrimEnd('/')}/api/v1/report";
            _logger.LogInformation("Calling ML API: {Url}", requestUrl);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            if (!string.IsNullOrWhiteSpace(_mlApiKey))
                httpRequest.Headers.Add("X-API-Key", _mlApiKey);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(request.TherapeuticArea ?? string.Empty), "therapeutic_area");

            if (!string.IsNullOrWhiteSpace(request.SpecificProduct))
                form.Add(new StringContent(request.SpecificProduct), "specific_product");

            if (!string.IsNullOrWhiteSpace(request.Indication))
                form.Add(new StringContent(request.Indication), "indication");

            foreach (var geo in request.TargetGeography ?? Enumerable.Empty<string>())
                form.Add(new StringContent(geo), "target_geography");

            foreach (var depth in request.ResearchDepth ?? Enumerable.Empty<string>())
                form.Add(new StringContent(depth), "research_depth");

            foreach (var file in request.Files ?? Enumerable.Empty<MLApiFileDto>())
            {
                if (string.IsNullOrWhiteSpace(file.FileUrl))
                    continue;

                using var fileResponse = await _httpClient.GetAsync(file.FileUrl, cancellationToken);
                if (!fileResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Skipping file {FileName}; source URL returned {StatusCode}.", file.FileName, fileResponse.StatusCode);
                    continue;
                }

                var fileBytes = await fileResponse.Content.ReadAsByteArrayAsync(cancellationToken);
                var fileContent = new ByteArrayContent(fileBytes);
                var contentType = fileResponse.Content.Headers.ContentType?.ToString();
                fileContent.Headers.TryAddWithoutValidation(
                    HeaderNames.ContentType,
                    string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

                form.Add(fileContent, "files", string.IsNullOrWhiteSpace(file.FileName) ? "upload.bin" : file.FileName);
            }

            httpRequest.Content = form;

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("ML API Status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ML API error response: {Response}", responseContent);
                throw new HttpRequestException($"ML API Error ({response.StatusCode}): {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(responseContent);
            if (result == null)
                throw new JsonException("Invalid response from ML API");

            _logger.LogInformation("ML Analysis generated successfully. ID: {ReportId}", result.Id);
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
            using var response = await _httpClient.GetAsync($"{_mlApiBaseUrl.TrimEnd('/')}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
