using EmployeeGraphQL.Infrastructure.Data;

public class KaryakarImportService
{
    private readonly AppDbContext _db;

    public KaryakarImportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task InsertKaryakarAsync(List<SyncRowValidationResult> records)
    {
        foreach (var record in records)
        {
            var entity = new ProjectKaryakar
            {
                KaryakarPersonId = record.ParsedMisId
            };

            if (!_db.ProjectKaryakars.Any(x => x.KaryakarPersonId == record.ParsedMisId))
            {
                _db.ProjectKaryakars.Add(entity);
            }
        }

        await _db.SaveChangesAsync();
    }
}