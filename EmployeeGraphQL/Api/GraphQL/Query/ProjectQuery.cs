using Api.GraphQL;
using EmployeeGraphQL.Infrastructure.Data;

[ExtendObjectType(typeof(Query))]
public class ProjectQuery
{
    [GraphQLName("projects")]
    public async Task<PagedResult<ProjectResponse>> Projects(
        QueryOptions options,
        [Service] IProjectService projectService,
        [Service] IHttpContextAccessor http)
    {
        var depRaw = http.HttpContext?.Items["departmentId"]?.ToString();

        if (string.IsNullOrWhiteSpace(depRaw))
            throw new GraphQLException("departmentId header is required.");

        if (!long.TryParse(depRaw, out var departmentId))
            throw new GraphQLException("Invalid departmentId header value.");

        return await projectService.Projects(departmentId, options);
    }
}