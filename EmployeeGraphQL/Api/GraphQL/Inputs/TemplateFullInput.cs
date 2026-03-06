public record TemplateFullInput(
    TemplateInput Template,
    List<TemplateTargetConfigInput>? TargetConfigs,
    List<TemplateDepartmentConfigInput>? DepartmentConfigs,
    List<TemplateTargetSurveyInput>? TargetSurveys,
    List<TemplateDocumentInput>? Documents
);