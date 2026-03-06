public class ImportRecord
{
    public long ImportRecordId { get; set; }
    public Guid ImportRecordUuCode { get; set; }
    public string? ImportJobId { get; set; }   // Must be string (varchar)
    public int? RowNumber { get; set; }
    public string? RecordData { get; set; }
    public bool? IsValid { get; set; }
    public string? ValidationErrors { get; set; }
    public bool? IsImported { get; set; }
    public DateTime? ImportedAt { get; set; }
    public string? ImportedEntityType { get; set; }
    public string? ImportedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public ImportJob ImportJob { get; set; }
}