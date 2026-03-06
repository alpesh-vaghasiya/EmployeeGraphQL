public class GraphQLPermissionProvider : IGraphQLPermissionProvider
{
    private readonly PersonService _personService;
    private readonly AsmPermissionService _asmService;

    public GraphQLPermissionProvider(PersonService personService, AsmPermissionService asmService)
    {
        _personService = personService;
        _asmService = asmService;
    }
    // STATIC SAMPLE PERMISSIONS FOR POC
    public async Task<List<UserModuleAccessResponse>> GetPermissionsAsync(int userId, int positionId, int eventId)
    {
        var accessPermissions = new List<UserModuleAccessResponse>();
        // Call your PersonService common MIS API method
        var userPositions = await _personService.GetPersonPosition(new int[] { userId });

        if (userPositions?.Any(f => f.PositionId == positionId) == true)
        {
            // Map positions to RolePositionModel
            var rolePositionModel = userPositions
                 .Where(d => d.PositionId == positionId)
                 .Select(t => new RolePositionModel
                 {
                     RoleId = t.RoleId ?? 0,
                     PositionId = t.PositionId
                 })
                 .ToList();

            if (rolePositionModel == null || rolePositionModel.Count == 0)
                return new List<UserModuleAccessResponse>();

            // Call ASM service to get access permissions
            var asmAccess = await _asmService.GetAllAccessByRolePositionId(rolePositionModel);

            if (asmAccess == null || asmAccess.Count == 0)
                return new List<UserModuleAccessResponse>();

            foreach (var permission in asmAccess)
            {
                // Action list based on permission flags
                var allowedActions = new List<(bool allowed, string action)>
            {
                (permission.HasViewAccess==true,   ModuleAction.View),
                (permission.HasCreateAccess==true, ModuleAction.Add),
                (permission.HasUpdateAccess==true, ModuleAction.Edit),
                (permission.HasDeleteAccess==true, ModuleAction.Delete)
            };

                // LOOP ONLY ON ALLOWED ACTIONS
                foreach (var (allowed, actionName) in allowedActions)
                {
                    if (!allowed) continue;

                    accessPermissions.Add(new UserModuleAccessResponse
                    {
                        ModuleCode = permission.ModuleCode,
                        ModuleId = permission.ModuleId,
                        ModuleName = permission.ModuleName,
                        ActionId = 1,
                        ActionName = actionName,
                        ActionCode = actionName,
                    });
                }
            }

        }
        return accessPermissions;
        // For now, return static permissions
        // return new List<UserModuleAccessResponse>
        // {
        //     new UserModuleAccessResponse
        //     {
        //         ModuleCode = "EventGroup",
        //         ActionCode = "View",
        //         DepartmentId = new List<int> { 1, 2, 3 }
        //     },
        //     new UserModuleAccessResponse
        //     {
        //         ModuleCode = "EventGroup",
        //         ActionCode = "Edit",
        //         DepartmentId = new List<int> { 1 }
        //     }
        // };
    }

}