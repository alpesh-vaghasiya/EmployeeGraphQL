public class TemplateTargetSurveyInput
{
    public string ConfigType { get; set; } = null!;   // "KARYAKAR" / "FAMILY"

    public string GssFormId { get; set; } = null!;

    public List<long>? DepartmentIds { get; set; }    // JSONB

    public List<long>? CategoryIds { get; set; }      // JSONB

    public bool IsRequired { get; set; }
}