using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class SsoService : ISsoService
{
    private readonly HttpClient _httpClient;
    private readonly SsoModel _settings;
    private readonly ILogger<SsoService> _logger;

    public SsoService(HttpClient httpClient, IOptions<SsoModel> settings, ILogger<SsoService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> ValidateToken(string token)
{
    var request = new HttpRequestMessage(
        HttpMethod.Post,
        _settings.Url + "user/validate/token"
    );

    request.Headers.Add("client-id", _settings.ClientId);
    request.Headers.Add("Authorization", token); // FIX
    request.Headers.Add("User-Agent", "YourApp");

    var response = await _httpClient.SendAsync(request);

    if (response.StatusCode == HttpStatusCode.OK)
    {
        return true;
    }
    else
    {
        var dataToPrint = new
        {
            Message = "SSO validation failed.",
            ReturnResponseFromSSO = response,
            Request = request
        };

        var ConvertToString = JsonConvert.SerializeObject(
            dataToPrint,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        _logger.LogInformation(ConvertToString);

        return false;
    }
}
}