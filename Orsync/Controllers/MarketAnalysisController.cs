using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Orsync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketAnalysisController : ControllerBase
{
    private const string GuestSessionHeaderName = "X-Guest-Session-Id";

    private readonly IAnalysisRepository _analysisRepository;
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMLApiService _mlApiService;
    private readonly IGuestAnalysisSessionService _guestAnalysisSessionService;
    private readonly ILogger<MarketAnalysisController> _logger;

    public MarketAnalysisController(
        IAnalysisRepository analysisRepository,
        IUploadedFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IMLApiService mlApiService,
        IGuestAnalysisSessionService guestAnalysisSessionService,
        ILogger<MarketAnalysisController> logger)
    {
        _analysisRepository = analysisRepository;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _mlApiService = mlApiService;
        _guestAnalysisSessionService = guestAnalysisSessionService;
        _logger = logger;
    }

    private string? GetAuthenticatedUserIdOrNull()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private string? GetGuestSessionIdOrNull()
    {
        if (!Request.Headers.TryGetValue(GuestSessionHeaderName, out var values))
            return null;

        var sessionId = values.FirstOrDefault();
        return string.IsNullOrWhiteSpace(sessionId) ? null : sessionId.Trim();
    }

    private void AttachGuestSessionContext(GenerateMarketAnalysisResponse response, string guestSessionId)
    {
        response.Extra ??= new Dictionary<string, JToken>();
        response.Extra["guest_mode"] = JToken.FromObject(true);
        response.Extra["guest_session_id"] = JToken.FromObject(guestSessionId);
        Response.Headers[GuestSessionHeaderName] = guestSessionId;
    }

    private static string? ExtractReportId(string? responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        try
        {
            return JObject.Parse(responseJson)["id"]?.ToString();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Generate(
        [FromForm] string therapeuticArea,
        [FromForm] string? product,
        [FromForm] string? indication,
        [FromForm] List<TargetGeography> geography,
        [FromForm] List<ResearchDepth> researchDepth,
        [FromForm] List<IFormFile>? files)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(therapeuticArea))
                return BadRequest("TherapeuticArea is required");

            if (!geography.Any())
                return BadRequest("At least one Geography must be selected");

            if (!researchDepth.Any())
                return BadRequest("At least one ResearchDepth must be selected");

            var authenticatedUserId = GetAuthenticatedUserIdOrNull();
            var isGuest = string.IsNullOrWhiteSpace(authenticatedUserId);
            var guestSessionId = isGuest
                ? GetGuestSessionIdOrNull() ?? _guestAnalysisSessionService.CreateSessionId()
                : null;

            var mlApiFiles = new List<MLApiFileDto>();
            var fileIds = new List<Guid>();

            if (files != null && files.Any())
            {
                var batchId = Guid.NewGuid();

                foreach (var file in files.Where(f => f.Length > 0))
                {
                    await using var stream = file.OpenReadStream();
                    var uploadResult = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

                    if (!isGuest)
                    {
                        var uploadedFile = new UploadedFile(
                            authenticatedUserId!,
                            file.FileName,
                            uploadResult.FilePath,
                            file.Length,
                            Path.GetExtension(file.FileName),
                            batchId);

                        await _fileRepository.AddAsync(uploadedFile);
                        fileIds.Add(uploadedFile.Id);

                        mlApiFiles.Add(new MLApiFileDto
                        {
                            FileId = uploadedFile.Id.ToString(),
                            FileName = uploadedFile.FileName,
                            FileUrl = uploadResult.PublicUrl,
                            FileSize = uploadedFile.FileSize,
                            FileExtension = uploadedFile.FileExtension
                        });
                    }
                    else
                    {
                        mlApiFiles.Add(new MLApiFileDto
                        {
                            FileId = Guid.NewGuid().ToString("N"),
                            FileName = file.FileName,
                            FileUrl = uploadResult.PublicUrl,
                            FileSize = file.Length,
                            FileExtension = Path.GetExtension(file.FileName)
                        });
                    }
                }
            }

            var mlApiRequest = new MLApiRequestDto
            {
                TherapeuticArea = therapeuticArea.Trim(),
                SpecificProduct = product?.Trim(),
                Indication = indication?.Trim(),
                TargetGeography = geography.Select(g => g.ToString()).ToList(),
                ResearchDepth = researchDepth.Select(d => d.ToString().ToLowerInvariant()).ToList(),
                Files = mlApiFiles
            };

            var mlResponse = await _mlApiService.GenerateAnalysisAsync(mlApiRequest);
            mlResponse.UploadedFiles = mlApiFiles.Select(f => new UploadedFileUrlDto
            {
                FileId = f.FileId,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                FileSize = f.FileSize,
                FileExtension = f.FileExtension
            }).ToList();

            if (isGuest)
            {
                AttachGuestSessionContext(mlResponse, guestSessionId!);
                await _guestAnalysisSessionService.SaveAsync(guestSessionId!, mlResponse);
                return Ok(mlResponse);
            }

            var analysis = new Analysis(
                authenticatedUserId!,
                therapeuticArea.Trim(),
                product?.Trim() ?? "General",
                indication?.Trim() ?? "General",
                geography,
                researchDepth);

            analysis.SetResponse(JsonConvert.SerializeObject(mlResponse));

            if (fileIds.Any())
                analysis.SetFileIds(fileIds);

            await _analysisRepository.AddAsync(analysis);

            return Ok(mlResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Generate");
            return StatusCode(500, new { error = "Internal Server Error", message = ex.Message });
        }
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var authenticatedUserId = GetAuthenticatedUserIdOrNull();
            if (!string.IsNullOrWhiteSpace(authenticatedUserId))
            {
                var analyses = await _analysisRepository.GetByUserIdAsync(authenticatedUserId);

                var responses = analyses
                    .Where(a => !string.IsNullOrWhiteSpace(a.ResponseJson))
                    .Select(a =>
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(a.ResponseJson);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "JSON Deserialize Failed for analysis {AnalysisId}", a.Id);
                            return null;
                        }
                    })
                    .Where(r => r != null)
                    .ToList();

                return Ok(responses);
            }

            var guestSessionId = GetGuestSessionIdOrNull();
            if (string.IsNullOrWhiteSpace(guestSessionId))
                return Ok(Array.Empty<GenerateMarketAnalysisResponse>());

            var guestResponses = await _guestAnalysisSessionService.GetAllAsync(guestSessionId);
            Response.Headers[GuestSessionHeaderName] = guestSessionId;
            return Ok(guestResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll Error");
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    private async Task<Analysis?> FindUserAnalysisAsync(string id, string userId)
    {
        if (Guid.TryParse(id, out var guidId))
            return await _analysisRepository.GetByIdAsync(guidId);

        var analyses = await _analysisRepository.GetByUserIdAsync(userId);

        return analyses.FirstOrDefault(a =>
            string.Equals(ExtractReportId(a.ResponseJson), id, StringComparison.OrdinalIgnoreCase));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var authenticatedUserId = GetAuthenticatedUserIdOrNull();
        if (!string.IsNullOrWhiteSpace(authenticatedUserId))
        {
            var analysis = await FindUserAnalysisAsync(id, authenticatedUserId);

            if (analysis == null)
                return NotFound();

            if (analysis.UserId != authenticatedUserId)
                return Forbid();

            var response = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(analysis.ResponseJson);
            return Ok(response);
        }

        var guestSessionId = GetGuestSessionIdOrNull();
        if (string.IsNullOrWhiteSpace(guestSessionId))
            return NotFound();

        var guestResponse = await _guestAnalysisSessionService.GetByIdAsync(guestSessionId, id);
        if (guestResponse == null)
            return NotFound();

        Response.Headers[GuestSessionHeaderName] = guestSessionId;
        return Ok(guestResponse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var authenticatedUserId = GetAuthenticatedUserIdOrNull();
        if (!string.IsNullOrWhiteSpace(authenticatedUserId))
        {
            var analysis = await FindUserAnalysisAsync(id, authenticatedUserId);

            if (analysis == null)
                return NotFound();

            if (analysis.UserId != authenticatedUserId)
                return Forbid();

            await _analysisRepository.DeleteAsync(analysis.Id);
            return Ok(new { message = "Deleted successfully" });
        }

        var guestSessionId = GetGuestSessionIdOrNull();
        if (string.IsNullOrWhiteSpace(guestSessionId))
            return NotFound();

        var deleted = await _guestAnalysisSessionService.DeleteAsync(guestSessionId, id);
        if (!deleted)
            return NotFound();

        Response.Headers[GuestSessionHeaderName] = guestSessionId;
        return Ok(new { message = "Deleted successfully" });
    }
}
