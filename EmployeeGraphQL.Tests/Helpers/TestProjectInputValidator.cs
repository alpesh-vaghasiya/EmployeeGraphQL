using Api.GraphQL.Inputs;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// FluentValidation validator for ProjectInput used in InMemory tests.
/// Contains all rules from ProjectInputValidator EXCEPT the Dapper-based
/// LocationId check that requires a real PostgreSQL connection.
/// </summary>
public class TestProjectInputValidator : AbstractValidator<ProjectInput>
{
    public TestProjectInputValidator(AppDbContext db)
    {
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
                    return x.ProjectStartDate.Value.Date <= x.ProjectEndDate.Value.Date;
                return true;
            })
            .WithMessage("Project start date cannot be greater than end date.");

        // Template date range validation (uses EF Core — works with InMemory)
        RuleFor(x => x)
            .MustAsync(async (input, cancellation) =>
            {
                if (!input.ProjectStartDate.HasValue && !input.ProjectEndDate.HasValue)
                    return true;

                var template = await db.Templates
                    .FirstOrDefaultAsync(t => t.TemplateId == input.TemplateId, cancellation);

                if (template == null)
                    return false;

                var templateStart = template.StartDate?.ToDateTime(TimeOnly.MinValue).Date;
                var templateEnd   = template.EndDate?.ToDateTime(TimeOnly.MinValue).Date;

                var projectStart = input.ProjectStartDate?.Date;
                var projectEnd   = input.ProjectEndDate?.Date;

                if (templateStart != null && projectStart != null && projectStart < templateStart)
                    return false;

                if (templateEnd != null && projectEnd != null && projectEnd > templateEnd)
                    return false;

                return true;
            })
            .WithMessage("Project dates must be within template start and end date.");

        // LocationId rule is intentionally omitted here —
        // it requires NpgsqlConnection (Dapper) which is unavailable in InMemory tests.
        // If LocationId validation is needed, wire in the real ProjectInputValidator.
    }
}
