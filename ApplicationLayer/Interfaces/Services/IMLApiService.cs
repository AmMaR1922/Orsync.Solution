using ApplicationLayer.Contracts.DTOs;

namespace ApplicationLayer.Interfaces.Services;

/// <summary>
/// Interface للتواصل مع ML API
/// </summary>
public interface IMLApiService
{
    /// <summary>
    /// إرسال request للـ ML API واستقبال النتيجة
    /// </summary>
    Task<GenerateMarketAnalysisResponse> GenerateAnalysisAsync(
        MLApiRequestDto request,
        CancellationToken cancellationToken = default);

    Task<string> GenerateAnalysisRawAsync(
        MLApiRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// التحقق من صحة الـ ML API
    /// </summary>
    Task<bool> HealthCheckAsync();
}
