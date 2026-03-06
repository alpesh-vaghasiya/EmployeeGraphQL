using EmployeeGraphQL.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Net.Http;

namespace EmployeeGraphQL.Application.Services;

public class DepartmentImportWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DepartmentImportWorker> _logger;
    private readonly IHttpClientFactory _httpFactory;

    public DepartmentImportWorker(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<DepartmentImportWorker> logger,
        IHttpClientFactory httpFactory)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _httpFactory = httpFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Worker] Department import worker started...");

        var db = _redis.GetDatabase();
        string stream = "department:import:jobs";
        string group = "department-group";
        string consumer = Environment.MachineName;

        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, group, "$");
        }
        catch
        {
            _logger.LogInformation("[Worker] Consumer group already exists");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await db.StreamReadGroupAsync(
                stream, group, consumer, ">", count: 10);

            if (messages.Length == 0)
            {
                await Task.Delay(1000);
                continue;
            }

            foreach (var msg in messages)
            {
                string jobId = msg.Values.First(v => v.Name == "jobId").Value;
                string fileUrl = msg.Values.First(v => v.Name == "filePath").Value;

                _logger.LogInformation($"[Worker] Processing JobId={jobId}, FileURL={fileUrl}");

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    var http = _httpFactory.CreateClient();

                    // 1️⃣ DOWNLOAD CSV FROM URL
                    var csvText = await http.GetStringAsync(fileUrl);

                    // 2️⃣ READ CSV
                    using var reader = new StringReader(csvText);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true
                    };
                    using var csv = new CsvReader(reader, config);

                    var rows = csv.GetRecords<dynamic>().ToList();

                    // 3️⃣ INSERT INTO DB
                    foreach (var row in rows)
                    {
                        string deptName = row.Name; // 👈 your CSV column
                        if (!string.IsNullOrWhiteSpace(deptName))
                        {
                            context.Departments.Add(new EmployeeGraphQL.Domain.Entities.Department
                            {
                                Name = deptName
                            });
                        }
                    }

                    await context.SaveChangesAsync();

                    _logger.LogInformation($"[Worker] JobId {jobId} Insert Complete");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Worker] Error processing job {jobId}");
                }

                await db.StreamAcknowledgeAsync(stream, group, msg.Id);
            }
        }
    }
}