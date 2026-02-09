//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;


//namespace InfrastructureLayer.Services;

//public interface IFileUploadService
//{
//    Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
//    Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, CancellationToken cancellationToken = default);
//    Task DeleteFileAsync(string filePath);
//}

//public class FileUploadService : IFileUploadService
//{
//    private readonly string _uploadPath;
//    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
//    private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx", ".txt", ".csv", ".png", ".jpg", ".jpeg" };

//    public FileUploadService(IWebHostEnvironment environment)
//    {
//        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");

//        if (!Directory.Exists(_uploadPath))
//        {
//            Directory.CreateDirectory(_uploadPath);
//        }
//    }

//    public async Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
//    {
//        if (file == null || file.Length == 0)
//            throw new ArgumentException("File is empty");

//        if (file.Length > _maxFileSize)
//            throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)} MB");

//        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
//        if (!_allowedExtensions.Contains(extension))
//            throw new ArgumentException($"File type {extension} is not allowed");

//        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
//        var filePath = Path.Combine(_uploadPath, uniqueFileName);

//        using (var stream = new FileStream(filePath, FileMode.Create))
//        {
//            await file.CopyToAsync(stream, cancellationToken);
//        }

//        return $"/uploads/{uniqueFileName}";
//    }

//    public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, CancellationToken cancellationToken = default)
//    {
//        var uploadedPaths = new List<string>();

//        foreach (var file in files)
//        {
//            var path = await UploadFileAsync(file, cancellationToken);
//            uploadedPaths.Add(path);
//        }

//        return uploadedPaths;
//    }

//    public Task DeleteFileAsync(string filePath)
//    {
//        if (string.IsNullOrWhiteSpace(filePath))
//            return Task.CompletedTask;

//        var fullPath = Path.Combine(_uploadPath, Path.GetFileName(filePath));

//        if (File.Exists(fullPath))
//        {
//            File.Delete(fullPath);
//        }

//        return Task.CompletedTask;
//    }
//}



using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace InfrastructureLayer.Services;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath);
}

public class FileUploadService : IFileUploadService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
    private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".xlsx", ".txt", ".csv", ".png", ".jpg", ".jpeg" };

    public FileUploadService(IWebHostEnvironment environment)
    {

        // ✨ استخدام ContentRootPath بدلاً من WebRootPath
        var basePath = environment.ContentRootPath;
        _uploadPath = Path.Combine(basePath, "Uploads"); // مجلد خارج wwwroot

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
        // ✨ التعامل مع حالة WebRootPath = null
        var webRootPath = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            // إذا WebRootPath فارغ، استخدم ContentRootPath بدلاً منه
            webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        _uploadPath = Path.Combine(webRootPath, "uploads");

        // إنشاء المجلد إذا لم يكن موجود
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        if (file.Length > _maxFileSize)
            throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)} MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException($"File type {extension} is not allowed");

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"/uploads/{uniqueFileName}";
    }

    public async Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, CancellationToken cancellationToken = default)
    {
        var uploadedPaths = new List<string>();

        foreach (var file in files)
        {
            var path = await UploadFileAsync(file, cancellationToken);
            uploadedPaths.Add(path);
        }

        return uploadedPaths;
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.CompletedTask;

        var fileName = Path.GetFileName(filePath);
        var fullPath = Path.Combine(_uploadPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}