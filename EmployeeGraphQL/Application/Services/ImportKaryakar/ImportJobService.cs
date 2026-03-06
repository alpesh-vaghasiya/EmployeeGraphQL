using EmployeeGraphQL.Infrastructure.Data;

public class ImportJobService
{
    private readonly AppDbContext _db;

    public ImportJobService(AppDbContext db) => _db = db;

    public async Task<ImportJob> CreateJob(string fileUrl, string projectId)
    {
        var job = new ImportJob
        {
            ImportJobId = Guid.NewGuid().ToString(),
            ProjectId = projectId,
            FileUrl = fileUrl,
            FileName = System.IO.Path.GetFileName(fileUrl),
            Status = "QUEUED",
            ProcessingMode = "ASYNC",   // ← REQUIRED FIX
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM",
            UpdatedBy = "SYSTEM"
        };

        _db.ImportJobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    public async Task UpdateStatus(string jobId, string status)
    {
        var job = await _db.ImportJobs.FindAsync(jobId);
        job.Status = status;
        job.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}