using System;
using System.Collections.Generic;

namespace EmployeeGraphQL.Domain.Entities;

public partial class Template
{
    public long TemplateId { get; set; }

    public Guid TemplateUucode { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = "DRAFT";

    public long ProjectTypeId { get; set; }

    public long SamparkTypeId { get; set; }

    public string? LocationScopeIds { get; set; }

    public long? LocationLevelId { get; set; }

    public string? AllowedDraftProject { get; set; }

    public bool? DefaultProjectCreation { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? ProjectRepeateFrequencyConfig { get; set; }

    public int? ReminderValue { get; set; }

    public string? ReminderFrequencyConfig { get; set; }

    public bool? CustomReminder { get; set; }

    public bool? CustomDocument { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }
    public List<TemplateTargetConfig> TargetConfigs { get; set; } = new();
    public List<TemplateDepartmentConfig> DepartmentConfigs { get; set; } = new();
    public List<TemplateTargetSurvey> TargetSurveys { get; set; } = new();
    public List<TemplateDocument> Documents { get; set; } = new();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
