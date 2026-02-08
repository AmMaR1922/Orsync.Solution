using ApplicationLayer.Contracts.DTOs;
using AutoMapper;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApplicationLayer.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // MarketForecast -> MarketForecastDto
            CreateMap<MarketForecast, MarketForecastDto>()
                .ForMember(dest => dest.Confidence, opt => opt.MapFrom(src => src.Confidence.ToString()));

            // SWOTAnalysis -> SWOTAnalysisDto
            CreateMap<SWOTAnalysis, SWOTAnalysisDto>();

            // MarketAnalysis -> MarketAnalysisResponseDto
            CreateMap<MarketAnalysis, MarketAnalysisResponseDto>()
                .ForMember(dest => dest.TherapeuticArea, opt => opt.MapFrom(src => src.TherapeuticArea.ToString()))
                .ForMember(dest => dest.Geography, opt => opt.MapFrom(src => src.Geography.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }

}
