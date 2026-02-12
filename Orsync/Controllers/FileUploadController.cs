using ApplicationLayer.Interfaces;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace  Orsync.Controllers;

[ApiController]
[Route("api/[controller]")]
 [Authorize]
public class FileUploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IMarketAnalysisRepository _repository;

    public FileUploadController(
        IFileUploadService fileUploadService,
        IMarketAnalysisRepository repository)
    {
        _fileUploadService = fileUploadService;
        _repository = repository;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");
    }

   



    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFiles([FromForm] IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files provided" });

        var uploadedPaths = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var path = await _fileUploadService.UploadFileAsync(file);
                uploadedPaths.Add(path);
            }
        }

        return Ok(new
        {
            message = "Files uploaded successfully",
            count = uploadedPaths.Count,
            files = uploadedPaths
        });
    }


 
    [HttpPost("upload-to-analysis/{analysisId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadToAnalysis(
    Guid analysisId,
    [FromForm] IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files provided" });

        var userId = GetUserId();
        var analysis = await _repository.GetByIdAsync(analysisId);

        if (analysis == null)
            return NotFound(new { error = "Analysis not found" });

        if (analysis.UserId != userId)
            return Forbid();

        var uploadedFiles = new List<object>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var filePath = await _fileUploadService.UploadFileAsync(file);
            analysis.AddUploadedFile(filePath);

            uploadedFiles.Add(new
            {
                fileName = file.FileName,
                filePath
            });
        }

        await _repository.UpdateAsync(analysis);

        return Ok(new
        {
            message = "Files uploaded successfully",
            analysisId,
            count = uploadedFiles.Count,
            files = uploadedFiles
        });
    }


    [HttpGet("my-files")]
    public async Task<IActionResult> GetUserFiles()
    {
        var userId = GetUserId();

        // 1. جلب كل التحليلات الخاصة بالمستخدم
        var analyses = await _repository.GetAllByUserIdAsync(userId);

        if (analyses == null || !analyses.Any())
            return Ok(new { message = "No files found for this user", files = new List<object>() });

        // 2. تجميع كل الملفات من جميع التحليلات في قائمة واحدة
        // نفترض أن اسم الحقل داخل الموديل هو UploadedFiles
        var allFiles = analyses
            .Where(a => a.UploadedFiles != null) // التأكد من وجود ملفات
            .SelectMany(a => a.UploadedFiles, (analysis, filePath) => new
            {
                AnalysisId = analysis.Id,
                // اختياري: لو حابب تعرض عنوان التحليل
                FilePath = filePath,
                FileName = Path.GetFileName(filePath) // استخراج اسم الملف من المسار
            })
            .ToList();

        return Ok(new
        {
            count = allFiles.Count,
            files = allFiles
        });
    }

}
