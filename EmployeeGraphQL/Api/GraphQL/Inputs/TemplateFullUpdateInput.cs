public class TemplateFullUpdateInput
{
    public TemplateUpdateInput Template { get; set; } = null!;
    public List<TargetConfigUpdateInput>? TargetConfigs { get; set; }
    public List<DepartmentConfigUpdateInput>? DepartmentConfigs { get; set; }
    public List<TargetSurveyUpdateInput>? TargetSurveys { get; set; }
    public List<DocumentUpdateInput>? Documents { get; set; }
}