using ApplicationLayer.Contracts.DTOs;
using ApplicationLayer.Contracts.Requests;
using ApplicationLayer.Interfaces;
using ApplicationLayer.UseCases.GenerateMarketAnalysis;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Orsync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MarketAnalysisController : ControllerBase
    {
        private readonly GenerateMarketAnalysisUseCase _generateUseCase;
        private readonly IMarketAnalysisRepository _repository;

        private readonly IMapper _mapper;

        public MarketAnalysisController(
            GenerateMarketAnalysisUseCase generateUseCase,
            IMarketAnalysisRepository repository,

            IMapper mapper)
        {
            _generateUseCase = generateUseCase;
            _repository = repository;

            _mapper = mapper;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found");
        }

        [HttpPost("generate")]
        public async Task<ActionResult<MarketAnalysisResponseDto>> GenerateAnalysis(
            [FromBody] GenerateMarketAnalysisRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserId();
                var result = await _generateUseCase.ExecuteAsync(request, userId, cancellationToken);
                return Ok(result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the analysis", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MarketAnalysisResponseDto>> GetAnalysis(
            Guid id,
            CancellationToken cancellationToken)
        {
            var analysis = await _repository.GetByIdAsync(id, cancellationToken);

            if (analysis == null)
                return NotFound(new { message = "Analysis not found" });

            var userId = GetUserId();
            if (analysis.UserId != userId)
                return Forbid();

            var result = _mapper.Map<MarketAnalysisResponseDto>(analysis);
            return Ok(result);
        }


        [HttpGet("GetAll")]
        public async Task<ActionResult<List<MarketAnalysisResponseDto>>> GetMyAnalyses(
            CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var analyses = await _repository.GetByUserIdAsync(userId, cancellationToken);
            var result = _mapper.Map<List<MarketAnalysisResponseDto>>(analyses);
            return Ok(result);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnalysis(Guid id, CancellationToken cancellationToken)
        {
            var analysis = await _repository.GetByIdAsync(id, cancellationToken);

            if (analysis == null)
                return NotFound(new { message = "Analysis not found" });

            var userId = GetUserId();
            if (analysis.UserId != userId)
                return Forbid();

            await _repository.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
    }

}
