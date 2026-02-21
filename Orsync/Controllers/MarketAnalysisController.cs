//using ApplicationLayer.Contracts.DTOs;
//using ApplicationLayer.Interfaces.Repositories;
//using ApplicationLayer.Interfaces.Services;
//using DomainLayer.Entities;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using System.Security.Claims;

//namespace Orsync.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize]
//public class MarketAnalysisController : ControllerBase
//{
//    private readonly IAnalysisRepository _analysisRepository;
//    private readonly IUploadedFileRepository _fileRepository;
//    private readonly IFileStorageService _fileStorageService;
//    private readonly IMLApiService _mlApiService;
//    private readonly ILogger<MarketAnalysisController> _logger;
//    private readonly IConfiguration _configuration;

//    public MarketAnalysisController(
//        IAnalysisRepository analysisRepository,
//        IUploadedFileRepository fileRepository,
//        IFileStorageService fileStorageService,
//        IMLApiService mlApiService,
//        ILogger<MarketAnalysisController> logger,
//        IConfiguration configuration)
//    {
//        _analysisRepository = analysisRepository;
//        _fileRepository = fileRepository;
//        _fileStorageService = fileStorageService;
//        _mlApiService = mlApiService;
//        _logger = logger;
//        _configuration = configuration;
//    }

//    private string GetUserId()
//    {
//        return User.FindFirstValue(ClaimTypes.NameIdentifier)
//            ?? throw new UnauthorizedAccessException("User ID not found");
//    }

//    /// <summary>
//    /// Generate comprehensive market analysis with ML processing
//    /// </summary>
//    /// <param name="therapeuticArea">Therapeutic area (required) - e.g., "GLP-1 Agonists"</param>
//    /// <param name="product">Specific product name (optional) - e.g., "Ozempic"</param>
//    /// <param name="indication">Disease indication (optional) - e.g., "Type 2 Diabetes"</param>
//    /// <param name="geography">Target geography (required) - e.g., "Global", "US", "EU"</param>
//    /// <param name="researchDepth">Research depth level (required) - "quick", "standard", or "comprehensive"</param>
//    /// <param name="files">Files to upload and analyze (optional) - PDF, Excel, Word documents</param>
//    /// <returns>Complete market analysis report</returns>
//    [HttpPost("generate")]
//    [Consumes("multipart/form-data")]
//    [RequestSizeLimit(100_000_000)] // 100 MB
//    [ProducesResponseType(typeof(GenerateMarketAnalysisResponse), 200)]
//    [ProducesResponseType(400)]
//    [ProducesResponseType(502)]
//    [ProducesResponseType(504)]
//    public async Task<IActionResult> Generate(
//        [FromForm] string therapeuticArea,
//        [FromForm] string? product,
//        [FromForm] string? indication,
//        [FromForm] string geography,
//        [FromForm] string researchDepth,
//        [FromForm] List<IFormFile>? files)
//    {
//        try
//        {
//            _logger.LogInformation("========================================");
//            _logger.LogInformation("NEW GENERATE REQUEST");
//            _logger.LogInformation("========================================");

//            // Log received parameters
//            _logger.LogInformation("RECEIVED PARAMETERS:");
//            _logger.LogInformation("  therapeuticArea: '{Value}'", therapeuticArea ?? "NULL");
//            _logger.LogInformation("  product: '{Value}'", product ?? "NULL");
//            _logger.LogInformation("  indication: '{Value}'", indication ?? "NULL");
//            _logger.LogInformation("  geography: '{Value}'", geography ?? "NULL");
//            _logger.LogInformation("  researchDepth: '{Value}'", researchDepth ?? "NULL");
//            _logger.LogInformation("  files count: {Count}", files?.Count ?? 0);

//            // Validate required fields
//            if (string.IsNullOrWhiteSpace(therapeuticArea))
//            {
//                _logger.LogWarning("Validation failed: therapeuticArea is required");
//                return BadRequest(new
//                {
//                    error = "Validation failed",
//                    field = "therapeuticArea",
//                    message = "Therapeutic area is required and cannot be empty"
//                });
//            }

