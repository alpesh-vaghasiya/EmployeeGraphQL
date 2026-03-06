public record TemplateUpdateInput(
    long TemplateId,
    string? Title,
    string? Description,
    string? Status,
    long? ProjectTypeId,
    long? SamparkTypeId,
    string? LocationScopeIds,
    long? LocationLevelId,
    string? AllowedDraftProject,
    bool? DefaultProjectCreation,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ProjectRepeateFrequencyConfig,
    int? ReminderValue,
    string? ReminderFrequencyConfig,
    bool? CustomReminder,
    bool? CustomDocument
);