using StackExchange.Redis;

public class RedisStreamKaryakarProducer
{
    private readonly IDatabase _db;
    public RedisStreamKaryakarProducer(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    // 1️⃣ Publish to VALIDATE stream
    public async Task PublishValidateJobAsync(
        string jobId,
        string projectId,
        string importType,
        string fileUrl,
        string createdBy)
    {
        await _db.StreamAddAsync(
            "sampark:jobs:import-validate",
            new NameValueEntry[]
            {
                new("jobId", jobId),
                new("projectId", projectId),
                new("importType", importType),
                new("fileUrl", fileUrl),
                new("createdBy", createdBy)
            });
    }

    // 2️⃣ Publish to EXECUTE stream
    public async Task PublishExecuteJobAsync(
        string jobId,
        string projectId,
        string importType,
        int validCount,
        string createdBy)
    {
        await _db.StreamAddAsync(
            "sampark:jobs:import-execute",
            new NameValueEntry[]
            {
                new("jobId", jobId),
                new("projectId", projectId),
                new("importType", importType),
                new("validRecordCount", validCount.ToString()),
                new("createdBy", createdBy)
            });
    }
}