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
}