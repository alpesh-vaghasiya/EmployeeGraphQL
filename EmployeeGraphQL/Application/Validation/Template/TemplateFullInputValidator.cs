using FluentValidation;

public class TemplateFullInputValidator : AbstractValidator<TemplateFullInput>
{
    public TemplateFullInputValidator()
    {
        RuleFor(x => x.Template.Title)
            .NotEmpty().WithMessage("Template title is required.")
            .MinimumLength(3);

        RuleFor(x => x.Template.ProjectTypeId)
            .GreaterThan(0);

        RuleFor(x => x.Template.StartDate)
            .NotNull().WithMessage("StartDate is required.");

        RuleFor(x => x.Template.EndDate)
            .NotNull().WithMessage("EndDate is required.")
            .GreaterThan(x => x.Template.StartDate)
            .When(x => x.Template.StartDate != null);
    }
}