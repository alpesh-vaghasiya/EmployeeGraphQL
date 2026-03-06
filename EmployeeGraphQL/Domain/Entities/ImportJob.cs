public class ImportJob
{
    public string ImportJobId { get; set; }
    public string ProjectId { get; set; }
    public string? ImportType { get; set; }
    public string? ProcessingMode { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public int? TotalRecords { get; set; }
    public int? ValidRecords { get; set; }
    public int? InvalidRecords { get; set; }
    public int? ImportedRecords { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Child records
    public ICollection<ImportRecord>? ImportRecords { get; set; }
}