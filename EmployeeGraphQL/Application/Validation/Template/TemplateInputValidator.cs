using FluentValidation;

public class TemplateInputValidator : AbstractValidator<TemplateInput>
{
    public TemplateInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Template title is required.")
            .MinimumLength(3);

        RuleFor(x => x.ProjectTypeId)
            .GreaterThan(0);

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotNull().WithMessage("EndDate is required.")
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate != null);

        // nested json validation
        RuleFor(x => x.ProjectRepeateFrequencyConfig)
            .SetValidator(new ProjectFrequencyValidator()!);
    }
}