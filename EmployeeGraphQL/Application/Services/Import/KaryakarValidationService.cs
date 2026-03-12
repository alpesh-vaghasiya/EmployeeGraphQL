public class SyncKaryakarValidationService
{
    public Task<SyncRowValidationResult> ValidateRowAsync(SyncKaryakarCsvRow row)
    {
        var result = new SyncRowValidationResult
        {
            RowNumber = row.RowNumber,
            MisId = row.MisId,
            IsValid = true
        };

        if (string.IsNullOrWhiteSpace(row.MisId))
        {
            result.Errors.Add("MIS ID required");
            result.IsValid = false;
            return Task.FromResult(result);
        }

        if (!long.TryParse(row.MisId, out var misId))
        {
            result.Errors.Add("MIS ID must be numeric");
            result.IsValid = false;
            return Task.FromResult(result);
        }

        result.ParsedMisId = misId;

        return Task.FromResult(result);
    }
}