namespace EmployeeGraphQL.GraphQL.Errors;

public class ValidationErrorItem
{
    public string Field { get; set; } = default!;
    public string Message { get; set; } = default!;
    public object? AttemptedValue { get; set; }
}