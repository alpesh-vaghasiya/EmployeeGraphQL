public class TargetSurveyUpdateInput
{
    public long? TargetSurveyId { get; set; }
    public string ConfigType { get; set; } = null!;
    public string GssFormId { get; set; } = null!;
    public List<long>? DepartmentIds { get; set; }
    public List<long>? CategoryIds { get; set; }
    public bool IsRequired { get; set; }
}