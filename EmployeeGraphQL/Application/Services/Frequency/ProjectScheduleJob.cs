using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using EmployeeGraphQL.Infrastructure.Data;

public class ProjectScheduleJob
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer _redis;

    public ProjectScheduleJob(
        AppDbContext context,
        IConnectionMultiplexer redis)
    {
        _context = context;
        _redis = redis;
    }

    public async Task Execute()
    {
        var db = _redis.GetDatabase();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var schedules = await _context.ProjectSchedules
            .Where(x => x.ScheduledDate == today && x.Status == "PENDING")
            .ToListAsync();

        foreach (var schedule in schedules)
        {
            await db.StreamAddAsync(
                "project:scheduler:jobs",
                new NameValueEntry[]
                {
                    new("scheduleId", schedule.ProjectScheduleId),
                    new("templateId", schedule.TemplateId)
                });
        }
    }
}