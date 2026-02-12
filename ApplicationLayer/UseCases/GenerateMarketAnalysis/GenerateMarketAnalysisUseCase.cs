using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Contracts.Requests;
using ApplicationLayer.Interfaces;
using ApplicationLayer.Services;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Entity;
using DomainLayer.ValueObjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationLayer.UseCases.GenerateMarketAnalysis
{
    public class GenerateMarketAnalysisUseCase
    {
        private readonly IMarketAnalysisRepository _repository;
        private readonly IMarketForecastProvider _forecastProvider;
        private readonly IReportGenerator _reportGenerator;
        private readonly SWOTAnalyzer _swotAnalyzer;
        private readonly IValidator<GenerateMarketAnalysisRequest> _validator;
        private readonly IMapper _mapper;

        public GenerateMarketAnalysisUseCase(
            IMarketAnalysisRepository repository,
            IMarketForecastProvider forecastProvider,
            IReportGenerator reportGenerator,
            SWOTAnalyzer swotAnalyzer,
            IValidator<GenerateMarketAnalysisRequest> validator,
            IMapper mapper)
        {
            _repository = repository;
            _forecastProvider = forecastProvider;
            _reportGenerator = reportGenerator;
            _swotAnalyzer = swotAnalyzer;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<MarketAnalysisResponseDto> ExecuteAsync(
            GenerateMarketAnalysisRequest request,
            string userId,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // 2. Create MarketAnalysis entity
            var analysis = new MarketAnalysis(
                TherapeuticArea.Create(request.TherapeuticArea),
                request.Product,
                request.Indication,
                Geography.Create(request.Geography),
                userId);

            analysis.SetStatus(AnalysisStatus.InProgress);

            // ======================== التعديل هنا ========================
            // 2.5 ربط الملفات المرفوعة مسبقاً بالتحليل الجديد
            if (request.UploadedFilePaths != null && request.UploadedFilePaths.Any())
            {
                foreach (var filePath in request.UploadedFilePaths)
                {
                    analysis.AddUploadedFile(filePath);
                }
            }
            // =============================================================

            try
            {
                // 3. Generate Market Forecast
                var forecast = await _forecastProvider.GenerateForecastAsync(
                    request.TherapeuticArea,
                    request.Product,
                    request.Indication,
                    request.Geography,
                    cancellationToken);

                analysis.SetMarketForecast(forecast);

                // 4. Generate SWOT Analysis
                var swotAnalysis = _swotAnalyzer.GenerateSWOTAnalysis(
                    request.TherapeuticArea,
                    request.Product,
                    request.Indication,
                    request.Geography);

                analysis.SetSWOTAnalysis(swotAnalysis);

                // 5. Generate Executive Summary
                var executiveSummary = _reportGenerator.GenerateExecutiveSummary(
                    analysis,
                    forecast,
                    swotAnalysis);

                analysis.SetExecutiveSummary(executiveSummary);

                // 6. Mark as completed
                analysis.Complete();

                // 7. Save to repository
                var savedAnalysis = await _repository.AddAsync(analysis, cancellationToken);

                // 8. Map to DTO and return
                return _mapper.Map<MarketAnalysisResponseDto>(savedAnalysis);
            }
            catch (Exception)
            {
                analysis.MarkAsFailed();
                throw;
            }
        }
    }
}