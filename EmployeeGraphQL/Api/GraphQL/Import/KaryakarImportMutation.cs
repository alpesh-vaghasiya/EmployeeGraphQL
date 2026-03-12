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


    // STEP 1: VALIDATE CSV (SYNC MODE)
    public async Task<ValidationSummaryDto> ValidateKaryakarCsv(
        string fileUrl,
        [Service] CsvParserService parser,
        [Service] SyncKaryakarValidationService validator,
        [Service] RedisCacheService cache,
        [Service] IMisApiService misService)
    {
        // Parse CSV
        var rows = await parser.ParseAsync(fileUrl);

        if (rows.Count > 100)
            throw new Exception("SYNC mode supports only up to 100 records");

        var validationResults = new List<SyncRowValidationResult>();

        foreach (var row in rows)
        {
            var result = await validator.ValidateRowAsync(row);

            if (result.IsValid)
            {
                var misValid = await misService.ValidateMisId(row.MisId);

                if (!misValid)
                {
                    result.Errors.Add("MIS ID not found");
                    result.IsValid = false;
                }
            }

            validationResults.Add(result);
        }

        // Generate validation token
        var validationToken = Guid.NewGuid().ToString();

        // Store validation result in Redis
        await cache.StoreValidationAsync(validationToken, validationResults);

        // Return summary
        return new ValidationSummaryDto
        {
            ValidationToken = validationToken,
            TotalRecords = validationResults.Count,
            ValidRecords = validationResults.Count(x => x.IsValid),
            InvalidRecords = validationResults.Count(x => !x.IsValid),
            Results = validationResults
        };
    }

    // STEP 2: IMPORT VALIDATED RECORDS
    public async Task<string> ImportKaryakarCsvSync(
        string validationToken,
        [Service] RedisCacheService cache,
        [Service] KaryakarImportService importService)
    {
        // Fetch validation results from Redis
        var records = await cache.GetValidationAsync(validationToken);

        if (records == null || !records.Any())
            throw new Exception("Validation data not found or expired");

        // Filter valid records
        var validRecords = records.Where(x => x.IsValid).ToList();

        if (!validRecords.Any())
            return "No valid records to import";

        // Insert records
        await importService.InsertKaryakarAsync(validRecords);

        return $"IMPORT COMPLETED. Inserted: {validRecords.Count}";
    }
}