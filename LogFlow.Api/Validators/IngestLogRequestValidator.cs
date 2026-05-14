using FluentValidation;
using LogFlow.Api.Contracts;

namespace LogFlow.Api.Validators;

public class IngestLogRequestValidator : AbstractValidator<IngestLogRequest>
{
    public IngestLogRequestValidator()
    {
        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Log level is required")
            .MaximumLength(20).WithMessage("Log level cannot exceed 20 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(5000).WithMessage("Message cannot exceed 5000 characters");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required");
    }
}