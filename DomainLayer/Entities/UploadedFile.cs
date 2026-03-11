using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{

    public class UploadedFile : BaseEntity
    {
        public string UserId { get; private set; } = string.Empty;
        public string FileName { get; private set; } = string.Empty;
        public string FilePath { get; private set; } = string.Empty;
        public long FileSize { get; private set; }
        public string FileExtension { get; private set; } = string.Empty;
        public Guid BatchId { get; private set; }

        private UploadedFile() { }

        public UploadedFile(
            string userId,
            string fileName,
            string filePath,
            long fileSize,
            string fileExtension,
            Guid batchId)
        {
            UserId = userId;
            FileName = fileName;
            FilePath = filePath;
            FileSize = fileSize;
            FileExtension = fileExtension;
            BatchId = batchId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}