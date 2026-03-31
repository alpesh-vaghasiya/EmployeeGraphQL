using Api.GraphQL.Auth;
using EmployeeGraphQL.Api.GraphQL.Filters;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Api.GraphQL;

[ExtendObjectType(typeof(Query))]
public partial class TemplateQuery
{
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [ModuleAuthorize(new[] { SystemModuleCode.EventGroup }, new[] { ModuleAction.View })]
    public IQueryable<Template> GetTemplates(List<int>? locationScopeIds, [Service] AppDbContext context, [Service] IConfiguration config)
    {
        var query = context.Templates.AsNoTracking()
        .FilterByEntityScopeAsync(10, config).GetAwaiter().GetResult()
        .FilterByLocationScope(locationScopeIds);
        return query;
    }

    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<Template> GetTemplateById(long id, [Service] AppDbContext context)
        => context.Templates.AsNoTracking().Where(t => t.TemplateId == id);

    [UsePaging(IncludeTotalCount = true, MaxPageSize = 50, DefaultPageSize = 20)]
    [UseFiltering]
    [UseSorting]
    [ModuleAuthorize(new[] { SystemModuleCode.EventGroup }, new[] { ModuleAction.View })]
    public IQueryable<TemplateListView> GetTemplateList(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor http,
        [Service] ILogger<TemplateQuery> logger)
    {
        var query = context.TemplateListView.AsNoTracking();

        var depRaw = http.HttpContext?.Items["departmentId"]?.ToString();

        if (string.IsNullOrWhiteSpace(depRaw))
        {
            logger.LogWarning("departmentId header missing — returning full template list.");
            return query;
        }

        // Support multi-select: "1,3,5"
        var departmentIds = depRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        logger.LogInformation("Filtered TemplateListView by departments: {Departments}", string.Join(",", departmentIds));

        return query.Where(t => t.Departments.Any(d => departmentIds.Contains(d)));
    }
}