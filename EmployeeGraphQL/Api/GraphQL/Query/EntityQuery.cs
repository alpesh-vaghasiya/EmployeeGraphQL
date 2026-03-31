using Api.GraphQL;
using Dapper;
using Npgsql;

[ExtendObjectType(typeof(Query))]
public class EntityQuery
{
    [GraphQLName("entityWithChildren")]
    public async Task<IEnumerable<int>> GetEntityWithChildren(
        int entityId,
        [Service] IConfiguration config)
    {
        using var connection = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));

        IEnumerable<int> entityIds = new List<int>();

        entityIds = await connection.QueryAsync<int>(
             ProjectQueries.GetEntityWithChildren,
             new { EntityId = entityId });

        return await connection.QueryAsync<int>(
            ProjectQueries.GetEntityWithChildren,
            new { EntityId = entityId });
    }
}
