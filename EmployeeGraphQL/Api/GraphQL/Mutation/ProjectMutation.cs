using Api.GraphQL.Inputs;
using EmployeeGraphQL.Domain.Entities;

namespace Api.GraphQL
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class ProjectMutation
    {
        public async Task<Project> CreateProject(
            ProjectInput input,
            [Service] IProjectService service,
            CancellationToken cancellationToken)
        {
            return await service.CreateProject(input, cancellationToken);
        }

        public async Task<Project> UpdateProject(
            int id,
            ProjectInput input,
            [Service] IProjectService service,
            CancellationToken cancellationToken)
        {
            return await service.UpdateProject(id, input, cancellationToken);
        }

        public async Task<bool> DeleteProject(
            int id,
            [Service] IProjectService service,
            CancellationToken cancellationToken)
        {
            return await service.DeleteProject(id, cancellationToken);
        }
    }
}