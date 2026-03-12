using Api.GraphQL.Inputs;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public class ProjectInputValidator : AbstractValidator<ProjectInput>
{
    private readonly AppDbContext _db;

    public ProjectInputValidator(AppDbContext db)
    {
        _db = db;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required.")
            .MaximumLength(150)
            .WithMessage("Project name cannot exceed 150 characters.");

        RuleFor(x => x.TemplateId)
            .GreaterThan(0)
            .WithMessage("TemplateId is required.");

        // Start date <= End date
        RuleFor(x => x)
            .Must(x =>
            {
                if (x.ProjectStartDate.HasValue && x.ProjectEndDate.HasValue)
                {
                    return x.ProjectStartDate.Value.Date <= x.ProjectEndDate.Value.Date;
                }
                return true;
            })
            .WithMessage("Project start date cannot be greater than end date.");

        // Template date range validation
        RuleFor(x => x)
            .MustAsync(async (input, cancellation) =>
            {
                if (!input.ProjectStartDate.HasValue && !input.ProjectEndDate.HasValue)
                    return true;

                var template = await _db.Templates
                    .FirstOrDefaultAsync(t => t.TemplateId == input.TemplateId, cancellation);

                if (template == null)
                    return false;

                var templateStart = template.StartDate?.ToDateTime(TimeOnly.MinValue).Date;
                var templateEnd = template.EndDate?.ToDateTime(TimeOnly.MinValue).Date;

                var projectStart = input.ProjectStartDate?.Date;
                var projectEnd = input.ProjectEndDate?.Date;

                if (templateStart != null && projectStart != null && projectStart < templateStart)
                    return false;

                if (templateEnd != null && projectEnd != null && projectEnd > templateEnd)
                    return false;

                return true;
            })
            .WithMessage("Project dates must be within template start and end date.");
    }
}