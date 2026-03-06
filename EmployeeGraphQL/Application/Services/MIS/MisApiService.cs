using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class MisApiService : IMisApiService
{
    private readonly HttpClient _client;
    private readonly MisModel _mis;
    private readonly ILogger<MisApiService> _logger;

    public MisApiService(
        IHttpClientFactory factory,
        IOptions<MisModel> mis,
        ILogger<MisApiService> logger)
    {
        _client = factory.CreateClient();
        _mis = mis.Value;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, object> queryParams = null)
    {
        var url = _mis.Url + endpoint;

        if (queryParams?.Count > 0)
        {
            var qp = string.Join("&", queryParams.Select(x =>
                $"{x.Key}={x.Value}"
            ));
            url += "?" + qp;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-baps-auth-app-id", _mis.AppId);
        request.Headers.Add("x-baps-auth-app-secret", _mis.AppSecret);
        request.Headers.Add("User-Agent", "ALM/dev");

        var watch = Stopwatch.StartNew();
        var response = await _client.SendAsync(request);
        watch.Stop();

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(
                "MIS GET {Url} took {ms}ms",
                url, watch.ElapsedMilliseconds
            );
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        throw new Exception($"MIS error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
    }
}