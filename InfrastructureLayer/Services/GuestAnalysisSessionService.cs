using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace InfrastructureLayer.Services;

public class GuestAnalysisSessionService : IGuestAnalysisSessionService
{
    private static readonly TimeSpan SessionSlidingExpiration = TimeSpan.FromHours(8);

    private readonly IMemoryCache _cache;

    public GuestAnalysisSessionService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string CreateSessionId() => Guid.NewGuid().ToString("N");

    public Task SaveAsync(string sessionId, GenerateMarketAnalysisResponse response, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cacheItem = new GuestStoredAnalysis
        {
            ReportId = string.IsNullOrWhiteSpace(response.Id) ? Guid.NewGuid().ToString("N") : response.Id,
            ResponseJson = JsonConvert.SerializeObject(response),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _cache.Set(GetCacheKey(sessionId), cacheItem, new MemoryCacheEntryOptions
        {
            SlidingExpiration = SessionSlidingExpiration
        });

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GenerateMarketAnalysisResponse>> GetAllAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var analysis = GetStoredAnalysis(sessionId);
        if (analysis == null)
            return Task.FromResult<IReadOnlyList<GenerateMarketAnalysisResponse>>(Array.Empty<GenerateMarketAnalysisResponse>());

        var response = Deserialize(analysis.ResponseJson);
        if (response == null)
            return Task.FromResult<IReadOnlyList<GenerateMarketAnalysisResponse>>(Array.Empty<GenerateMarketAnalysisResponse>());

        RefreshSession(sessionId, analysis);
        return Task.FromResult<IReadOnlyList<GenerateMarketAnalysisResponse>>(new[] { response });
    }

    public Task<GenerateMarketAnalysisResponse?> GetByIdAsync(string sessionId, string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var analysis = GetStoredAnalysis(sessionId);
        if (analysis == null || !IsMatch(analysis, id))
            return Task.FromResult<GenerateMarketAnalysisResponse?>(null);

        RefreshSession(sessionId, analysis);
        return Task.FromResult(Deserialize(analysis.ResponseJson));
    }

    public Task<bool> DeleteAsync(string sessionId, string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var analysis = GetStoredAnalysis(sessionId);
        if (analysis == null || !IsMatch(analysis, id))
            return Task.FromResult(false);

        _cache.Remove(GetCacheKey(sessionId));
        return Task.FromResult(true);
    }

    private GuestStoredAnalysis? GetStoredAnalysis(string sessionId)
    {
        _cache.TryGetValue(GetCacheKey(sessionId), out GuestStoredAnalysis? analysis);
        return analysis;
    }

    private void RefreshSession(string sessionId, GuestStoredAnalysis analysis)
    {
        _cache.Set(GetCacheKey(sessionId), analysis, new MemoryCacheEntryOptions
        {
            SlidingExpiration = SessionSlidingExpiration
        });
    }

    private static string GetCacheKey(string sessionId) => $"guest-analysis:{sessionId}";

    private static bool IsMatch(GuestStoredAnalysis analysis, string id)
    {
        return string.Equals(analysis.ReportId, id, StringComparison.OrdinalIgnoreCase);
    }

    private static GenerateMarketAnalysisResponse? Deserialize(string responseJson)
    {
        return JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(responseJson);
    }

    private sealed class GuestStoredAnalysis
    {
        public string ReportId { get; init; } = string.Empty;
        public string ResponseJson { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
    }
}
