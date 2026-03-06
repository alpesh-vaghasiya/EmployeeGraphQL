public class DepartmentConfigUpdateInput
{
    public long? DepartmentConfigId { get; set; } // null = new
    public long DepartmentId { get; set; }
    public long? OwnerRoleId { get; set; }
    public bool IsPrimary { get; set; }
}