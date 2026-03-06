public record TemplateInput(
    string Title,
    string? Description,
    long ProjectTypeId,
    long SamparkTypeId,
    string? LocationScopeIds,
    long? LocationLevelId,
    string AllowedDraftProject,
    bool? DefaultProjectCreation,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ProjectRepeateFrequencyConfig,
    int? ReminderValue,
    string? ReminderFrequencyConfig,
    bool? CustomReminder,
    bool? CustomDocument
);