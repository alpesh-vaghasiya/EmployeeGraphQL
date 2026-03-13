public class TemplateInput
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int ProjectTypeId { get; set; }

    public int SamparkTypeId { get; set; }

    public string? LocationScopeIds { get; set; }

    public int? LocationLevelId { get; set; }

    public string? AllowedDraftProject { get; set; }

    public bool? DefaultProjectCreation { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    // public string? ProjectRepeateFrequencyConfig { get; set; }

    public ProjectFrequencyInput? ProjectRepeateFrequencyConfig { get; set; }
    public int? ReminderValue { get; set; }

    public string? ReminderFrequencyConfig { get; set; }

    public bool? CustomReminder { get; set; }

    public bool? CustomDocument { get; set; }

    public List<TemplateTargetConfigInput>? TargetConfigs { get; set; }

    public List<TemplateDepartmentConfigInput>? DepartmentConfigs { get; set; }

    public List<TemplateTargetSurveyInput>? TargetSurveys { get; set; }

    public List<TemplateDocumentInput>? Documents { get; set; }
}
public class ProjectFrequencyInput
{
    public string Type { get; set; } = null!;  // once | repeat | adhoc

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int CreateProjectTimes { get; set; }   // Create project 3 times

    public int? RepeatEvery { get; set; }         // 15 days / 1 week / 1 month

    public string? RepeatUnit { get; set; }       // days | weeks | months

    public int? DayOfMonth { get; set; }          // monthly start date

    public List<string>? DaysOfWeek { get; set; } // weekly monday friday

    public List<DateTime>? AdhocDates { get; set; }

    public int MinDurationDays { get; set; }

    public int MaxDurationDays { get; set; }
}