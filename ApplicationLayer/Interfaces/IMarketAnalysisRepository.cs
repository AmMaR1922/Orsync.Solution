using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IMarketAnalysisRepository
    {
        Task<MarketAnalysis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<MarketAnalysis>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<MarketAnalysis>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MarketAnalysis> AddAsync(MarketAnalysis analysis, CancellationToken cancellationToken = default);
        Task UpdateAsync(MarketAnalysis analysis, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

}
