using DomainLayer.Common;
using DomainLayer.Entity;
using DomainLayer.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Entities
{


    public class MarketAnalysis : BaseEntity
    {
        public TherapeuticArea TherapeuticArea { get; private set; }
        public string Product { get; private set; }
        public string Indication { get; private set; }
        public Geography Geography { get; private set; }
        public string ExecutiveSummary { get; private set; }
        public AnalysisStatus Status { get; private set; }
        public string UserId { get; private set; }

        // ✨ إضافة جديدة: للملفات المرفوعة
        public List<string> UploadedFiles { get; private set; }  // ← جديد!

        public MarketForecast MarketForecast { get; private set; }
        public SWOTAnalysis SWOTAnalysis { get; private set; }

        private MarketAnalysis()
        {
            UploadedFiles = new List<string>();  // ← جديد!
        }

        public MarketAnalysis(
            TherapeuticArea therapeuticArea,
            string product,
            string indication,
            Geography geography,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(product))
                throw new ArgumentException("Product cannot be empty", nameof(product));

            if (string.IsNullOrWhiteSpace(indication))
                throw new ArgumentException("Indication cannot be empty", nameof(indication));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            TherapeuticArea = therapeuticArea ?? throw new ArgumentNullException(nameof(therapeuticArea));
            Product = product;
            Indication = indication;
            Geography = geography ?? throw new ArgumentNullException(nameof(geography));
            UserId = userId;
            Status = AnalysisStatus.Pending;
            ExecutiveSummary = string.Empty;
            UploadedFiles = new List<string>();  // ← جديد!
        }

        // ✨ Method جديدة لإضافة ملف
        public void AddUploadedFile(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                UploadedFiles.Add(filePath);
                SetUpdatedAt();
            }
        }

        public void SetMarketForecast(MarketForecast forecast)
        {
            MarketForecast = forecast ?? throw new ArgumentNullException(nameof(forecast));
            forecast.SetMarketAnalysisId(Id);
            SetUpdatedAt();
        }

        public void SetSWOTAnalysis(SWOTAnalysis swotAnalysis)
        {
            SWOTAnalysis = swotAnalysis ?? throw new ArgumentNullException(nameof(swotAnalysis));
            swotAnalysis.SetMarketAnalysisId(Id);
            SetUpdatedAt();
        }

        public void SetExecutiveSummary(string summary)
        {
            ExecutiveSummary = summary ?? string.Empty;
            SetUpdatedAt();
        }

        public void SetStatus(AnalysisStatus status)
        {
            Status = status;
            SetUpdatedAt();
        }

        public void Complete()
        {
            Status = AnalysisStatus.Completed;
            SetUpdatedAt();
        }

        public void MarkAsFailed()
        {
            Status = AnalysisStatus.Failed;
            SetUpdatedAt();
        }
    }
}
