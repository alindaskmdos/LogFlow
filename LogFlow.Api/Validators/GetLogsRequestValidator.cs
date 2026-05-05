using FluentValidation;
using LogFlow.Api.Contracts;

namespace LogFlow.Api.Validators;

public class GetLogsRequestValidator : AbstractValidator<GetLogsRequest>
{
    public GetLogsRequestValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty().WithMessage("From is required");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("To is required")
            .GreaterThanOrEqualTo(x => x.From).WithMessage("'To' must be greater than or equal to 'From'");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 1000).WithMessage("Limit must be between 1 and 1000");
    }
}