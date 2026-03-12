public class TemplateDepartmentConfigInput
{
    public long DepartmentId { get; set; }

    public long? OwnerRoleId { get; set; }

    public bool IsPrimary { get; set; }
}