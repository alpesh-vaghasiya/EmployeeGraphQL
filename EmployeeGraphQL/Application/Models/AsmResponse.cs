public class AsmResponse
{
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public List<AsmRoleAccessItem> Data { get; set; }
}
public class AsmRoleAccessItem
{
    public int RoleId { get; set; }
    public int PositionId { get; set; }
    public List<RoleAccessViewModel> ApplicationAccess { get; set; }
}