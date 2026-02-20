using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using InfrastructureLayer.Data.Context;

namespace InfrastructureLayer.Data.Repositories;

public class UploadedFileRepository : IUploadedFileRepository
{
    private readonly ApplicationDbContext _context;

    public UploadedFileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UploadedFile> AddAsync(UploadedFile file)
    {
        await _context.UploadedFiles.AddAsync(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task<List<UploadedFile>> AddRangeAsync(List<UploadedFile> files)
    {
        await _context.UploadedFiles.AddRangeAsync(files);
        await _context.SaveChangesAsync();
        return files;
    }

    public async Task<UploadedFile?> GetByIdAsync(Guid id)
    {
        return await _context.UploadedFiles.FindAsync(id);
    }
}