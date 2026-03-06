using BenchmarkDotNet.Attributes;
using System.Text;
using System.Text.Json;

[MemoryDiagnoser]
public class EmployeeQueryBenchmark
{
    private HttpClient _httpClient;

    [GlobalSetup]
    public void Setup()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5004");
    }

    // 1️⃣ Tracking without Pagination
    [Benchmark]
    public async Task Tracking_NoPagination()
    {
        var query = new
        {
            query = "{ departments { id name } }"
        };

        var json = JsonSerializer.Serialize(query);

        var response = await _httpClient.PostAsync(
            "/graphql",
            new StringContent(json, Encoding.UTF8, "application/json"));

        await response.Content.ReadAsStringAsync();
    }

    // 2️⃣ NoTracking without Pagination
    [Benchmark]
    public async Task NoTracking_NoPagination()
    {
        var query = new
        {
            query = "{ departmentsNoTracking { id name } }"
        };

        var json = JsonSerializer.Serialize(query);

        var response = await _httpClient.PostAsync(
            "/graphql",
            new StringContent(json, Encoding.UTF8, "application/json"));

        await response.Content.ReadAsStringAsync();
    }

    // 3️⃣ Tracking with Pagination
    [Benchmark]
    public async Task Tracking_WithPagination()
    {
        var query = new
        {
            query = "{ departments(first:50) { nodes { id name } } }"
        };

        var json = JsonSerializer.Serialize(query);

        var response = await _httpClient.PostAsync(
            "/graphql",
            new StringContent(json, Encoding.UTF8, "application/json"));

        await response.Content.ReadAsStringAsync();
    }

    // 4️⃣ NoTracking with Pagination
    [Benchmark]
    public async Task NoTracking_WithPagination()
    {
        var query = new
        {
            query = "{ departmentsNoTracking(first:50) { nodes { id name } } }"
        };

        var json = JsonSerializer.Serialize(query);

        var response = await _httpClient.PostAsync(
            "/graphql",
            new StringContent(json, Encoding.UTF8, "application/json"));

        await response.Content.ReadAsStringAsync();
    }
}