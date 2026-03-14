using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using InfrastructureLayer.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Data.Repositories;

public class AnalysisRepository : IAnalysisRepository
{
    private readonly ApplicationDbContext _context;

    public AnalysisRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Analysis> AddAsync(Analysis analysis)
    {
        await _context.Analyses.AddAsync(analysis);
        await _context.SaveChangesAsync();
        return analysis;
    }

    public async Task<Analysis?> GetByIdAsync(Guid id)
    {
        return await _context.Analyses.FindAsync(id);
    }

    public async Task<List<Analysis>> GetByUserIdAsync(string userId)
    {
        return await _context.Analyses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }


    public async Task<List<Analysis>> GetAllAsync()
    {
        return await _context.Analyses
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(Analysis analysis)
    {
        _context.Analyses.Update(analysis);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var analysis = await _context.Analyses.FindAsync(id);
        if (analysis != null)
        {
            _context.Analyses.Remove(analysis);
            await _context.SaveChangesAsync();
        }
    }
}