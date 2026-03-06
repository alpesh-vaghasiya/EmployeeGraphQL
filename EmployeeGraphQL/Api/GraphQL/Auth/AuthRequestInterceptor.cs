using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using System.Security.Claims;

public class AuthRequestInterceptor : DefaultHttpRequestInterceptor
{
    public override async ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        var ssoService = context.RequestServices.GetRequiredService<ISsoService>();

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        // if (string.IsNullOrWhiteSpace(authHeader))
        // {
        //     throw new GraphQLException(ErrorBuilder.New().SetMessage("Invalid Token").SetCode("AUTH_NOT_VALID").Build());
        // }
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
            return;
        }

        var token = authHeader.Replace("Bearer ", "");

        var isValid = await ssoService.ValidateToken(token);

        if (!isValid)
        {
            throw new GraphQLException("Invalid Token");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "SSOUser")
        };

        var identity = new ClaimsIdentity(claims, "SSO");
        context.User = new ClaimsPrincipal(identity);

        await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}