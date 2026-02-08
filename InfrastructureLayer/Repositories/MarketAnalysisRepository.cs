using ApplicationLayer.Interfaces;
using DomainLayer.Entities;
using InfrastructureLayer.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repositories
{
    internal class MarketAnalysisRepository : IMarketAnalysisRepository
    {

        private readonly ApplicationDbContext _context;

        public MarketAnalysisRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MarketAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.MarketAnalyses
                .Include(a => a.MarketForecast)
                .Include(a => a.SWOTAnalysis)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<List<MarketAnalysis>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.MarketAnalyses
                .Include(a => a.MarketForecast)
                .Include(a => a.SWOTAnalysis)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<MarketAnalysis>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MarketAnalyses
                .Include(a => a.MarketForecast)
                .Include(a => a.SWOTAnalysis)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<MarketAnalysis> AddAsync(MarketAnalysis analysis, CancellationToken cancellationToken = default)
        {
            await _context.MarketAnalyses.AddAsync(analysis, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return analysis;
        }

        public async Task UpdateAsync(MarketAnalysis analysis, CancellationToken cancellationToken = default)
        {
            _context.MarketAnalyses.Update(analysis);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var analysis = await GetByIdAsync(id, cancellationToken);
            if (analysis != null)
            {
                _context.MarketAnalyses.Remove(analysis);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
