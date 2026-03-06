using Api.GraphQL;
using EmployeeGraphQL.Domain.Entities;
using EmployeeGraphQL.Infrastructure.Data;

[ExtendObjectType(typeof(Mutation))]
public class DepartmentImportMutation
{
    public async Task<string> ImportDepartmentCsv(
        string fileUrl,    // 👈 UI will pass fileUrl from SFS
        [Service] AppDbContext db,
        [Service] CsvService csvService,
        [Service] RedisStreamProducer producer)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return "❌ fileUrl is required.";

        // 1️⃣ UI already uploaded CSV → now download CSV temporarily to COUNT rows
        using var http = new HttpClient();
        var csvText = await http.GetStringAsync(fileUrl);

        using var reader = new StringReader(csvText);
        var rows = csvService.ReadDepartmentCsv(reader);

        // -------------------------
        // ⭐ CASE 1: SYNC IMPORT
        // -------------------------
        if (rows.Count <= 100)
        {
            foreach (var row in rows)
            {
                db.Departments.Add(new Department
                {
                    Name = row.Name
                });
            }

            await db.SaveChangesAsync();
            return $"✔ SYNC import completed. Inserted {rows.Count} rows.";
        }

        // -------------------------
        // ⭐ CASE 2: ASYNC IMPORT
        // -------------------------
        var jobId = Guid.NewGuid().ToString();

        // Push job to Redis WITH fileUrl
        await producer.PublishJobAsync(jobId, fileUrl);

        return $"⏳ ASYNC job queued. JobId={jobId}";
    }
}