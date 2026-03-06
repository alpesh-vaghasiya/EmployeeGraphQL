using HotChocolate.Resolvers;

namespace Api.GraphQL.Auth;

public class ModuleAuthorizationMiddleware
{
    private readonly FieldDelegate _next;

    public ModuleAuthorizationMiddleware(FieldDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(IMiddlewareContext context, IGraphQLPermissionProvider permissionProvider)
    {
        // ✅ read attribute from ContextData (works for extended types, stitched types, etc.)
        if (!context.Selection.Field.ContextData.TryGetValue("ModuleAuthorize", out var raw))
        {
            await _next(context);
            return;
        }

        var attr = raw as ModuleAuthorizeAttribute;
        if (attr == null)
        {
            await _next(context);
            return;
        }

        // ===== Permission Logic (same) =====
        var http = context.Services.GetRequiredService<IHttpContextAccessor>();
        var authProvider = context.Services.GetRequiredService<IGraphQLPermissionProvider>();

        var headers = http.HttpContext.Request.Headers;

        if (!headers.TryGetValue("X-App-Position", out var pos) ||
            !headers.TryGetValue("X-App-Event", out var evt))
        {
            throw new GraphQLException("Missing required headers");
        }

        int positionId = int.Parse(pos);
        int eventId = int.Parse(evt);
        int userId = 138422;

        var permissions = await authProvider.GetPermissionsAsync(userId, positionId, eventId);
        var accessList = permissions ?? new List<UserModuleAccessResponse>();

        bool matched = accessList.Any(a =>
            attr.ModuleCodes.Contains(a.ModuleCode) &&
            attr.Actions.Contains(a.ActionCode));

        if (!matched)
            throw new GraphQLException("Forbidden");

        await _next(context);
    }
}