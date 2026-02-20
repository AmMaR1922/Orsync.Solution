using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IUploadedFileRepository
    {
        Task<UploadedFile> AddAsync(UploadedFile file);
        Task<List<UploadedFile>> AddRangeAsync(List<UploadedFile> files);
        Task<UploadedFile?> GetByIdAsync(Guid id);
    }
}
