using Api.GraphQL.Auth;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using HotChocolate.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Api.GraphQL;

[ExtendObjectType(typeof(Query))]
public partial class TemplateQuery
{
    // Templates
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    // [UsePaging]
    public IQueryable<Template> GetTemplates([Service] AppDbContext context)
        => context.Templates;

    [UseProjection]
    public IQueryable<Template> GetTemplateById(long id, [Service] AppDbContext context)
        => context.Templates.Where(t => t.TemplateId == id);


    [UsePaging(IncludeTotalCount = true)]
    [UseFiltering]
    [UseSorting]
    [ModuleAuthorize(new[] { SystemModuleCode.EventGroup }, new[] { ModuleAction.View })]
    public IQueryable<TemplateListView> GetTemplateList([Service] AppDbContext context, [Service] IHttpContextAccessor http,
    [Service] ILogger<TemplateQuery> logger)
    {
        var query = context.TemplateListView.AsQueryable();
        // 1. Safely read header
        var depRaw = http.HttpContext?.Items["departmentId"]?.ToString();

        // 2. No department → allow full list (or deny based on your rule)
        if (string.IsNullOrWhiteSpace(depRaw))
        {
            logger.LogWarning("departmentId header missing — returning full template list.");
            return query;
        }

        // 3. Support multi-select: "1,3,5"
        var departmentIds = depRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        // 4. Apply array filtering with ContainsAny
        query = query.Where(t => t.Departments.Any(d => departmentIds.Contains(d)));

        logger.LogInformation("Filtered TemplateListView by departments: {Departments}", string.Join(",", departmentIds)
        );
        return query;
    }
}