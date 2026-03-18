

using Api.GraphQL;

[ExtendObjectType(typeof(Query))]
public class MISQuery
{

    public async Task<IEnumerable<PersonSearch>> GetPersonSearch(
   PersonSearchInput input,
   [Service] IHttpClientFactory factory,
   [Service] IMisApiService misApi,
   [Service] IConfiguration config)
    {
        var endpoint = "Person/Search";
        var queryParams = new Dictionary<string, object>();

        if (input.PersonSearchID.HasValue)
            queryParams["personSearchID"] = input.PersonSearchID.Value;

        if (input.FamilyID.HasValue)
            queryParams["familyID"] = input.FamilyID.Value;

        if (!string.IsNullOrWhiteSpace(input.FirstName))
            queryParams["firstName"] = Uri.EscapeDataString(input.FirstName);

        if (!string.IsNullOrWhiteSpace(input.LastName))
            queryParams["lastName"] = Uri.EscapeDataString(input.LastName);

        if (!string.IsNullOrWhiteSpace(input.Phone))
            queryParams["phone"] = Uri.EscapeDataString(input.Phone);

        if (!string.IsNullOrWhiteSpace(input.Email))
            queryParams["email"] = Uri.EscapeDataString(input.Email);

        if (!string.IsNullOrWhiteSpace(input.Address))
            queryParams["address"] = Uri.EscapeDataString(input.Address);

        if (!string.IsNullOrWhiteSpace(input.City))
            queryParams["city"] = Uri.EscapeDataString(input.City);

        if (!string.IsNullOrWhiteSpace(input.PostalCode))
            queryParams["postalCode"] = Uri.EscapeDataString(input.PostalCode);

        if (!string.IsNullOrWhiteSpace(input.CenterName))
            queryParams["centerName"] = Uri.EscapeDataString(input.CenterName);

        if (!string.IsNullOrWhiteSpace(input.BAPSID))
            queryParams["BAPSID"] = Uri.EscapeDataString(input.BAPSID);

        return await misApi.GetAsync<IEnumerable<PersonSearch>>(endpoint, queryParams);
    }

    public async Task<IEnumerable<DeptMandal>> GetDeptMandal(
     int divId,
     [Service] IMisApiService misApi)
    {
        var endpoint = "Global/DeptMandal";

        var queryParams = new Dictionary<string, object>
    {
        { "divId", divId }
    };

        return await misApi.GetAsync<IEnumerable<DeptMandal>>(endpoint, queryParams);
    }
    public async Task<IEnumerable<DeptSCategory>> GetDeptSCategory(
    int divId,
    [Service] IMisApiService misApi)
    {
        var endpoint = "Global/DeptSCategory";

        var queryParams = new Dictionary<string, object>
    {
        { "divId", divId }
    };

        return await misApi.GetAsync<IEnumerable<DeptSCategory>>(endpoint, queryParams);
    }
}
