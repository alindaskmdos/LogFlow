using FluentValidation;
using LogFlow.Api.Contracts;

namespace LogFlow.Api.Validators;

public class GetActivityGraphRequestValidator : AbstractValidator<GetActivityGraphRequest>
{
    public GetActivityGraphRequestValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty().WithMessage("From is required");

        RuleFor(x => x.To)
            .NotEmpty().WithMessage("To is required")
            .GreaterThanOrEqualTo(x => x.From).WithMessage("'To' must be greater than or equal to 'From'");

        RuleFor(x => x.Interval)
            .GreaterThan(TimeSpan.Zero).WithMessage("Interval must be greater than zero");
    }
}