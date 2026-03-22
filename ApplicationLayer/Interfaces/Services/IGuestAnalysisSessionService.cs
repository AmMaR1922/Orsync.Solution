using ApplicationLayer.Contracts.DTOs;

namespace ApplicationLayer.Interfaces.Services;

public interface IGuestAnalysisSessionService
{
    string CreateSessionId();

    Task SaveAsync(string sessionId, GenerateMarketAnalysisResponse response, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GenerateMarketAnalysisResponse>> GetAllAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<GenerateMarketAnalysisResponse?> GetByIdAsync(string sessionId, string id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string sessionId, string id, CancellationToken cancellationToken = default);
}
