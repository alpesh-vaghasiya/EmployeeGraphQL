using System.Text.Json;

public class GssService : IGssService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public GssService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, object> queryParams)
    {
        var baseUrl = _config["GssApi:BaseUrl"];

        var query = string.Join("&",
            queryParams.SelectMany(kvp =>
                kvp.Value is IEnumerable<int> list
                    ? list.Select(v => $"{kvp.Key}={v}")
                    : new[] { $"{kvp.Key}={kvp.Value}" }
            )
        );

        var url = $"{baseUrl}/{endpoint}?{query}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // 🔐 Headers
        request.Headers.Add("x-gss-app-id", _config["GssApi:AppId"]);
        request.Headers.Add("x-gss-app-secret", _config["GssApi:AppSecret"]);

        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}