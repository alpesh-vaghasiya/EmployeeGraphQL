using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

public class AuthRequestInterceptor : DefaultHttpRequestInterceptor
{
    public override async ValueTask OnCreateAsync(
        HttpContext context,
        IRequestExecutor requestExecutor,
        OperationRequestBuilder requestBuilder,
        CancellationToken cancellationToken)
    {
        var ssoService = context.RequestServices.GetRequiredService<ISsoService>();
        var redis = context.RequestServices.GetService<RedisCacheService>();

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
            return;
        }

        // Remove Bearer
        var token = authHeader.Replace("Bearer ", "");

        // Decode JWT
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Get uid from token
        var uid = jwt.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;

        if (string.IsNullOrEmpty(uid))
        {
            throw new GraphQLException(ErrorBuilder.New().SetMessage("Invalid Token").SetCode("AUTH_NOT_VALID").Build());
        }

        var cacheKey = $"TOKEN|{uid}";

        bool isValid = false;

        // Check Redis
        string? cachedToken = null;

        if (redis != null)
        {
            cachedToken = await redis.GetAsync(cacheKey);
        }

        if (!string.IsNullOrEmpty(cachedToken) && cachedToken == token)
        {
            isValid = true;
        }

        // Redis miss → call SSO
        if (!isValid)
        {
            isValid = await ssoService.ValidateToken(token);

            if (isValid)
            {
                var expClaim = jwt.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;

                if (long.TryParse(expClaim, out long expUnix))
                {
                    var expiry = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                    var remaining = expiry - DateTime.UtcNow;

                    if (remaining.TotalMinutes > 0 && redis != null)
                    {
                        await redis.SetAsync(cacheKey, token, remaining);
                    }
                }
            }
        }

        if (!isValid)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Invalid Token")
                    .SetCode("AUTH_NOT_VALID")
                    .Build());
        }

        // Set Claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, uid)
        };

        var identity = new ClaimsIdentity(claims, "SSO");
        context.User = new ClaimsPrincipal(identity);

        await base.OnCreateAsync(context, requestExecutor, requestBuilder, cancellationToken);
    }
}