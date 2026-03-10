//using ApplicationLayer.Contracts.DTOs;
//using ApplicationLayer.Interfaces.Services;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using System.Text.Json;

//namespace InfrastructureLayer.Services;

//public class MLApiService : IMLApiService
//{
//    private readonly HttpClient _httpClient;
//    private readonly IConfiguration _configuration;
//    private readonly ILogger<MLApiService> _logger;
//    private readonly string _mlApiBaseUrl;
//    private readonly string? _mlApiKey;

//    public MLApiService(
//        HttpClient httpClient,
//        IConfiguration configuration,
//        ILogger<MLApiService> logger)
//    {
//        _httpClient = httpClient;
//        _configuration = configuration;
//        _logger = logger;

//        _mlApiBaseUrl = _configuration["MLApi:BaseUrl"]
//            ?? throw new InvalidOperationException("MLApi:BaseUrl not configured");

//        _mlApiKey = _configuration["MLApi:ApiKey"];

//        _logger.LogInformation("ML API Service initialized with BaseUrl: {BaseUrl}", _mlApiBaseUrl);
//    }

//    public async Task<GenerateMarketAnalysisResponse> GenerateAnalysisAsync(
//        MLApiRequestDto request,
//        CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("========================================");
//            _logger.LogInformation("ML API REQUEST - DEBUGGING");
//            _logger.LogInformation("========================================");

//            // Log received object
//            _logger.LogInformation("Received MLApiRequestDto:");
//            _logger.LogInformation("  - TherapeuticArea: '{Value}' (IsNullOrEmpty: {IsEmpty})",
//                request.TherapeuticArea, string.IsNullOrEmpty(request.TherapeuticArea));
//            _logger.LogInformation("  - SpecificProduct: '{Value}'",
//                request.SpecificProduct ?? "NULL");
//            _logger.LogInformation("  - Indication: '{Value}'",
//                request.Indication ?? "NULL");
//            _logger.LogInformation("  - TargetGeography: '{Value}' (IsNullOrEmpty: {IsEmpty})",
//                request.TargetGeography, string.IsNullOrEmpty(request.TargetGeography));
//            _logger.LogInformation("  - ResearchDepth: '{Value}' (IsNullOrEmpty: {IsEmpty})",
//                request.ResearchDepth, string.IsNullOrEmpty(request.ResearchDepth));
//            _logger.LogInformation("  - Files Count: {Count}", request.Files.Count);

//            if (request.Files.Any())
//            {
//                _logger.LogInformation("  - Files:");
//                foreach (var file in request.Files)
//                {
//                    _logger.LogInformation("    * {FileName} ({Size} bytes) - {Url}",
//                        file.FileName, file.FileSize, file.FileUrl);
//                }
//            }

//            // Build URL
//            var requestUrl = $"{_mlApiBaseUrl}/api/v1/report";
//            _logger.LogInformation("Target URL: {Url}", requestUrl);

//            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);

//            // Add API Key if exists
//            if (!string.IsNullOrWhiteSpace(_mlApiKey))
//            {
//                httpRequest.Headers.Add("X-API-Key", _mlApiKey);
//                _logger.LogInformation("API Key added to headers");
//            }
//            else
//            {
//                _logger.LogInformation("No API Key configured");
//            }

//            // Serialize request
//            var serializerOptions = new JsonSerializerOptions
//            {
//                PropertyNamingPolicy = null,  // Keep property names as-is (JsonPropertyName will handle conversion)
//                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
//                WriteIndented = true
//            };

//            var jsonContent = JsonSerializer.Serialize(request, serializerOptions);

//            _logger.LogInformation("========================================");
//            _logger.LogInformation("JSON BEING SENT TO ML API:");
//            _logger.LogInformation("========================================");
//            _logger.LogInformation(jsonContent);
//            _logger.LogInformation("========================================");

//            httpRequest.Content = new StringContent(
//                jsonContent,
//                System.Text.Encoding.UTF8,
//                "application/json");

//            // Log request details
//            _logger.LogInformation("HTTP Request Details:");
//            _logger.LogInformation("  - Method: {Method}", httpRequest.Method);
//            _logger.LogInformation("  - URL: {Url}", httpRequest.RequestUri);
//            _logger.LogInformation("  - Content-Type: {ContentType}", httpRequest.Content.Headers.ContentType);
//            _logger.LogInformation("  - Content Length: {Length} bytes", jsonContent.Length);

//            // Send request
//            _logger.LogInformation("Sending HTTP request to ML API...");
//            var startTime = DateTime.UtcNow;

//            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

//            var endTime = DateTime.UtcNow;
//            var duration = (endTime - startTime).TotalSeconds;

//            _logger.LogInformation("HTTP Response received in {Duration:F2} seconds", duration);
//            _logger.LogInformation("Response Status Code: {StatusCode}", response.StatusCode);

//            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

