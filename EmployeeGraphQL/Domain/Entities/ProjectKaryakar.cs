using System;
using EmployeeGraphQL.Domain.Entities;

public class ProjectKaryakar
{
    public long ProjectKaryakarId { get; set; }
    public Guid ProjectKaryakarUucode { get; set; }
    public long ProjectId { get; set; }

    public long KaryakarPersonId { get; set; }

    public long? CategoryId { get; set; }
    public long? MandalId { get; set; }
    public long? DepartmentId { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    // ⭐ Navigation
    public Project Project { get; set; }
}