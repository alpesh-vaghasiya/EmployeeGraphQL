using Api.Input;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/karyakar-import")]
public class KaryakarImportController : ControllerBase
{
    private readonly CsvParserService _parser;
    private readonly SyncKaryakarValidationService _validator;
    private readonly RedisCacheService _cache;
    private readonly IMisApiService _misService;
    private readonly ImportJobService _jobService;
    private readonly RedisStreamKaryakarProducer _producer;
    private readonly KaryakarImportService _importService;

    public KaryakarImportController(
        CsvParserService parser,
        SyncKaryakarValidationService validator,
        RedisCacheService cache,
        IMisApiService misService,
        ImportJobService jobService,
        RedisStreamKaryakarProducer producer,
        KaryakarImportService importService)
    {
        _parser = parser;
        _validator = validator;
        _cache = cache;
        _misService = misService;
        _jobService = jobService;
        _producer = producer;
        _importService = importService;
    }

    // STEP 1 : AUTO IMPORT
    [HttpPost("auto")]
    public async Task<IActionResult> ImportAuto([FromBody] ImportKaryakarRequest request)
    {
        var rows = await _parser.ParseAsync(request.FileUrl);

        if (rows.Count <= 100)
        {
            var validationResults = new List<SyncRowValidationResult>();

            foreach (var row in rows)
            {
                var result = await _validator.ValidateRowAsync(row);

                if (result.IsValid)
                {
                    var misValid = await _misService.ValidateMisId(row.MisId);

                    if (!misValid)
                    {
                        result.Errors.Add("MIS ID not found");
                        result.IsValid = false;
                    }
                }

                validationResults.Add(result);
            }

            var validationToken = Guid.NewGuid().ToString();

            await _cache.StoreValidationAsync(validationToken, validationResults);

            return Ok(new
            {
                isAsync = false,
                validationToken,
                totalRecords = validationResults.Count,
                validRecords = validationResults.Count(x => x.IsValid),
                invalidRecords = validationResults.Count(x => !x.IsValid),
                results = validationResults
            });
        }

        var job = await _jobService.CreateJob(request.FileUrl, "103");

        await _producer.PublishValidateJobAsync(
            job.ImportJobId,
            "103",
            "KARYAKAR",
            request.FileUrl,
            "SYSTEM"
        );

        return Ok(new
        {
            isAsync = true,
            jobId = job.ImportJobId
        });
    }
    // STEP 2 : IMPORT FROM TOKEN
    [HttpPost("import")]
    public async Task<IActionResult> ImportFromToken(
        string validationToken,
        long projectId)
    {
        var records = await _cache.GetValidationAsync(validationToken);

        if (records == null || !records.Any())
            return BadRequest("Validation data not found");

        var validRecords = records.Where(x => x.IsValid).ToList();

        if (!validRecords.Any())
            return BadRequest("No valid records");

        await _importService.InsertKaryakarAsync(projectId, validRecords);

        return Ok($"IMPORT COMPLETED. Inserted: {validRecords.Count}");
    }
}