using Api.GraphQL.Inputs;
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

public class DepartmentInputValidator : AbstractValidator<DepartmentInput>
{
    public DepartmentInputValidator(AppDbContext db)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Department name is required");

    }
}