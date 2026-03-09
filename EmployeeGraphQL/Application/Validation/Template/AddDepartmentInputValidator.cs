using Api.GraphQL.Inputs;
using FluentValidation;

public class AddDepartmentInputValidator : AbstractValidator<DepartmentInput>
{
    public AddDepartmentInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Department name is required");
    }
}