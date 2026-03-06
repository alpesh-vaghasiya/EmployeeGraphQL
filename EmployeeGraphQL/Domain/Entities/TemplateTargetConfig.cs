using System;
using System.Collections.Generic;

namespace EmployeeGraphQL.Domain.Entities;

public class TemplateTargetConfig
{
    public long TargetConfigId { get; set; }
    public Guid TenantConfigUucode { get; set; }

    public long TemplateId { get; set; }
    public Template Template { get; set; }

    public string ConfigType { get; set; } = null!;  // KARYAKAR / FAMILY

    public bool WingMale { get; set; }
    public bool WingFemale { get; set; }

    public string? CategoryIds { get; set; }      // JSONB (store JSON string)
    public string? MandalIds { get; set; }        // JSONB (store JSON string)

    public int? FamiliesPairMin { get; set; }
    public int? FamiliesPairMax { get; set; }

    public bool BulkUploadKaryakar { get; set; }
    public bool BulkUploadFamily { get; set; }
    public bool BulkUploadAssignment { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string Status { get; set; } = "ACTIVE";

}