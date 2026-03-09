using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;

public class RedisStreamProducer
{
    private readonly StackExchange.Redis.IDatabase _db;
    public RedisStreamProducer(IConnectionMultiplexer? redis)
    {
        _db = redis?.GetDatabase();
    }

    public async Task PublishJobAsync(string jobId, string filePath)
    {
        if (_db == null) return;
        await _db.StreamAddAsync("department:import:jobs",
            new NameValueEntry[]
            {
            new NameValueEntry("jobId", jobId),
            new NameValueEntry("filePath", filePath)
            });
    }
}