
public class EmployeeInput
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public decimal? Salary { get; set; }

    public int DepartmentId { get; set; }

    public List<long>? ProjectIds { get; set; }
}