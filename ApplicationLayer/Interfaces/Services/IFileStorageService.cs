using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public string FilePath { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
