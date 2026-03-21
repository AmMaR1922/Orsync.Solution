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

        var cacheItem = GetOrCreateSession(sessionId);
        var reportId = string.IsNullOrWhiteSpace(response.Id)
            ? Guid.NewGuid().ToString("N")
            : response.Id;

        var stored = new GuestStoredAnalysis
        {
            LocalId = Guid.NewGuid().ToString("N"),
            ReportId = reportId,
            ResponseJson = JsonConvert.SerializeObject(response),
            CreatedAt = DateTimeOffset.UtcNow
        };

        lock (cacheItem.SyncRoot)
        {
            var existing = cacheItem.Analyses.FirstOrDefault(a => IsMatch(a, reportId));
            if (existing != null)
                cacheItem.Analyses.Remove(existing);

            cacheItem.Analyses.Add(stored);
        }

        RefreshSession(sessionId, cacheItem);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GenerateMarketAnalysisResponse>> GetAllAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_cache.TryGetValue(GetCacheKey(sessionId), out GuestSessionCacheItem? cacheItem))
            return Task.FromResult<IReadOnlyList<GenerateMarketAnalysisResponse>>(Array.Empty<GenerateMarketAnalysisResponse>());

        List<GenerateMarketAnalysisResponse> responses;
        lock (cacheItem!.SyncRoot)
        {
            responses = cacheItem.Analyses
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => Deserialize(a.ResponseJson))
                .Where(a => a != null)
                .Cast<GenerateMarketAnalysisResponse>()
                .ToList();
        }

        RefreshSession(sessionId, cacheItem);
        return Task.FromResult<IReadOnlyList<GenerateMarketAnalysisResponse>>(responses);
    }

    public Task<GenerateMarketAnalysisResponse?> GetByIdAsync(string sessionId, string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_cache.TryGetValue(GetCacheKey(sessionId), out GuestSessionCacheItem? cacheItem))
            return Task.FromResult<GenerateMarketAnalysisResponse?>(null);

        GuestStoredAnalysis? match;
        lock (cacheItem!.SyncRoot)
        {
            match = cacheItem.Analyses.FirstOrDefault(a => IsMatch(a, id));
        }

        RefreshSession(sessionId, cacheItem);
        return Task.FromResult(match == null ? null : Deserialize(match.ResponseJson));
    }

    public Task<bool> DeleteAsync(string sessionId, string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_cache.TryGetValue(GetCacheKey(sessionId), out GuestSessionCacheItem? cacheItem))
            return Task.FromResult(false);

        bool removed;
        lock (cacheItem!.SyncRoot)
        {
            removed = cacheItem.Analyses.RemoveAll(a => IsMatch(a, id)) > 0;
        }

        RefreshSession(sessionId, cacheItem);
        return Task.FromResult(removed);
    }

    private GuestSessionCacheItem GetOrCreateSession(string sessionId)
    {
        return _cache.GetOrCreate(GetCacheKey(sessionId), entry =>
        {
            entry.SlidingExpiration = SessionSlidingExpiration;
            return new GuestSessionCacheItem();
        })!;
    }

    private void RefreshSession(string sessionId, GuestSessionCacheItem cacheItem)
    {
        _cache.Set(GetCacheKey(sessionId), cacheItem, new MemoryCacheEntryOptions
        {
            SlidingExpiration = SessionSlidingExpiration
        });
    }

    private static string GetCacheKey(string sessionId) => $"guest-analysis:{sessionId}";

    private static bool IsMatch(GuestStoredAnalysis analysis, string id)
    {
        return string.Equals(analysis.ReportId, id, StringComparison.OrdinalIgnoreCase)
               || string.Equals(analysis.LocalId, id, StringComparison.OrdinalIgnoreCase);
    }

    private static GenerateMarketAnalysisResponse? Deserialize(string responseJson)
    {
        return JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(responseJson);
    }

    private sealed class GuestSessionCacheItem
    {
        public object SyncRoot { get; } = new();
        public List<GuestStoredAnalysis> Analyses { get; } = new();
    }

    private sealed class GuestStoredAnalysis
    {
        public string LocalId { get; init; } = string.Empty;
        public string ReportId { get; init; } = string.Empty;
        public string ResponseJson { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
    }
}
