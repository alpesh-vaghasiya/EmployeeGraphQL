namespace Api.GraphQL.Inputs;

public record AddEmployeeInput(
    string Name,
    string Email,
    decimal Salary,
    int DepartmentId,
    List<int>? ProjectIds

);