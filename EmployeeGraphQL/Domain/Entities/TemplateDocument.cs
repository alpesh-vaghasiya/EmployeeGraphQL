using System;

namespace EmployeeGraphQL.Domain.Entities;

public class TemplateDocument
{
    public long DocumentId { get; set; }
    public Guid DocumentUucode { get; set; }

    public long TemplateId { get; set; }
    public Template Template { get; set; }

    public string DocumentName { get; set; } = null!;
    public string? DocumentUrl { get; set; }
    public string? DocumentSfsId { get; set; }
    public string? DocumentType { get; set; }
    public long? FileSize { get; set; }

    public bool IsOptional { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public string Status { get; set; } = "ACTIVE";

}