//            _logger.LogInformation("========================================");
//            _logger.LogInformation("ML API RESPONSE:");
//            _logger.LogInformation("========================================");

//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError("ML API returned error!");
//                _logger.LogError("Status Code: {StatusCode}", response.StatusCode);
//                _logger.LogError("Response Body:");
//                _logger.LogError(responseContent);
//                _logger.LogInformation("========================================");

//                throw new HttpRequestException(
//                    $"ML API failed with status {response.StatusCode}: {responseContent}");
//            }

//            _logger.LogInformation("ML API returned success (200 OK)");
//            _logger.LogDebug("Response Body (first 500 chars):");
//            _logger.LogDebug(responseContent.Length > 500
//                ? responseContent.Substring(0, 500) + "..."
//                : responseContent);
//            _logger.LogInformation("========================================");

//            // Deserialize response
//            var deserializerOptions = new JsonSerializerOptions
//            {
//                PropertyNameCaseInsensitive = true
//            };

//            var mlResponse = JsonSerializer.Deserialize<GenerateMarketAnalysisResponse>(
//                responseContent,
//                deserializerOptions);

//            if (mlResponse == null)
//            {
//                _logger.LogError("Deserialization returned null!");
//                throw new InvalidOperationException("ML API returned null response");
//            }

//            _logger.LogInformation("Response deserialized successfully");
//            _logger.LogInformation("Analysis ID: {Id}", mlResponse.Id);
//            _logger.LogInformation("Confidence Score: {Score}", mlResponse.ConfidenceScore);
//            _logger.LogInformation("========================================");

//            return mlResponse;
//        }
//        catch (TaskCanceledException ex)
//        {
//            _logger.LogError(ex, "ML API request timed out after {Timeout} seconds",
//                _httpClient.Timeout.TotalSeconds);
//            throw new TimeoutException($"ML API request timed out after {_httpClient.Timeout.TotalSeconds} seconds", ex);
//        }
//        catch (HttpRequestException ex)
//        {
//            _logger.LogError(ex, "ML API HTTP request failed");
//            throw;
//        }
//        catch (JsonException ex)
//        {
//            _logger.LogError(ex, "Failed to parse ML API response as JSON");
//            throw new InvalidOperationException("Invalid ML API response format", ex);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error calling ML API");
//            throw;
//        }
//    }

//    public async Task<bool> HealthCheckAsync()
//    {
//        try
//        {
//            var healthUrl = $"{_mlApiBaseUrl}/health";

//            _logger.LogInformation("Performing health check: {Url}", healthUrl);

//            var response = await _httpClient.GetAsync(healthUrl);

//            var isHealthy = response.IsSuccessStatusCode;

//            _logger.LogInformation("Health check result: {Status} ({StatusCode})",
//                isHealthy ? "Healthy" : "Unhealthy",
//                response.StatusCode);

//            if (!isHealthy)
//            {
//                var content = await response.Content.ReadAsStringAsync();
//                _logger.LogWarning("Health check failed. Response: {Response}", content);
//            }

//            return isHealthy;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogWarning(ex, "ML API health check failed with exception");
//            return false;
//        }
//    }
//}using ApplicationLayer.Contracts.DTOs;

#region MLApiService
using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Services;
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

            // ✅ Build JSON request (NOT form-data!)
            var jsonRequest = new
            {
                therapeutic_area = request.TherapeuticArea,
                specific_product = request.SpecificProduct,
                indication = request.Indication,
                target_geography = string.Join(",", request.TargetGeography),
                research_depth = string.Join(",", request.ResearchDepth),
                files = request.Files.Select(f => new
                {
                    file_id = f.FileId,
                    file_name = f.FileName,
                    file_url = f.FileUrl,
                    file_size = f.FileSize,
                    file_extension = f.FileExtension
                }).ToList()
            };

            var jsonContent = JsonConvert.SerializeObject(jsonRequest, Formatting.Indented);

            _logger.LogInformation("========================================");
            _logger.LogInformation("Request JSON:");
            _logger.LogInformation(jsonContent);
            _logger.LogInformation("========================================");

            httpRequest.Content = new StringContent(
                jsonContent,
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("ML API Status: {Status}", response.StatusCode);
            _logger.LogInformation("Response Length: {Length} characters", responseContent.Length);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("ML API Error: {Response}", responseContent);
                throw new Exception($"ML API Error ({response.StatusCode}): {responseContent}");
            }

            _logger.LogInformation("Deserializing response...");

            var result = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(responseContent);

            if (result == null)
                throw new JsonException("Received null when deserializing ML API response.");

            _logger.LogInformation("✓ Analysis ID: {Id}", result.Id);
            _logger.LogInformation("✓ Confidence: {Score}", result.ConfidenceScore);
            _logger.LogInformation("✓ Generated By: {Count} sources", result.GeneratedBy.Count);
            _logger.LogInformation("========================================");

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

#endregion