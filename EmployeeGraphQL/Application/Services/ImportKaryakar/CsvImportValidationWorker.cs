using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using EmployeeGraphQL.Infrastructure.Data;
using StackExchange.Redis;

public class CsvImportValidationWorker : BackgroundService
{
    private readonly ILogger<CsvImportValidationWorker> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;

    public CsvImportValidationWorker(
        ILogger<CsvImportValidationWorker> logger,
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redisDb = _redis.GetDatabase();
        const string stream = "sampark:jobs:import-validate";
        const string group = "sampark-validate-group";

        try { await redisDb.StreamCreateConsumerGroupAsync(stream, group, "0", true); }
        catch { }

        string consumer = $"validate-{Guid.NewGuid()}";

        _logger.LogInformation("🚀 Validation Worker Started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await redisDb.StreamReadGroupAsync(
                stream, group, consumer, ">", 1);

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
                    var producer = scope.ServiceProvider.GetRequiredService<RedisStreamKaryakarProducer>();

                    await ProcessMessage(msg, db, producer);

                    await redisDb.StreamAcknowledgeAsync(stream, group, msg.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ ERROR in ValidationWorker");
                }
            }
        }
    }

    private async Task ProcessMessage(
        StreamEntry msg,
        AppDbContext db,
        RedisStreamKaryakarProducer producer)
    {
        string jobId = msg["jobId"];
        string projectId = msg["projectId"];
        string importType = msg["importType"];
        string fileUrl = msg["fileUrl"];
        string createdBy = msg["createdBy"];

        var job = await db.ImportJobs.FindAsync(jobId);
        if (job == null) return;

        job.Status = "VALIDATING";
        job.StartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Download CSV
        using var http = new HttpClient();
        string csvText = await http.GetStringAsync(fileUrl);

        using var reader = new StringReader(csvText);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        });

        var rows = csv.GetRecords<KaryakarCsvRow>().ToList();

        int valid = 0;
        int invalid = 0;
        int rowNum = 1;

        var validator = new KaryakarValidationService();

        foreach (var row in rows)
        {
            var validation = validator.Validate(row);

            db.ImportRecords.Add(new ImportRecord
            {
                ImportRecordUuCode = Guid.NewGuid(),
                ImportJobId = jobId,
                RowNumber = rowNum++,
                RecordData = JsonSerializer.Serialize(row),
                IsValid = validation.IsValid,
                ValidationErrors = validation.Error,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            if (validation.IsValid) valid++;
            else invalid++;
        }

        await db.SaveChangesAsync();

        job.TotalRecords = rows.Count;
        job.ValidRecords = valid;
        job.InvalidRecords = invalid;
        await db.SaveChangesAsync();

        await producer.PublishExecuteJobAsync(jobId, projectId, importType, valid, createdBy);
    }
}