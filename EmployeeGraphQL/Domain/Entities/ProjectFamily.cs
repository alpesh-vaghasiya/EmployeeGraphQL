using System;
using EmployeeGraphQL.Domain.Entities;

public class ProjectFamily
{
    public long ProjectFamilyId { get; set; }
    public Guid ProjectFamilyUucode { get; set; }
    public long ProjectId { get; set; }

    public string? PrimaryMemberName { get; set; }
    public string? PrimaryPersonId { get; set; }

    public string? CategoryId { get; set; }
    public string? MandalId { get; set; }
    public string? DepartmentId { get; set; }

    public long? AssignedKaryakarPairId { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    // ⭐ Navigation
    public Project Project { get; set; }
    public ProjectKaryakarPair? AssignedPair { get; set; }
}