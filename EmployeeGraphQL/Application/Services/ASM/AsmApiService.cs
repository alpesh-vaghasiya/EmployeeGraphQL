using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class AsmApiService : IAsmApiService
{
    private readonly HttpClient _client;
    private readonly ASMModel _asm;
    private readonly ILogger<AsmApiService> _logger;

    public AsmApiService(
        IHttpClientFactory factory,
        IOptions<ASMModel> asm,
        ILogger<AsmApiService> logger)
    {
        _client = factory.CreateClient();
        _asm = asm.Value;
        _logger = logger;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
    {
        var url = _asm.Url + endpoint;
        var json = JsonConvert.SerializeObject(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("User-Agent", "ALM/dev");

        _logger.LogInformation("ASM POST → {Url}", url);

        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsync(url, content);
        stopwatch.Stop();

        _logger.LogInformation("ASM POST completed in {ms}ms", stopwatch.ElapsedMilliseconds);

        string responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("ASM Error {Code}: {Body}", response.StatusCode, responseString);
            throw new Exception($"ASM error {response.StatusCode}: {responseString}");
        }

        if (string.IsNullOrWhiteSpace(responseString))
            throw new Exception("ASM returned empty response");

        return JsonConvert.DeserializeObject<TResponse>(responseString);
    }
}