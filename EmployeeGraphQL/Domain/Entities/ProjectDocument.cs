using System;
using EmployeeGraphQL.Domain.Entities;

public class ProjectDocument
{
    public string ProjectDocumentId { get; set; }
    public long ProjectId { get; set; }

    public string DocumentName { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentSfsId { get; set; }

    public string? DocumentType { get; set; }
    public long? FileSize { get; set; }

    public bool? IsCustomUpload { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    // ⭐ Navigation
    public Project Project { get; set; }
}