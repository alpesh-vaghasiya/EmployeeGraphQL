using Microsoft.Extensions.Options;

public class AsmPermissionService
{
    private readonly IAsmApiService _asmService;
    private readonly ASMModel _asm;

    public AsmPermissionService(IAsmApiService asmService, IOptions<ASMModel> asm)
    {
        _asmService = asmService;
        _asm = asm.Value;
    }

    public async Task<List<RoleAccessViewModel>> GetAllAccessByRolePositionId(List<RolePositionModel> model)
    {
        var request = new AccessRolePositionModel
        {
            ApplicationId = _asm.ApplicationId,
            Positions = model
        };

        var asmResponse = await _asmService.PostAsync<AccessRolePositionModel, AsmResponse>(_asm.ApplicationSecurity, request);

        // Validate
        if (asmResponse == null || !asmResponse.Succeeded || asmResponse.Data == null || asmResponse.Data.Count == 0)
            return new List<RoleAccessViewModel>();

        var item = asmResponse.Data.First();

        // Attach Role + Position to each access
        foreach (var access in item.ApplicationAccess)
        {
            access.PositionId = item.PositionId;
            access.RoleId = item.RoleId;
        }

        return item.ApplicationAccess;
    }
}