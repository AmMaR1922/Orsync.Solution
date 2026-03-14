using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IAnalysisRepository
    {
        Task<Analysis> AddAsync(Analysis analysis);
        Task<Analysis?> GetByIdAsync(Guid id);
        Task<List<Analysis>> GetByUserIdAsync(string userId);
        Task<List<Analysis>> GetAllAsync();
        Task UpdateAsync(Analysis analysis);
        Task DeleteAsync(Guid id);
    }
}
