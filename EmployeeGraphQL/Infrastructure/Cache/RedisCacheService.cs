using System.Text.Json;
using StackExchange.Redis;

public class RedisCacheService
{
    private readonly IConnectionMultiplexer? _redis;

    public RedisCacheService(IConnectionMultiplexer? redis)
    {
        _redis = redis;
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_redis == null)
            return null;

        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan expiry)
    {
        if (_redis == null)
            return;

        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }
    public async Task StoreValidationAsync(string token, List<SyncRowValidationResult> results)
    {
        var db = _redis.GetDatabase();
        var key = $"karyakar:validation:{token}";

        var json = JsonSerializer.Serialize(results);

        await db.StringSetAsync(key, json, TimeSpan.FromMinutes(15));
    }

    public async Task<List<SyncRowValidationResult>> GetValidationAsync(string token)
    {
        var db = _redis.GetDatabase();
        var key = $"karyakar:validation:{token}";

        var json = await db.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<List<SyncRowValidationResult>>(json);
    }
}