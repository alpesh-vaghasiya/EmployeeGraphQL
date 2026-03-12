
using EmployeeGraphQL.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;


namespace Api.GraphQL
{
    public class EmployeeValidator : AbstractValidator<EmployeeInput>
    {
        public EmployeeValidator(AppDbContext db)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Employee name is required");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Employee email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department ID must be greater than 0")
                .MustAsync(async (id, cancellation) =>
                {
                    return await db.Departments.AnyAsync(d => d.Id == id, cancellation);
                })
                .WithMessage("Department with the specified ID does not exist");
        }
    }
}