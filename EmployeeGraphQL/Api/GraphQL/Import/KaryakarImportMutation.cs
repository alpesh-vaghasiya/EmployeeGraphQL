using Api.GraphQL;

[ExtendObjectType(typeof(Mutation))]
public class KaryakarImportMutation
{
    public async Task<string> ImportKaryakarCsv(
        string fileUrl,
        [Service] ImportJobService jobService,
        [Service] RedisStreamKaryakarProducer producer)
    {
        // 1️⃣ Create job entry in DB
        var job = await jobService.CreateJob(fileUrl, "101");

        // 2️⃣ Push message to Redis validate stream
        await producer.PublishValidateJobAsync(
            job.ImportJobId,
            "101",
            "KARYAKAR",
            fileUrl,
            "SYSTEM"
        );

        return $"JOB QUEUED: {job.ImportJobId}";
    }
}