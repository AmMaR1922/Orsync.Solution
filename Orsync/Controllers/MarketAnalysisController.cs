using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
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
    private readonly ILogger<MarketAnalysisController> _logger;
    private readonly IConfiguration _configuration;
    
    public MarketAnalysisController(
        IAnalysisRepository analysisRepository,
        IUploadedFileRepository fileRepository,
        IFileStorageService fileStorageService,
        ILogger<MarketAnalysisController> logger,
        IConfiguration configuration)
    {
        _analysisRepository = analysisRepository;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _configuration = configuration;
    }
    
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");
    }
    
    /// <summary>
    /// ✨ THE MAIN ENDPOINT - توليد تحليل سوقي مع رفع ملفات
    /// </summary>
    [HttpPost("generate")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Generate(
        [FromForm] string therapeuticArea,
        [FromForm] string product,
        [FromForm] string indication,
        [FromForm] string geography,
        [FromForm] List<IFormFile>? files)
    {
        try
        {
            var userId = GetUserId();
            
            _logger.LogInformation(
                "Generate analysis request from user {UserId} for {Product}",
                userId, product);
            
            // ✅ 1. Upload files and generate URLs
            var uploadedFileUrls = new List<UploadedFileUrlDto>();
            var fileIds = new List<Guid>();
            
            if (files != null && files.Any())
            {
                var batchId = Guid.NewGuid();
                var baseUrl = _configuration["FileStorage:BaseUrl"] ?? Request.Scheme + "://" + Request.Host;
                
                _logger.LogInformation("Uploading {Count} files", files.Count);
                
                foreach (var file in files)
                {
                    if (file.Length == 0) continue;
                    
                    // Upload to storage
                    var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    
                    var uploadResult = await _fileStorageService.UploadFileAsync(
                        memoryStream,
                        file.FileName,
                        file.ContentType
                    );
                    
                    memoryStream.Dispose();
                    
                    // Save metadata to DB
                    var uploadedFile = new UploadedFile(
                        userId: userId,
                        fileName: file.FileName,
                        filePath: uploadResult.FilePath,
                        fileSize: uploadResult.FileSize,
                        fileExtension: Path.GetExtension(file.FileName),
                        batchId: batchId
                    );
                    
                    await _fileRepository.AddAsync(uploadedFile);
                    fileIds.Add(uploadedFile.Id);
                    
                    // ✨ Add to URLs list for ML Engineer
                    uploadedFileUrls.Add(new UploadedFileUrlDto
                    {
                        FileId = uploadedFile.Id.ToString(),
                        FileName = uploadedFile.FileName,
                        FileUrl = uploadResult.PublicUrl,  // ✨ URL للـ ML Engineer
                        FileSize = uploadedFile.FileSize,
                        FileExtension = uploadedFile.FileExtension
                    });
                }
            }
            
            // ✅ 2. Generate comprehensive response
            var response = GenerateComprehensiveResponse(therapeuticArea, product, indication, geography);
            
            // ✅ 3. Create Analysis entity
            var analysis = new Analysis(
                userId: userId,
                therapeuticArea: therapeuticArea,
                product: product,
                indication: indication,
                geography: geography,
                researchDepth: "deep_dive"
            );
            
            response.Id = analysis.Id.ToString();

            // ✅ 4. Add uploaded files info to response
            //response.UploadedFiles = uploadedFileUrls;  // ✨ الملفات مع الـ URLs
            response.UploadedFiles = uploadedFileUrls
              .Select(f => new UploadedFileUrlDto
              {
                  FileId = f.FileId,
                  FileName = f.FileName,
                  FileUrl = f.FileUrl,
                  FileSize = f.FileSize,
                  FileExtension = f.FileExtension
              }).ToList();
            var responseJson = JsonSerializer.Serialize(response);
            analysis.SetResponse(responseJson);
            
            if (fileIds.Any())
            {
                analysis.SetFileIds(fileIds);
            }
            
            await _analysisRepository.AddAsync(analysis);
            
            _logger.LogInformation(
                "Analysis created: {AnalysisId} with {FileCount} files",
                analysis.Id, uploadedFileUrls.Count);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analysis");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// الحصول على كل التحاليل
    /// </summary>
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = GetUserId();
            var analyses = await _analysisRepository.GetByUserIdAsync(userId);
            
            var responses = analyses.Select(a =>
            {
                var response = JsonSerializer.Deserialize<GenerateMarketAnalysisResponse>(a.ResponseJson);
                return response;
            }).ToList();
            
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analyses");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// الحصول على تحليل محدد
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _analysisRepository.GetByIdAsync(id);
            
            if (analysis == null)
                return NotFound(new { error = "Analysis not found" });
            
            if (analysis.UserId != userId)
                return Forbid();
            
            var response = JsonSerializer.Deserialize<GenerateMarketAnalysisResponse>(analysis.ResponseJson);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analysis");
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    // ========== Helper Method ==========
    
    private GenerateMarketAnalysisResponse GenerateComprehensiveResponse(
        string therapeuticArea,
        string product,
        string indication,
        string geography)
    {
        return new GenerateMarketAnalysisResponse
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Input = new InputDataDto
            {
                Topic = therapeuticArea,
                Product = product,
                Indication = indication,
                Geography = geography,
                Depth = "deep_dive"
            },
            ConfidenceScore = 0.92,
            GeneratedBy = new List<string>
            {
                "Gemini AI Agent",
                "Evidence Linker",
                "PubMed",
                "ClinicalTrials.gov"
            },
            ExecutiveSummary = $"The {therapeuticArea} market for {product} represents one of the most dynamic and rapidly expanding segments in pharmaceutical history, driven by breakthrough efficacy in both diabetes management and weight loss. The market is projected to reach $100+ billion by 2030, with a CAGR of 25-30%.\n\n**Key Market Drivers:** {product} has revolutionized treatment paradigms, demonstrating unprecedented outcomes alongside cardiovascular benefits.\n\n**Strategic Outlook:** The market is experiencing supply constraints due to overwhelming demand, creating significant opportunities for new entrants. Payer coverage is expanding rapidly despite high pricing.\n\n**Critical Success Factors:** Manufacturing scale-up, differentiated clinical profiles, and innovative delivery mechanisms will determine market winners.",
            
            MarketOverview = new MarketOverviewDto
            {
                MarketSize = "$24.8 billion (2024)",
                Cagr = "28.5%",
                ForecastYear = "2030",
                KeyTrends = new List<string>
                {
                    "Explosive demand for weight loss indications driving 300%+ prescription growth",
                    "Supply chain constraints limiting market penetration despite unprecedented demand"
                }
            },
            
            FinancialForecast = new FinancialForecastDto
            {
                Cagr = "28.5%",
                RevenueProjections = new List<RevenueProjectionDto>
                {
                    new RevenueProjectionDto { Year = "2024", Amount = "$24.8 billion" },
                    new RevenueProjectionDto { Year = "2025", Amount = "$35.2 billion" },
                    new RevenueProjectionDto { Year = "2030", Amount = "$110+ billion" }
                },
                KeyDrivers = new List<string>
                {
                    "Obesity indication approval expanding addressable market"
                }
            },
            
            Competitors = new List<CompetitorDto>
            {
                new CompetitorDto
                {
                    Name = "Novo Nordisk",
                    Product = "Ozempic/Wegovy",
                    Mechanism = "GLP-1 receptor agonist",
                    Status = "Market Leader - 45% market share"
                }
            },
            
            ScientificEvidence = new List<ScientificEvidenceDto>
            {
                new ScientificEvidenceDto
                {
                    Title = "Semaglutide and cardiovascular outcomes",
                    Source = "PubMed (2023)",
                    Url = "https://pubmed.ncbi.nlm.nih.gov/37622680/",
                    Summary = "SELECT trial: 20% reduction in MACE"
                }
            },
            
            AIInsights = new AIInsightsDto
            {
                Swot = new SWOTDto
                {
                    Strengths = new List<string> { "Unprecedented clinical efficacy" },
                    Weaknesses = new List<string> { "Supply constraints" },
                    Opportunities = new List<string> { "Obesity market expansion" },
                    Threats = new List<string> { "Payer restrictions" }
                },
                Regulatory = new RegulatoryDto
                {
                    ApprovalPathways = new List<string> { "FDA: Approved for T2D (2017)" },
                    KeyRegulations = new List<string> { "FDA REMS program" },
                    UpcomingChanges = new List<string> { "Medicare expansion" }
                },
                Reimbursement = new ReimbursementDto
                {
                    PayerLandscape = new List<string> { "Commercial insurance: 80%+ coverage" },
                    PricingModels = new List<string> { "List price: $1,000-1,500/month" },
                    AccessBarriers = new List<string> { "Prior authorization required" }
                },
                StrategicRecommendations = new List<string>
                {
                    "Manufacturing Scale-Up"
                }
            },
            
            Risks = new List<string>
            {
                "Supply Chain Disruption"
            },
            
            Triangulation = new TriangulationDto
            {
                Score = 0.92,
                Methodology = "Strategic Triangulation",
                Points = new List<TriangulationPointDto>
                {
                    new TriangulationPointDto
                    {
                        Claim = "Market will exceed $100B by 2030",
                        Sources = new List<string> { "Industry Reports" },
                        Confidence = 0.95,
                        Status = "verified"
                    }
                }
            }
        };
    }
}