//            if (string.IsNullOrWhiteSpace(geography))
//            {
//                _logger.LogWarning("Validation failed: geography is required");
//                return BadRequest(new
//                {
//                    error = "Validation failed",
//                    field = "geography",
//                    message = "Geography is required and cannot be empty"
//                });
//            }

//            if (string.IsNullOrWhiteSpace(researchDepth))
//            {
//                _logger.LogWarning("Validation failed: researchDepth is required");
//                return BadRequest(new
//                {
//                    error = "Validation failed",
//                    field = "researchDepth",
//                    message = "Research depth is required and cannot be empty"
//                });
//            }

//            // Validate researchDepth value
//            var validDepths = new[] { "quick", "standard", "comprehensive" };
//            var normalizedDepth = researchDepth.Trim().ToLower();

//            if (!validDepths.Contains(normalizedDepth))
//            {
//                _logger.LogWarning("Invalid researchDepth: '{Value}'", researchDepth);
//                return BadRequest(new
//                {
//                    error = "Validation failed",
//                    field = "researchDepth",
//                    message = "Research depth must be one of: quick, standard, comprehensive",
//                    received = researchDepth,
//                    validValues = validDepths
//                });
//            }

//            _logger.LogInformation("✓ Validation passed");

//            // Get user ID
//            var userId = GetUserId();
//            _logger.LogInformation("User ID: {UserId}", userId);

//            // Upload files
//            var mlApiFiles = new List<MLApiFileDto>();
//            var fileIds = new List<Guid>();

//            if (files != null && files.Any())
//            {
//                var batchId = Guid.NewGuid();
//                _logger.LogInformation("Uploading {Count} files (BatchId: {BatchId})", files.Count, batchId);

//                foreach (var file in files)
//                {
//                    if (file.Length == 0)
//                    {
//                        _logger.LogWarning("Skipping empty file: {FileName}", file.FileName);
//                        continue;
//                    }

//                    _logger.LogInformation("Uploading: {FileName} ({Size} bytes)", file.FileName, file.Length);

//                    var memoryStream = new MemoryStream();
//                    await file.CopyToAsync(memoryStream);
//                    memoryStream.Position = 0;

//                    var uploadResult = await _fileStorageService.UploadFileAsync(
//                        memoryStream,
//                        file.FileName,
//                        file.ContentType
//                    );

//                    memoryStream.Dispose();

//                    _logger.LogInformation("✓ Uploaded: {Url}", uploadResult.PublicUrl);

//                    var uploadedFile = new UploadedFile(
//                        userId: userId,
//                        fileName: file.FileName,
//                        filePath: uploadResult.FilePath,
//                        fileSize: uploadResult.FileSize,
//                        fileExtension: Path.GetExtension(file.FileName),
//                        batchId: batchId
//                    );

//                    await _fileRepository.AddAsync(uploadedFile);
//                    fileIds.Add(uploadedFile.Id);

//                    mlApiFiles.Add(new MLApiFileDto
//                    {
//                        FileId = uploadedFile.Id.ToString(),
//                        FileName = uploadedFile.FileName,
//                        FileUrl = uploadResult.PublicUrl,
//                        FileSize = uploadedFile.FileSize,
//                        FileExtension = uploadedFile.FileExtension
//                    });
//                }

//                _logger.LogInformation("✓ Successfully uploaded {Count} files", mlApiFiles.Count);
//            }
//            else
//            {
//                _logger.LogInformation("No files to upload");
//            }

//            // Prepare ML API request
//            var mlApiRequest = new MLApiRequestDto
//            {
//                TherapeuticArea = therapeuticArea.Trim(),
//                SpecificProduct = product?.Trim(),
//                Indication = indication?.Trim(),
//                TargetGeography = geography.Trim(),
//                ResearchDepth = normalizedDepth,
//                Files = mlApiFiles
//            };

