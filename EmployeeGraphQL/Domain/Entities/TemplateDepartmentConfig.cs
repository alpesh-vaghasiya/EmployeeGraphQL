using System;

namespace EmployeeGraphQL.Domain.Entities;

public class TemplateDepartmentConfig
{
    public long DepartmentConfigId { get; set; }
    public Guid DepartmentConfigUucode { get; set; }

    public long TemplateId { get; set; }
    public Template Template { get; set; }

    public long DepartmentId { get; set; }
    public long? OwnerRoleId { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string Status { get; set; } = "ACTIVE";
}