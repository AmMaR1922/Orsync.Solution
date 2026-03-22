using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Orsync.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketAnalysisController : ControllerBase
{
    private readonly IAnalysisRepository _analysisRepository;
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMLApiService _mlApiService;
    private readonly ILogger<MarketAnalysisController> _logger;

    public MarketAnalysisController(
        IAnalysisRepository analysisRepository,
        IUploadedFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IMLApiService mlApiService,
        ILogger<MarketAnalysisController> logger)
    {
        _analysisRepository = analysisRepository;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _mlApiService = mlApiService;
        _logger = logger;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found");

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

            var userId = GetUserId();
            var mlApiFiles = new List<MLApiFileDto>();
            var fileIds = new List<Guid>();

            if (files != null && files.Any())
            {
                var batchId = Guid.NewGuid();

                foreach (var file in files.Where(f => f.Length > 0))
                {
                    await using var stream = file.OpenReadStream();
                    var uploadResult = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

                    var uploadedFile = new UploadedFile(
                        userId,
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

            var analysis = new Analysis(
                userId,
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
            var userId = GetUserId();
            var analyses = await _analysisRepository.GetByUserIdAsync(userId);

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
        var userId = GetUserId();
        var analysis = await FindUserAnalysisAsync(id, userId);

        if (analysis == null)
            return NotFound();

        if (analysis.UserId != userId)
            return Forbid();

        var response = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(analysis.ResponseJson);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        var analysis = await FindUserAnalysisAsync(id, userId);

        if (analysis == null)
            return NotFound();

        if (analysis.UserId != userId)
            return Forbid();

        await _analysisRepository.DeleteAsync(analysis.Id);
        return Ok(new { message = "Deleted successfully" });
    }
}