//            _logger.LogInformation("ML API Request prepared:");
//            _logger.LogInformation("  TherapeuticArea: '{Value}'", mlApiRequest.TherapeuticArea);
//            _logger.LogInformation("  SpecificProduct: '{Value}'", mlApiRequest.SpecificProduct ?? "NULL");
//            _logger.LogInformation("  Indication: '{Value}'", mlApiRequest.Indication ?? "NULL");
//            _logger.LogInformation("  TargetGeography: '{Value}'", mlApiRequest.TargetGeography);
//            _logger.LogInformation("  ResearchDepth: '{Value}'", mlApiRequest.ResearchDepth);
//            _logger.LogInformation("  Files: {Count}", mlApiRequest.Files.Count);

//            // Call ML API
//            _logger.LogInformation("Calling ML API...");
//            GenerateMarketAnalysisResponse mlResponse;

//            try
//            {
//                mlResponse = await _mlApiService.GenerateAnalysisAsync(mlApiRequest);
//                _logger.LogInformation("✓ ML API call completed successfully");
//            }
//            catch (TimeoutException timeoutEx)
//            {
//                _logger.LogError(timeoutEx, "ML API timeout");

//                return StatusCode(504, new
//                {
//                    error = "ML service timeout",
//                    details = timeoutEx.Message,
//                    message = "The analysis is taking longer than expected. Please try again with 'quick' research depth.",
//                    suggestion = "Use researchDepth='quick' for faster processing"
//                });
//            }
//            catch (HttpRequestException httpEx)
//            {
//                _logger.LogError(httpEx, "ML API request failed");

//                return StatusCode(502, new
//                {
//                    error = "ML service is unavailable",
//                    details = httpEx.Message,
//                    message = "Please try again later"
//                });
//            }
//            catch (Exception mlEx)
//            {
//                _logger.LogError(mlEx, "Unexpected ML API error");

//                return StatusCode(500, new
//                {
//                    error = "ML processing failed",
//                    details = mlEx.Message,
//                    message = "An error occurred while processing your request"
//                });
//            }

//            // Create Analysis entity
//            _logger.LogInformation("Creating Analysis entity...");

//            var analysis = new Analysis(
//                userId: userId,
//                therapeuticArea: therapeuticArea.Trim(),
//                product: product?.Trim() ?? "General",
//                indication: indication?.Trim() ?? "General",
//                geography: geography.Trim(),
//                researchDepth: normalizedDepth
//            );

//            // Add uploaded files info to response
//            mlResponse.UploadedFiles = mlApiFiles.Select(f => new UploadedFileUrlDto
//            {
//                FileId = f.FileId,
//                FileName = f.FileName,
//                FileUrl = f.FileUrl,
//                FileSize = f.FileSize,
//                FileExtension = f.FileExtension
//            }).ToList();

//            _logger.LogInformation("Added {Count} file URLs to response", mlResponse.UploadedFiles.Count);

//            // Save ML response to database using Newtonsoft.Json
//            var responseJson = JsonConvert.SerializeObject(mlResponse);
//            analysis.SetResponse(responseJson);

//            if (fileIds.Any())
//            {
//                analysis.SetFileIds(fileIds);
//                _logger.LogInformation("Linked {Count} files to analysis", fileIds.Count);
//            }

//            await _analysisRepository.AddAsync(analysis);

//            _logger.LogInformation("========================================");
//            _logger.LogInformation("✓ Analysis saved successfully!");
//            _logger.LogInformation("Analysis ID: {AnalysisId}", analysis.Id);
//            _logger.LogInformation("Confidence Score: {Score}", mlResponse.ConfidenceScore);
//            _logger.LogInformation("========================================");

//            return Ok(mlResponse);
//        }
//        catch (UnauthorizedAccessException authEx)
//        {
//            _logger.LogWarning(authEx, "Unauthorized access");
//            return Unauthorized(new { error = authEx.Message });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error in Generate");
//            return StatusCode(500, new
//            {
//                error = "An unexpected error occurred",
//                details = ex.Message,
//                type = ex.GetType().Name
//            });
//        }
//    }

