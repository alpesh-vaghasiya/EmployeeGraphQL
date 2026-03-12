using EmployeeGraphQL.Domain.Entities;

namespace Domain.Entities;

public class EmployeeProject
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public long ProjectId { get; set; }
    public Project? Project { get; set; }
}
