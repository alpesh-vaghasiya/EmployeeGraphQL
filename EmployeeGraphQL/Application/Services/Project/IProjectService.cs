using Api.GraphQL.Inputs;
using EmployeeGraphQL.Domain.Entities;

public interface IProjectService
{
    Task<Project> CreateProject(ProjectInput input, CancellationToken cancellationToken);
    Task<Project> UpdateProject(int id, ProjectInput input, CancellationToken cancellationToken);
    Task<bool> DeleteProject(int id, CancellationToken cancellationToken);
}