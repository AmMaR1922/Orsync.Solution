using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces.Infrastructure
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<List<string>> UploadMultipleFilesAsync(List<IFormFile> files, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string filePath);
    }
}
