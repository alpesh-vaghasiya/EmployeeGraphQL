public class ImportKaryakarResponse
{
    public bool IsAsync { get; set; }

    public string? JobId { get; set; }

    public ValidationSummaryDto? Validation { get; set; }
}