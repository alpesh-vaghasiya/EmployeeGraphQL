using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
public class GraphQLHelper
{
    private readonly HttpClient _client;
    private readonly string _token;
    private readonly string _position;

    public GraphQLHelper(HttpClient client)
    {
        _client = client;

        var config = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.Test.json", optional: false)
      .Build();

        _token = config["Auth:Token"];
        _position = config["Auth:Position"];
    }

    public async Task<HttpResponseMessage> Execute(string query)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/graphql");

        request.Content = JsonContent.Create(new { query });

        request.Headers.Add("Authorization", $"Bearer {_token}");
        request.Headers.Add("X-App-Position", _position);
        request.Headers.Add("X-App-Event", "1");
        request.Headers.Add("departmentId", "100");

        return await _client.SendAsync(request);
    }
}