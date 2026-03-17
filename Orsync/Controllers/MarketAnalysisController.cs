using System.Security.Claims;
using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orsync.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketAnalysisController : ControllerBase
{
    private readonly IAnalysisRepository _analysisRepository;
    private readonly IUploadedFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMLApiService _mlApiService;
    private readonly ILogger<MarketAnalysisController> _logger;

    private const string GuestUserId = "anonymous";

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

    private string ResolveUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User?.FindFirstValue("sub")
               ?? GuestUserId;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ExtractReportId(string? responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        try
        {
            var token = JsonConvert.DeserializeObject<JToken>(responseJson);
            return token?["id"]?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool HasReportId(string? responseJson) =>
        !string.IsNullOrWhiteSpace(ExtractReportId(responseJson));

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

            if (geography == null || !geography.Any())
                return BadRequest("At least one Geography must be selected");

            if (researchDepth == null || !researchDepth.Any())
                return BadRequest("At least one ResearchDepth must be selected");

            var userId = ResolveUserId();
            var mlApiFiles = new List<MLApiFileDto>();
            var fileIds = new List<Guid>();

            if (files != null && files.Any())
            {
                var batchId = Guid.NewGuid();

                foreach (var file in files.Where(f => f.Length > 0))
                {
                    await using var stream = file.OpenReadStream();
                    var uploadResult = await _fileStorageService.UploadFileAsync(
                        stream,
                        file.FileName,
                        file.ContentType);

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
                        FileName = file.FileName,
                        FileUrl = uploadResult.PublicUrl,
                        FileSize = file.Length,
                        FileExtension = Path.GetExtension(file.FileName)
                    });
                }
            }

            var mlApiRequest = new MLApiRequestDto
            {
                TherapeuticArea = therapeuticArea.Trim(),
                SpecificProduct = NormalizeOptional(product),
                Indication = NormalizeOptional(indication),
                TargetGeography = geography.Select(g => g.ToString()).ToList(),
                ResearchDepth = researchDepth.Select(d => d.ToString().ToLowerInvariant()).ToList(),
                Files = mlApiFiles
            };

            var mlRawResponse = await _mlApiService.GenerateAnalysisRawAsync(mlApiRequest);

            JObject mlResponseObject;
            try
            {
                mlResponseObject = JsonConvert.DeserializeObject<JObject>(mlRawResponse) ?? new JObject();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse ML API response as JSON. Response: {Response}", mlRawResponse);
                return StatusCode(502, new
                {
                    error = "Bad Gateway",
                    message = "ML API returned an invalid JSON response."
                });
            }

            if (mlApiFiles.Any())
            {
                mlResponseObject["uploaded_files"] = JArray.FromObject(
                    mlApiFiles.Select(f => new UploadedFileUrlDto
                    {
                        FileId = f.FileId,
                        FileName = f.FileName,
                        FileUrl = f.FileUrl,
                        FileSize = f.FileSize,
                        FileExtension = f.FileExtension
                    }).ToList());
            }

            var finalResponseJson = mlResponseObject.ToString(Formatting.None);

            if (!HasReportId(finalResponseJson))
            {
                _logger.LogError("ML API response missing report id. Response: {Response}", finalResponseJson);
                return StatusCode(502, new
                {
                    error = "Bad Gateway",
                    message = "ML API response missing required 'id' field"
                });
            }

            var analysis = new Analysis(
                userId,
                therapeuticArea.Trim(),
                NormalizeOptional(product) ?? "General",
                NormalizeOptional(indication) ?? "General",
                geography,
                researchDepth);

            analysis.SetResponse(finalResponseJson);

            if (fileIds.Any())
                analysis.SetFileIds(fileIds);

            try
            {
                await _analysisRepository.AddAsync(analysis);
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning(dbEx, "Could not persist analysis to database. Returning ML response without storage.");

                mlResponseObject["storage_warning"] =
                    "Analysis generated but could not be saved to database (connection issue).";

                return Content(mlResponseObject.ToString(Formatting.None), "application/json");
            }

            return Content(finalResponseJson, "application/json");
        }
        catch (MlApiHttpException ex) when ((int)ex.StatusCode >= 500)
        {
            _logger.LogError(ex, "Upstream ML API server error in Generate");
            return StatusCode(502, new
            {
                error = "Bad Gateway",
                message = "ML API is temporarily unavailable. Please try again in a moment.",
                upstream_status = (int)ex.StatusCode,
                upstream_response = ex.ResponseBody
            });
        }
        catch (MlApiHttpException ex)
        {
            _logger.LogError(ex, "Upstream ML API error in Generate");
            return StatusCode(502, new
            {
                error = "Bad Gateway",
                message = ex.Message,
                upstream_status = (int)ex.StatusCode,
                upstream_response = ex.ResponseBody
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Generate");
            return StatusCode(500, new
            {
                error = "Internal Server Error",
                message = ex.Message
            });
        }
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var userId = ResolveUserId();

        try
        {
            var analyses = await _analysisRepository.GetByUserIdAsync(userId);

            var responses = analyses
                .Where(a => !string.IsNullOrWhiteSpace(a.ResponseJson) && HasReportId(a.ResponseJson))
                .Select(a =>
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<JToken>(a.ResponseJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "JSON deserialize failed for analysis id {AnalysisId}", a.Id);
                        return null;
                    }
                })
                .Where(r => r != null)
                .ToList();

            return Content(JsonConvert.SerializeObject(responses), "application/json");
        }
        catch (RetryLimitExceededException ex)
        {
            _logger.LogError(ex, "Database retry limit exceeded in GetAll");
            return StatusCode(503, new
            {
                error = "Service Unavailable",
                message = "Database connection is unavailable or login failed after retries."
            });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database unavailable in GetAll");
            return StatusCode(503, new
            {
                error = "Service Unavailable",
                message = "Database connection is unavailable."
            });
        }
      
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll Error");
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    private async Task<Analysis?> FindAnalysisAsync(string id, string userId)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var analysisByGuid = await _analysisRepository.GetByIdAsync(guidId);

            if (analysisByGuid != null &&
                string.Equals(analysisByGuid.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                return analysisByGuid;
            }

            return null;
        }

        var analyses = await _analysisRepository.GetByUserIdAsync(userId);

        return analyses.FirstOrDefault(a =>
        {
            if (string.IsNullOrWhiteSpace(a.ResponseJson))
                return false;

            var responseId = ExtractReportId(a.ResponseJson);
            return string.Equals(responseId, id, StringComparison.OrdinalIgnoreCase);
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var userId = ResolveUserId();
            var analysis = await FindAnalysisAsync(id, userId);

            if (analysis == null)
                return NotFound();

            return Content(analysis.ResponseJson, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetById");
            return StatusCode(500, new
            {
                error = "Internal Server Error",
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var userId = ResolveUserId();
            var analysis = await FindAnalysisAsync(id, userId);

            if (analysis == null)
                return NotFound();

            await _analysisRepository.DeleteAsync(analysis.Id);

            return Ok(new { message = "Deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Delete");
            return StatusCode(500, new
            {
                error = "Internal Server Error",
                message = ex.Message
            });
        }
    }
}
