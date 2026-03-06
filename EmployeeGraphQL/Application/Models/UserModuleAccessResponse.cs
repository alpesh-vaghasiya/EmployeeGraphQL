public class UserModuleAccessResponse
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = null!;
    public string ModuleCode { get; set; } = null!;
    public int ActionId { get; set; }
    public string ActionName { get; set; } = null!;
    public string ActionCode { get; set; } = null!;
    public List<int> DepartmentId { get; set; }
}
public class PositionViewModel
{
    public int PositionId { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? GeoLevelId { get; set; }
    public int? EntityId { get; set; }
    public string? EntityName { get; set; }
    public int? Occurance { get; set; }
    public int? PersonId { get; set; }
    public string? PersonFName { get; set; }
    public string? PersonMName { get; set; }
    public string? PersonLName { get; set; }
    public string? PersonOName { get; set; }
}
public class RolePositionModel
{
    public int RoleId { get; set; }
    public int PositionId { get; set; }
}