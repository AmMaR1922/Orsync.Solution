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
    public async Task<IActionResult> UploadFile( IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        var filePath = await _fileUploadService.UploadFileAsync(file);

        return Ok(new
        {
            message = "File uploaded successfully",
            filePath,
            fileName = file.FileName,
            fileSize = file.Length
        });
    }

    [HttpPost("upload-multiple")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMultipleFiles( List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files provided" });

        var filePaths = await _fileUploadService.UploadMultipleFilesAsync(files);

        return Ok(new
        {
            message = $"{files.Count} files uploaded successfully",
            files = files.Select((f, i) => new
            {
                fileName = f.FileName,
                filePath = filePaths[i],
                fileSize = f.Length
            })
        });
    }

    [HttpPost("upload-to-analysis/{analysisId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadToAnalysis(Guid analysisId,  IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        var userId = GetUserId();
        var analysis = await _repository.GetByIdAsync(analysisId);

        if (analysis == null)
            return NotFound(new { error = "Analysis not found" });

        if (analysis.UserId != userId)
            return Forbid();

        var filePath = await _fileUploadService.UploadFileAsync(file);
        analysis.AddUploadedFile(filePath);
        await _repository.UpdateAsync(analysis);

        return Ok(new
        {
            message = "File uploaded and attached to analysis successfully",
            analysisId,
            filePath,
            fileName = file.FileName
        });
    }
}
