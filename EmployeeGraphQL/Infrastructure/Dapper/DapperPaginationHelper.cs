using Dapper;
using System.Data;

public static class DapperPaginationHelper
{
    public static async Task<PagedResult<T>> QueryPagedAsync<T>(
        this IDbConnection connection,
        string sql,
        object parameters,
        int page,
        int pageSize)
    {
        using var multi = await connection.QueryMultipleAsync(sql, parameters);

        var totalCount = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<T>()).ToList();

        return new PagedResult<T>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }
}