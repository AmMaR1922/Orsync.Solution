using ApplicationLayer.Interfaces;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace  Orsync.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
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


}
