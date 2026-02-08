using ApplicationLayer.Contracts.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.UseCases.GenerateMarketAnalysis
{
    public class GenerateMarketAnalysisValidator : AbstractValidator<GenerateMarketAnalysisRequest>
    {
        public GenerateMarketAnalysisValidator()
        {
            RuleFor(x => x.TherapeuticArea)
                .NotEmpty().WithMessage("Therapeutic area is required")
                .MaximumLength(200).WithMessage("Therapeutic area must not exceed 200 characters");

            RuleFor(x => x.Product)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

            RuleFor(x => x.Indication)
                .NotEmpty().WithMessage("Indication is required")
                .MaximumLength(300).WithMessage("Indication must not exceed 300 characters");

            RuleFor(x => x.Geography)
                .NotEmpty().WithMessage("Geography is required")
                .MaximumLength(100).WithMessage("Geography must not exceed 100 characters");
        }
    }

}
