public class TargetConfigUpdateInput
{
    public long? TargetConfigId { get; set; } // null = new record
    public string ConfigType { get; set; } = null!;
    public bool WingMale { get; set; }
    public bool WingFemale { get; set; }
    public List<long>? CategoryIds { get; set; }
    public List<long>? MandalIds { get; set; }
    public int? FamiliesPairMin { get; set; }
    public int? FamiliesPairMax { get; set; }
    public bool BulkUploadKaryakar { get; set; }
    public bool BulkUploadFamily { get; set; }
    public bool BulkUploadAssignment { get; set; }
}