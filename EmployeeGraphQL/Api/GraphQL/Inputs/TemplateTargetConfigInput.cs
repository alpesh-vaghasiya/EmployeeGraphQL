public class TemplateTargetConfigInput
{
    public string ConfigType { get; set; } = null!;   // "KARYAKAR" / "FAMILY"

    public bool WingMale { get; set; }

    public bool WingFemale { get; set; }

    public List<long>? CategoryIds { get; set; }      // Convert to JSONB

    public List<long>? MandalIds { get; set; }        // Convert to JSONB

    public int? FamiliesPairMin { get; set; }

    public int? FamiliesPairMax { get; set; }

    public bool BulkUploadKaryakar { get; set; }

    public bool BulkUploadFamily { get; set; }

    public bool BulkUploadAssignment { get; set; }
}