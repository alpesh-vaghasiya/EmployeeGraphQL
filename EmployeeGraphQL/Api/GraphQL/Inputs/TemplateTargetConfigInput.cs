public record TemplateTargetConfigInput(
    string ConfigType,                 // "KARYAKAR" / "FAMILY"
    bool WingMale,
    bool WingFemale,
    List<long>? CategoryIds,          // Convert to JSONB
    List<long>? MandalIds,            // Convert to JSONB
    int? FamiliesPairMin,
    int? FamiliesPairMax,
    bool BulkUploadKaryakar,
    bool BulkUploadFamily,
    bool BulkUploadAssignment
);