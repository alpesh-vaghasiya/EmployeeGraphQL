public record TemplateTargetSurveyInput(
    string ConfigType,          // "KARYAKAR" / "FAMILY"
    string GssFormId,
    List<long>? DepartmentIds,  // JSONB
    List<long>? CategoryIds,    // JSONB
    bool IsRequired
);