using System;

namespace EmployeeGraphQL.Domain.Entities;

public class TemplateTargetSurvey
{
    public long TargetSurveyId { get; set; }
    public Guid TargetSurveyUucode { get; set; }

    public string ConfigType { get; set; } = null!; // KARYAKAR / FAMILY

    public long TemplateId { get; set; }
    public Template Template { get; set; }

    public string GssFormId { get; set; } = null!;

    public string? DepartmentIds { get; set; }   // JSONB
    public string? CategoryIds { get; set; }     // JSONB

    public bool IsRequired { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string Status { get; set; } = "ACTIVE";

}