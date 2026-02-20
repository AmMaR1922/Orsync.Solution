using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace InfrastructureLayer.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        _uploadPath = Path.Combine(webRootPath, "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }

        _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "https://localhost:7001";
    }

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        using (var fileStreamOut = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOut, cancellationToken);
        }

        var fileInfo = new FileInfo(filePath);
        var publicUrl = $"{_baseUrl}/uploads/{uniqueFileName}";

        return new FileUploadResult
        {
            FilePath = $"/uploads/{uniqueFileName}",
            PublicUrl = publicUrl,
            FileSize = fileInfo.Length
        };
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, Path.GetFileName(filePath));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}