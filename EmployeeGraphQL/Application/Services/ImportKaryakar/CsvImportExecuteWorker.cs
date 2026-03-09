using System.Text.Json;
using EmployeeGraphQL.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

public class CsvImportExecuteWorker : BackgroundService
{
    private readonly ILogger<CsvImportExecuteWorker> _logger;
    private readonly IConnectionMultiplexer? _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    public CsvImportExecuteWorker(
        ILogger<CsvImportExecuteWorker> logger,
        IConnectionMultiplexer? redis,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_redis == null)
        {
            _logger.LogWarning("[Worker] Redis is disabled. CsvImportExecuteWorker will not run.");
            return;
        }

        var redisDb = _redis?.GetDatabase();
        const string stream = "sampark:jobs:import-execute";
        const string group = "sampark-execute-group";

        try { await redisDb?.StreamCreateConsumerGroupAsync(stream, group, "0", true); }
        catch { }

        string consumer = $"exec-{Guid.NewGuid()}";

        _logger.LogInformation("🚀 Execute Worker Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            // 1️⃣ First read pending messages
            var messages = await redisDb.StreamReadGroupAsync(
                stream, group, consumer, "0", 100);

            if (messages.Length == 0)
            {
                // 2️⃣ Then read new messages
                messages = await redisDb.StreamReadGroupAsync(
                    stream, group, consumer, ">", 100);
            }

            if (messages.Length == 0)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            foreach (var msg in messages)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    await ProcessMessage(msg, db);

                    await redisDb.StreamAcknowledgeAsync(stream, group, msg.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR in ExecuteWorker");
                }
            }
        }
    }

    private async Task ProcessMessage(StreamEntry msg, AppDbContext db)
    {
        string jobId = msg["jobId"];
        string projectId = msg["projectId"];
        string createdBy = msg["createdBy"];

        var job = await db.ImportJobs.FindAsync(jobId);
        if (job == null) return;

        job.Status = "IMPORTING";
        await db.SaveChangesAsync();

        var records = await db.ImportRecords.Where(x => x.ImportJobId == jobId && x.IsValid == true).ToListAsync();
        int imported = 0;

        var karyakars = new List<ProjectKaryakar>();

        foreach (var record in records)
        {
            var data = JsonSerializer.Deserialize<JsonElement>(record.RecordData);
            if (!data.TryGetProperty("MisBapsId", out var misProp))
                continue;

            string misId = misProp.GetString() ?? "";
            if (!long.TryParse(misId, out long pid))
                continue;

            karyakars.Add(new ProjectKaryakar
            {
                ProjectKaryakarUucode = Guid.NewGuid(),
                ProjectId = long.Parse(projectId),
                KaryakarPersonId = pid,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            record.IsImported = true;
            record.ImportedAt = DateTime.UtcNow;

            imported++;
        }

        db.ProjectKaryakars.AddRange(karyakars);

        job.Status = "COMPLETED";
        job.ImportedRecords = imported;
        job.CompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }
}