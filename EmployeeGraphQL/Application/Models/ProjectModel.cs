
public class ProjectResponse
{
    public long ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? SamparkType { get; set; }

    public int? KaryakarCount { get; set; }
    public int? FamilyCount { get; set; }

    public string? Departments { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? CreatedBy { get; set; }
    public string? Status { get; set; }
}