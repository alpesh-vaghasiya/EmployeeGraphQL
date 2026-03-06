using System;
using EmployeeGraphQL.Domain.Entities;

public class ProjectKaryakarPair
{
    public long KaryakarPairId { get; set; }
    public Guid KaryakarPairUucode { get; set; }
    public long ProjectId { get; set; }

    public long PrimaryKaryakarPersonId { get; set; }
    public long? SecondaryKaryakarPersonId { get; set; }

    public string PairType { get; set; }
    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public Project Project { get; set; }
    public ICollection<ProjectFamily> Families { get; set; } = new List<ProjectFamily>();
}