

using Api.GraphQL;

[ExtendObjectType(typeof(Query))]
public class GSSQuery
{
    public async Task<IEnumerable<SurveyResponse>> GetSurveys(
            List<int> ids,
            [Service] IGssService gssApi)
    {
        var endpoint = "surveys/departments";

        var queryParams = new Dictionary<string, object>
        {
            { "ids", ids }
        };

        var result = await gssApi.GetAsync<GssResponse<SurveyResponse>>(endpoint, queryParams);
        return result.Data;
    }
}
