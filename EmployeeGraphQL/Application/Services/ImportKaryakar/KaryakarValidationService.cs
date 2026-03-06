public class KaryakarValidationService
{
    public RowValidationResult Validate(KaryakarCsvRow row)
    {
        if (string.IsNullOrWhiteSpace(row.MisBapsId))
            return RowValidationResult.Fail("MISBapsId is empty");

        if (!long.TryParse(row.MisBapsId, out _))
            return RowValidationResult.Fail("MISBapsId must be numeric");

        return RowValidationResult.Ok();
    }
}

public record RowValidationResult(bool IsValid, string Error)
{
    public static RowValidationResult Ok() => new(true, "");
    public static RowValidationResult Fail(string error) => new(false, error);
}