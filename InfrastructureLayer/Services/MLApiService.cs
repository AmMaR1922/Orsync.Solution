 using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Services;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace InfrastructureLayer.Services;

public sealed class MlApiHttpException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseBody { get; }

    public MlApiHttpException(HttpStatusCode statusCode, string responseBody)
        : base($"ML API Error ({statusCode}): {responseBody}")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

public class MLApiService : IMLApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MLApiService> _logger;
    private readonly string _mlApiBaseUrl;
    private readonly string? _mlApiFallbackBaseUrl;
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

        _mlApiFallbackBaseUrl = configuration["MLApi:FallbackBaseUrl"];
        _mlApiKey = configuration["MLApi:ApiKey"];

        if (int.TryParse(configuration["MLApi:TimeoutSeconds"], out var timeoutSeconds))
            _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<GenerateMarketAnalysisResponse> GenerateAnalysisAsync(
        MLApiRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var responseContent = await GenerateAnalysisRawAsync(request, cancellationToken);
        var result = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(responseContent);

        if (result == null)
            throw new Exception("Invalid response from ML API");

        return result;
    }

    public async Task<string> GenerateAnalysisRawAsync(
        MLApiRequestDto request,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        foreach (var baseUrl in GetCandidateBaseUrls())
        {
            var requestUrl = $"{baseUrl}/api/v1/report";
            _logger.LogInformation("Calling ML API: {Url}", requestUrl);

            try
            {
                var responseContent = await SendJsonRequestAsync(requestUrl, request, cancellationToken);

                if (IsMissingTherapeuticAreaValidationError(responseContent))
                {
                    _logger.LogWarning("ML API rejected JSON schema; retrying with multipart/form-data payload.");
                    responseContent = await SendMultipartWithRetryAsync(requestUrl, request, cancellationToken);
                }

                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "ML API host/network failure for {BaseUrl}. Trying next candidate.", baseUrl);
            }
            catch (MlApiHttpException ex) when ((int)ex.StatusCode >= 500)
            {
                lastException = ex;
                _logger.LogWarning(ex, "ML API server error for {BaseUrl}. Trying multipart fallback then next candidate.", baseUrl);

                try
                {
                    return await SendMultipartWithRetryAsync(requestUrl, request, cancellationToken);
                }
                catch (Exception fallbackEx)
                {
                    lastException = fallbackEx;
                    _logger.LogWarning(fallbackEx, "Multipart fallback failed for {BaseUrl}. Trying next candidate.", baseUrl);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "ML API call failed for {BaseUrl}. Trying next candidate.", baseUrl);
            }
        }

        throw lastException ?? new Exception("ML API call failed for all configured base URLs");
    }

    private async Task<string> SendJsonRequestAsync(
        string requestUrl,
        MLApiRequestDto request,
        CancellationToken cancellationToken)
    {
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

        httpRequest.Content = new StringContent(
            JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("ML API Status (JSON): {Status}", response.StatusCode);

        if (response.IsSuccessStatusCode)
            return responseContent;

        if (response.StatusCode != HttpStatusCode.UnprocessableEntity)
        {
            _logger.LogError("ML API ERROR RESPONSE (JSON): {Response}", responseContent);
            throw new MlApiHttpException(response.StatusCode, responseContent);
        }

        return responseContent;
    }

    private async Task<string> SendMultipartWithRetryAsync(
        string requestUrl,
        MLApiRequestDto request,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await SendMultipartRequestAsync(requestUrl, request, cancellationToken);
            }
            catch (MlApiHttpException ex) when (attempt < maxAttempts && (int)ex.StatusCode >= 500)
            {
                _logger.LogWarning(
                    ex,
                    "Multipart attempt {Attempt}/{MaxAttempts} failed with upstream {Status}. Retrying...",
                    attempt,
                    maxAttempts,
                    ex.StatusCode);

                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
            }
        }

        return await SendMultipartRequestAsync(requestUrl, request, cancellationToken);
    }

    private async Task<string> SendMultipartRequestAsync(
        string requestUrl,
        MLApiRequestDto request,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);

        if (!string.IsNullOrWhiteSpace(_mlApiKey))
            httpRequest.Headers.Add("X-API-Key", _mlApiKey);

        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(request.TherapeuticArea ?? string.Empty), "therapeutic_area");

        if (!string.IsNullOrWhiteSpace(request.SpecificProduct))
            form.Add(new StringContent(request.SpecificProduct), "specific_product");

        if (!string.IsNullOrWhiteSpace(request.Indication))
            form.Add(new StringContent(request.Indication), "indication");

        // ✅ FIX: Send as comma-separated string, not multiple fields
        var geographyString = request.TargetGeography != null && request.TargetGeography.Any()
            ? string.Join(",", request.TargetGeography)
            : "Global";
        form.Add(new StringContent(geographyString), "target_geography");

        var depthString = request.ResearchDepth != null && request.ResearchDepth.Any()
            ? string.Join(",", request.ResearchDepth)
            : "standard";
        form.Add(new StringContent(depthString), "research_depth");

        // ✅ Upload files
        foreach (var file in request.Files ?? Enumerable.Empty<MLApiFileDto>())
        {
            if (string.IsNullOrWhiteSpace(file.FileUrl))
                continue;

            try
            {
                // ✅ FIX: Download once, not twice
                var fileBytes = await _httpClient.GetByteArrayAsync(file.FileUrl, cancellationToken);
                var byteContent = new ByteArrayContent(fileBytes);

                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                form.Add(
                    byteContent,
                    "files",
                    string.IsNullOrWhiteSpace(file.FileName) ? "upload.pdf" : file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping file upload to ML API for URL: {FileUrl}", file.FileUrl);
            }
        }

        httpRequest.Content = form;

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("ML API Status (Multipart): {Status}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ML API ERROR RESPONSE (Multipart): {Response}", responseContent);
            throw new MlApiHttpException(response.StatusCode, responseContent);
        }

        return responseContent;
    }

    private static bool IsMissingTherapeuticAreaValidationError(string responseContent)
    {
        return responseContent.Contains("\"therapeutic_area\"", StringComparison.OrdinalIgnoreCase)
               && responseContent.Contains("\"missing\"", StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerable<string> GetCandidateBaseUrls()
    {
        var urls = new List<string>();

        if (!string.IsNullOrWhiteSpace(_mlApiBaseUrl))
            urls.Add(_mlApiBaseUrl.TrimEnd('/'));

        if (!string.IsNullOrWhiteSpace(_mlApiFallbackBaseUrl))
            urls.Add(_mlApiFallbackBaseUrl.TrimEnd('/'));

        if (!string.IsNullOrWhiteSpace(_mlApiBaseUrl) &&
            _mlApiBaseUrl.Contains("-v2", StringComparison.OrdinalIgnoreCase))
        {
            urls.Add(_mlApiBaseUrl.Replace("-v2", string.Empty, StringComparison.OrdinalIgnoreCase).TrimEnd('/'));
        }

        return urls.Distinct(StringComparer.OrdinalIgnoreCase);
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
