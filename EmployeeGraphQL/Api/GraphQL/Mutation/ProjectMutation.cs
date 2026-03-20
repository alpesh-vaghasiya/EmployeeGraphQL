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
            long id,
            ProjectInput input,
            [Service] IProjectService service,
            CancellationToken cancellationToken)
        {
            return await service.UpdateProject(id, input, cancellationToken);
        }

        public async Task<bool> DeleteProject(
            long id,
            [Service] IProjectService service,
            CancellationToken cancellationToken)
        {
            return await service.DeleteProject(id, cancellationToken);
        }
        public async Task<Project> PublishProject(
        long id,
        [Service] IProjectService service,
        CancellationToken cancellationToken)
        {
            return await service.PublishProject(id, cancellationToken);
        }
    }
}