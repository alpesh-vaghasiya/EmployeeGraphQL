using Microsoft.EntityFrameworkCore;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;
using Npgsql;
using Dapper;

namespace EmployeeGraphQL.Api.GraphQL.Filters
{
    public static class TemplateFilterExtensions
    {
        public static IQueryable<Template> FilterByLocationScope(this IQueryable<Template> query, List<int>? locationScopeIds)
        {
            if (locationScopeIds == null || !locationScopeIds.Any())
                return query;

            var jsonValues = locationScopeIds
                .Select(id => $"[{id}]")
                .ToList();

            return query.Where(t =>
                jsonValues.Any(val =>
                    EF.Functions.JsonContains(
                        t.LocationScopeIds!,
                        val
                    )
                )
            );
        }

        public static async Task<IQueryable<Template>> FilterByEntityScopeAsync(this IQueryable<Template> query, int? entityId, IConfiguration config)
        {
            if (entityId == null)
                return query;

            using var connection = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));

            var entityIds = (await connection.QueryAsync<int>(ProjectQueries.GetEntityWithChildren, new { EntityId = entityId })).ToList();

            if (!entityIds.Any())
                return query;

            var jsonValues = entityIds
                .Select(id => $"[{id}]")
                .ToList();

            return query.Where(t =>
                jsonValues.Any(val =>
                    EF.Functions.JsonContains(
                        t.LocationScopeIds!,
                        val
                    )
                )
            );
        }
    }
}