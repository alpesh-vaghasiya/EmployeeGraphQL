namespace Api.GraphQL.Inputs;

public record UpdateEmployeeInput(
    int Id,
    string? Name,
    string? Email,
    decimal? Salary,
    int? DepartmentId,
    List<long>? ProjectIds
);