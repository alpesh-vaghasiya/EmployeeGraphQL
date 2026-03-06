public class PersonService
{
    private readonly IMisApiService _mis;

    public PersonService(IMisApiService mis)
    {
        _mis = mis;
    }

    public async Task<List<PositionViewModel>> GetPersonPosition(int[] personIds)
    {
        var parameters = new Dictionary<string, object>();

        foreach (var id in personIds)
            parameters.Add("personId", id);

        var response = await _mis.GetAsync<ListResponse<PositionViewModel>>(
            "Person/Position",
            parameters
        );

        return response.Data;   // unwrap the list
    }
}