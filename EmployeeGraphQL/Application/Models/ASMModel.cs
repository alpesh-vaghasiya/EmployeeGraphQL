public class ASMModel
{
    public string? Url { get; set; }
    public string? ApplicationId { get; set; }
    public string? ApplicationSecurity { get; set; }
    public bool EnableAccessPermission { get; set; }
}
public class RoleAccessViewModel
{
    public long UserId { get; set; }
    public int RoleId { get; set; }
    public int PositionId { get; set; }
    public int ModuleId { get; set; }
    public string? ModuleName { get; set; }
    public string? ModuleCode { get; set; }
    public bool IsControlType { get; set; }
    public bool? HasViewAccess { get; set; }
    public bool? HasCreateAccess { get; set; }
    public bool? HasUpdateAccess { get; set; }
    public bool? HasDeleteAccess { get; set; }
    public bool? HasAccess { get; set; }
}
public class AccessRolePositionModel
{
    public string? ApplicationId { get; set; }
    public int PersonId { get; set; }
    public List<RolePositionModel>? Positions { get; set; }
}