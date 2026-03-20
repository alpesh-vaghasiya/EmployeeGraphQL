using Api.GraphQL.Inputs;
using EmployeeGraphQL.Domain.Entities;

public interface IProjectService
{
    Task<PagedResult<ProjectResponse>> Projects(long departmentId, QueryOptions options);
    Task<Project> CreateProject(ProjectInput input, CancellationToken cancellationToken);
    Task<Project> UpdateProject(long id, ProjectInput input, CancellationToken cancellationToken);
    Task<bool> DeleteProject(long id, CancellationToken cancellationToken);
    Task<Project> PublishProject(long id, CancellationToken cancellationToken);
}