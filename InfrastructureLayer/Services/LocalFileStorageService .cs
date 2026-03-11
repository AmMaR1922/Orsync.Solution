using ApplicationLayer.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace InfrastructureLayer.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;

        // 🔹 تحديد wwwroot
        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        // 🔹 فولدر uploads
        _uploadPath = Path.Combine(webRootPath, "uploads");

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }

        // 🔹 الدومين الحقيقي من appsettings
        _baseUrl = configuration["FileStorage:BaseUrl"]
                   ?? throw new Exception("FileStorage:BaseUrl is not configured.");
    }

    public async Task<FileUploadResult> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var physicalPath = Path.Combine(_uploadPath, uniqueFileName);

        using (var fileStreamOut = new FileStream(physicalPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOut, cancellationToken);
        }

        var fileInfo = new FileInfo(physicalPath);

        var relativePath = $"/uploads/{uniqueFileName}";
        var publicUrl = $"{_baseUrl}{relativePath}";

        return new FileUploadResult
        {
            FilePath = relativePath,   // نخزن النسبي مش الفيزكال
            PublicUrl = publicUrl,
            FileSize = fileInfo.Length
        };
    }

    public Task DeleteFileAsync(string filePath)
    {
        // filePath جاي بصيغة /uploads/filename.ext
        var fileName = Path.GetFileName(filePath);
        var physicalPath = Path.Combine(_uploadPath, fileName);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }
}