//    /// <summary>
//    /// Test ML API connection and health
//    /// </summary>
//    [HttpGet("test-ml-api")]
//    [ProducesResponseType(200)]
//    [ProducesResponseType(502)]
//    [ProducesResponseType(503)]
//    public async Task<IActionResult> TestMLApi()
//    {
//        try
//        {
//            _logger.LogInformation("Testing ML API connection...");

//            var baseUrl = _configuration["MLApi:BaseUrl"];
//            var timeout = _configuration["MLApi:TimeoutSeconds"];

//            _logger.LogInformation("Configuration:");
//            _logger.LogInformation("  BaseUrl: {BaseUrl}", baseUrl);
//            _logger.LogInformation("  Timeout: {Timeout}s", timeout);

//            var isHealthy = await _mlApiService.HealthCheckAsync();

//            if (isHealthy)
//            {
//                _logger.LogInformation("✓ ML API is healthy");

//                return Ok(new
//                {
//                    status = "success",
//                    message = "ML API is reachable and healthy",
//                    timestamp = DateTime.UtcNow,
//                    configuration = new
//                    {
//                        baseUrl = baseUrl,
//                        healthEndpoint = $"{baseUrl}/health",
//                        reportEndpoint = $"{baseUrl}/api/v1/report",
//                        timeout = $"{timeout} seconds"
//                    }
//                });
//            }
//            else
//            {
//                _logger.LogWarning("ML API is unhealthy");

//                return StatusCode(503, new
//                {
//                    status = "unhealthy",
//                    message = "ML API is reachable but not healthy",
//                    timestamp = DateTime.UtcNow,
//                    configuration = new
//                    {
//                        baseUrl = baseUrl,
//                        healthEndpoint = $"{baseUrl}/health"
//                    }
//                });
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "ML API test failed");

//            return StatusCode(502, new
//            {
//                status = "error",
//                message = "Cannot reach ML API",
//                error = ex.Message,
//                timestamp = DateTime.UtcNow,
//                configuration = new
//                {
//                    baseUrl = _configuration["MLApi:BaseUrl"]
//                }
//            });
//        }
//    }

//    /// <summary>
//    /// Get all analyses for the current user
//    /// </summary>
//    [HttpGet("GetAll")]
//    [ProducesResponseType(typeof(List<GenerateMarketAnalysisResponse>), 200)]
//    public async Task<IActionResult> GetAll()
//    {
//        try
//        {
//            var userId = GetUserId();
//            _logger.LogInformation("Fetching all analyses for user: {UserId}", userId);

//            var analyses = await _analysisRepository.GetByUserIdAsync(userId);

//            _logger.LogInformation("Found {Count} analyses", analyses.Count);

//            var responses = analyses.Select(a =>
//            {
//                try
//                {
//                    return JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(a.ResponseJson);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Failed to deserialize analysis {AnalysisId}", a.Id);
//                    return null;
//                }
//            })
//            .Where(r => r != null)
//            .ToList();

//            return Ok(responses);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error fetching analyses");
//            return StatusCode(500, new { error = ex.Message });
//        }
//    }

//    /// <summary>
//    /// Get a specific analysis by ID
//    /// </summary>
//    [HttpGet("{id}")]
//    [ProducesResponseType(typeof(GenerateMarketAnalysisResponse), 200)]
//    [ProducesResponseType(404)]
//    [ProducesResponseType(403)]
//    public async Task<IActionResult> GetById(Guid id)
//    {
//        try
//        {
//            var userId = GetUserId();
//            _logger.LogInformation("Fetching analysis {AnalysisId} for user {UserId}", id, userId);

//            var analysis = await _analysisRepository.GetByIdAsync(id);

//            if (analysis == null)
//            {
//                _logger.LogWarning("Analysis not found: {AnalysisId}", id);
//                return NotFound(new
//                {
//                    error = "Analysis not found",
//                    analysisId = id
//                });
//            }

