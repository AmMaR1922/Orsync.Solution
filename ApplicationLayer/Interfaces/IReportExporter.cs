using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{

    public interface IReportExporter
    {
        Task<byte[]> ExportToPdfAsync(MarketAnalysis analysis, CancellationToken cancellationToken = default);
        Task<byte[]> ExportToExcelAsync(MarketAnalysis analysis, CancellationToken cancellationToken = default);
    }

}
