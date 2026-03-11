using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task DeleteFileAsync(string filePath);
    }

    public class FileUploadResult
    {
        public string FilePath { get; set; } = string.Empty;   // /uploads/file.ext
        public string PublicUrl { get; set; } = string.Empty;  // https://domain/uploads/file.ext
        public long FileSize { get; set; }
    }
}
