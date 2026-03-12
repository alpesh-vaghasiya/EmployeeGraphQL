public class ValidationSummaryDto
{
    public string ValidationToken { get; set; }

    public int TotalRecords { get; set; }

    public int ValidRecords { get; set; }

    public int InvalidRecords { get; set; }

    public List<SyncRowValidationResult> Results { get; set; }
}


public class SyncRowValidationResult
{
    public int RowNumber { get; set; }

    public string MisId { get; set; }

    public long ParsedMisId { get; set; }

    public bool IsValid { get; set; }

    public List<string> Errors { get; set; } = new();
}
public class SyncKaryakarCsvRow
{
    public int RowNumber { get; set; }

    public string MisId { get; set; }
}