//            if (analysis.UserId != userId)
//            {
//                _logger.LogWarning("User {UserId} attempted to access analysis {AnalysisId} owned by {OwnerId}",
//                    userId, id, analysis.UserId);
//                return Forbid();
//            }

//            var response = JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(analysis.ResponseJson);

//            _logger.LogInformation("✓ Analysis retrieved successfully");

//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error fetching analysis {AnalysisId}", id);
//            return StatusCode(500, new { error = ex.Message });
//        }
//    }

//    /// <summary>
//    /// Delete a specific analysis by ID
//    /// </summary>
//    [HttpDelete("{id}")]
//    [ProducesResponseType(200)]
//    [ProducesResponseType(404)]
//    [ProducesResponseType(403)]
//    public async Task<IActionResult> Delete(Guid id)
//    {
//        try
//        {
//            var userId = GetUserId();
//            _logger.LogInformation("Deleting analysis {AnalysisId} for user {UserId}", id, userId);

//            var analysis = await _analysisRepository.GetByIdAsync(id);

//            if (analysis == null)
//            {
//                _logger.LogWarning("Analysis not found: {AnalysisId}", id);
//                return NotFound(new
//                {
//                    error = "Analysis not found",
//                    analysisId = id
//                });
//            }

//            if (analysis.UserId != userId)
//            {
//                _logger.LogWarning("User {UserId} attempted to delete analysis {AnalysisId} owned by {OwnerId}",
//                    userId, id, analysis.UserId);
//                return Forbid();
//            }

//            await _analysisRepository.DeleteAsync(id);

//            _logger.LogInformation("✓ Analysis {AnalysisId} deleted successfully", id);

//            return Ok(new
//            {
//                message = "Analysis deleted successfully",
//                analysisId = id,
//                timestamp = DateTime.UtcNow
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error deleting analysis {AnalysisId}", id);
//            return StatusCode(500, new { error = ex.Message });
//        }
//    }
//}

