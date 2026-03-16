using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using EmployeeGraphQL.Infrastructure.Data;
using EmployeeGraphQL.Domain.Entities;
using System.Text.Json;

public class ProjectSchedulerWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProjectSchedulerWorker> _logger;

    public ProjectSchedulerWorker(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<ProjectSchedulerWorker> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase();

        string stream = "project:scheduler:jobs";
        string group = "project-group";
        string consumer = Environment.MachineName;

        try
        {
            await db.StreamCreateConsumerGroupAsync(stream, group, "$");
        }
        catch
        {
            _logger.LogInformation("Consumer group already exists");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await db.StreamReadGroupAsync(
                stream,
                group,
                consumer,
                ">",
                count: 10);

            if (messages.Length == 0)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            foreach (var msg in messages)
            {
                try
                {
                    int scheduleId = int.Parse(
                        msg.Values.First(x => x.Name == "scheduleId").Value);

                    int templateId = int.Parse(
                        msg.Values.First(x => x.Name == "templateId").Value);

                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var schedule = await context.ProjectSchedules
                        .FirstAsync(x => x.ProjectScheduleId == scheduleId);

                    var template = await context.Templates.FirstAsync(x => x.TemplateId == templateId);
                    if (template == null)
                    {
                        _logger.LogWarning("Template {TemplateId} not found", templateId);
                        continue;
                    }

                    // JSONB → List<int>
                    var locations = string.IsNullOrWhiteSpace(template.LocationScopeIds) ? new List<int>() : JsonSerializer.Deserialize<List<int>>(template.LocationScopeIds) ?? new List<int>();

                    var startDate = DateTime.SpecifyKind(template.StartDate!.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

                    var existingProjects = await context.Projects.Where(p => p.TemplateId == template.TemplateId && p.ProjectStartDate == startDate)
                        .Select(p => p.LocationId)
                        .ToListAsync();

                    var endDate = DateTime.SpecifyKind(template.EndDate!.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

                    var existingSet = existingProjects.ToHashSet();

                    var projectList = new List<Project>();

                    foreach (var locationId in locations)
                    {
                        var loc = locationId.ToString();

                        if (existingSet.Contains(loc))
                            continue;

                        projectList.Add(new Project
                        {
                            ProjectUucode = Guid.NewGuid(),
                            TemplateId = template.TemplateId,
                            Title = template.Title,
                            Description = template.Description,
                            Status = "CREATED",
                            LocationId = loc,
                            ProjectStartDate = startDate,
                            ProjectEndDate = endDate,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    context.Projects.AddRange(projectList);

                    schedule.Status = "COMPLETED";

                    await context.SaveChangesAsync();

                    await db.StreamAcknowledgeAsync(stream, group, msg.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker error");
                }
            }
        }
    }
}