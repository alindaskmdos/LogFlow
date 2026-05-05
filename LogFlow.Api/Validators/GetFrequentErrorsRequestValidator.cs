using FluentValidation;
using LogFlow.Api.Contracts;

namespace LogFlow.Api.Validators;

public class GetFrequentErrorsRequestValidator : AbstractValidator<GetFrequentErrorsRequest>
{
    public GetFrequentErrorsRequestValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty().WithMessage("From is required");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("To is required")
            .GreaterThanOrEqualTo(x => x.From).WithMessage("'To' must be greater than or equal to 'From'");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100");
    }
}