using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Security.Claims;
using static ApplicationLayer.Contracts.DTOs.GenerateMarketAnalysisResponse;

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
    private readonly IConfiguration _configuration;

    public MarketAnalysisController(
        IAnalysisRepository analysisRepository,
        IUploadedFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IMLApiService mlApiService,
        ILogger<MarketAnalysisController> logger,
        IConfiguration configuration)
    {
        _analysisRepository = analysisRepository;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _mlApiService = mlApiService;
        _logger = logger;
        _configuration = configuration;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");
    }

    // ============================================================
    // ✅ GENERATE
    // ============================================================

    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(GenerateMarketAnalysisResponse), 200)]
    public async Task<IActionResult> Generate(
        [FromForm] string therapeuticArea,
        [FromForm] string? product,
        [FromForm] string? indication,

        // ✅ Multi Select Enum
        [FromForm] List<TargetGeography> geography,

        // ✅ Multi Select Enum
        [FromForm] List<ResearchDepth> researchDepth,

        [FromForm] List<IFormFile>? files)
    {
        try
        {
            _logger.LogInformation("========== NEW GENERATE REQUEST ==========");

            // ================= VALIDATION =================

            if (string.IsNullOrWhiteSpace(therapeuticArea))
                return BadRequest("TherapeuticArea is required");

            if (geography == null || !geography.Any())
                return BadRequest("At least one Geography must be selected");

            if (researchDepth == null || !researchDepth.Any())
                return BadRequest("At least one ResearchDepth must be selected");

            var userId = GetUserId();

            // ================= FILE UPLOAD =================

            var mlApiFiles = new List<MLApiFileDto>();
            var fileIds = new List<Guid>();

            if (files != null && files.Any())
            {
                var batchId = Guid.NewGuid();

                foreach (var file in files)
                {
                    if (file.Length == 0)
                        continue;

                    _logger.LogInformation("Uploading File: {FileName}", file.FileName);

                    var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var uploadResult = await _fileStorageService.UploadFileAsync(
                        memoryStream,
                        file.FileName,
                        file.ContentType
                    );

                    memoryStream.Dispose();

                    var uploadedFile = new UploadedFile(
                        userId,
                        file.FileName,
                        uploadResult.FilePath,
                        file.Length,
                        Path.GetExtension(file.FileName),
                        batchId
                    );

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

            // ================= CALL ML API =================

            var mlApiRequest = new MLApiRequestDto
            {
                TherapeuticArea = therapeuticArea.Trim(),
                SpecificProduct = product?.Trim(),
                Indication = indication?.Trim(),

                // ✅ Convert Enum List → String
                //TargetGeography = string.Join(",", geography
                //         .Select(g => g.ToString().ToLower())),

                TargetGeography = string.Join(",",
                        geography.Select(g =>
                            g.GetType()
                             .GetField(g.ToString())
                             ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                             .Cast<EnumMemberAttribute>()
                             .FirstOrDefault()?.Value ?? g.ToString()
                        )
                    ),

                //ResearchDepth = string.Join(",", researchDepth
                //.Select(r => r.ToString().ToLower())),

                                ResearchDepth = string.Join(",",
                    researchDepth.Select(r =>
                        r.GetType()
                         .GetField(r.ToString())
                         ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                         .Cast<EnumMemberAttribute>()
                         .FirstOrDefault()?.Value ?? r.ToString()
                    )
                ),


                Files = mlApiFiles
            };

            _logger.LogInformation("Calling ML API...");

            var mlResponse = await _mlApiService.GenerateAnalysisAsync(mlApiRequest);

            // Attach uploaded files to response
            mlResponse.UploadedFiles = mlApiFiles.Select(f => new UploadedFileUrlDto
            {
                FileId = f.FileId,
                FileName = f.FileName,
                FileUrl = f.FileUrl,
                FileSize = f.FileSize,
                FileExtension = f.FileExtension
            }).ToList();

            // ================= SAVE TO DB =================

            var analysis = new Analysis(
                userId,
                therapeuticArea.Trim(),
                product ?? "General",
                indication ?? "General",

                geography,
                researchDepth
            );

            var responseJson = JsonConvert.SerializeObject(mlResponse);
            analysis.SetResponse(responseJson);

            if (fileIds.Any())
                analysis.SetFileIds(fileIds);

            await _analysisRepository.AddAsync(analysis);

            _logger.LogInformation("Analysis saved successfully");

            return Ok(mlResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Generate");

            return StatusCode(500, new
            {
                error = "Internal Server Error",
                details = ex.Message
            });
        }
    }

    // ============================================================
    // ✅ TEST ML API
    // ============================================================

    [HttpGet("test-ml-api")]
    public async Task<IActionResult> TestMLApi()
    {
        try
        {
            var isHealthy = await _mlApiService.HealthCheckAsync();

            if (!isHealthy)
                return StatusCode(503, new { status = "ML API unhealthy" });

            return Ok(new { status = "ML API is healthy" });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { error = ex.Message });
        }
    }

    // ============================================================
    // ✅ GET ALL
    // ============================================================

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var analyses = await _analysisRepository.GetByUserIdAsync(userId);

        var responses = analyses
            .Select(a => JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(a.ResponseJson))
            .Where(r => r != null)
            .ToList();

        return Ok(responses);
    }

    // ============================================================
    // ✅ GET BY ID
    // ============================================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();

        var analysis = await _analysisRepository.GetByIdAsync(id);

        if (analysis == null)
            return NotFound();

        if (analysis.UserId != userId)
            return Forbid();

        var response =
            JsonConvert.DeserializeObject<GenerateMarketAnalysisResponse>(analysis.ResponseJson);

        return Ok(response);
    }

    // ============================================================
    // ✅ DELETE
    // ============================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var analysis = await _analysisRepository.GetByIdAsync(id);

        if (analysis == null)
            return NotFound();

        if (analysis.UserId != userId)
            return Forbid();

        await _analysisRepository.DeleteAsync(id);

        return Ok(new { message = "Deleted successfully" });
    }
}
