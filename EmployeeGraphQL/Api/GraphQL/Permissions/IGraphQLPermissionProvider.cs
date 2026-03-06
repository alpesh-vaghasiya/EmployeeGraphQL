public interface IGraphQLPermissionProvider
{
    Task<List<UserModuleAccessResponse>> GetPermissionsAsync(int userId, int positionId, int eventId);
}