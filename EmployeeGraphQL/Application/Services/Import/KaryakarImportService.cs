using EmployeeGraphQL.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class KaryakarImportService
{
    private readonly AppDbContext _db;

    public KaryakarImportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task InsertKaryakarAsync(long projectId, List<SyncRowValidationResult> records)
    {
        // 1️⃣ Remove duplicates from CSV
        var uniqueMisIds = records
            .Select(x => x.ParsedMisId)
            .Distinct()
            .ToList();

        // 2️⃣ Fetch existing from DB
        var existingIds = await _db.ProjectKaryakars.Where(x => x.ProjectId == projectId && uniqueMisIds.Contains(x.KaryakarPersonId)).Select(x => x.KaryakarPersonId).ToListAsync();

        var existingSet = existingIds.ToHashSet();

        var newEntities = new List<ProjectKaryakar>();

        foreach (var misId in uniqueMisIds)
        {
            if (existingSet.Contains(misId))
                continue;

            newEntities.Add(new ProjectKaryakar
            {
                ProjectKaryakarUucode = Guid.NewGuid(),
                ProjectId = projectId,
                KaryakarPersonId = misId,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (newEntities.Any())
        {
            _db.ProjectKaryakars.AddRange(newEntities);
            await _db.SaveChangesAsync();
        }
    }
}